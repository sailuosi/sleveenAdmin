<template>
  <el-dialog
    v-model="visible"
    title="流程设计修复"
    width="640px"
    append-to-body
    destroy-on-close
    @closed="onClosed"
  >
    <el-form label-position="top" class="sf-repair-dialog">
      <el-form-item>
        <el-checkbox v-model="options.cleanOrphans">
          自动清理无首尾的连线，清理无效图元
        </el-checkbox>
      </el-form-item>

      <el-divider content-position="left">待办人规则</el-divider>
      <el-form-item>
        <el-checkbox v-model="options.fillAssignee">
          自动补全缺失的必填待办人规则（不覆盖已有 Type）
        </el-checkbox>
      </el-form-item>
      <template v-if="options.fillAssignee">
        <el-form-item label="Resolve Type（定范围）">
          <el-select v-model="options.resolveType" filterable style="width: 100%">
            <el-option
              v-for="o in resolveTypes"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="LeaveJudge Type（办理模式）">
          <el-select v-model="options.leaveType" filterable style="width: 100%">
            <el-option
              v-for="o in leaveTypes"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>
        <SailAssigneeParamsForm
          v-model="options.leaveParams"
          kind="leave"
          :type="options.leaveType"
        />
      </template>

      <el-divider content-position="left">其他问题</el-divider>
      <el-form-item>
        <el-checkbox v-model="options.fillButtons">
          补全缺失的办理按钮设置（ButtonsSetting）
        </el-checkbox>
      </el-form-item>
      <el-form-item>
        <el-checkbox v-model="options.renameEvents">
          规范化起止事件默认名称（开始 / 结束）
        </el-checkbox>
      </el-form-item>

      <el-alert
        v-if="previewSummary"
        type="info"
        :closable="false"
        show-icon
        class="sf-repair-dialog__preview"
        :title="`预览将执行：${previewSummary}`"
      />
      <el-alert
        v-if="previewRemaining.length"
        type="warning"
        :closable="false"
        show-icon
        class="sf-repair-dialog__preview"
        title="以下问题无法自动修复，需手动调整"
      >
        <ul class="sf-repair-dialog__list">
          <li v-for="(msg, i) in previewRemaining" :key="i">{{ msg }}</li>
        </ul>
      </el-alert>
      <el-alert
        v-else-if="previewReady && !previewRemaining.length"
        type="success"
        :closable="false"
        show-icon
        class="sf-repair-dialog__preview"
        title="扫描未发现需手修的结构性问题"
      />
    </el-form>

    <template #footer>
      <el-button :disabled="running" @click="refreshPreview" :loading="scanning">刷新预览</el-button>
      <el-button :disabled="running" @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="running" @click="onApply">执行修复</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { LEAVE_TYPE_OPTIONS, RESOLVE_TYPE_OPTIONS } from '@sail/bpmn/assignee/types';
import {
  defaultRepairOptions,
  repairProcessDesign,
  summarizeApplied,
  type RepairProcessOptions
} from '@sail/bpmn/utils/repairProcessDesign';
import SailAssigneeParamsForm from '../assignee/SailAssigneeParamsForm.vue';

defineOptions({ name: 'SailProcessRepairDialog' });

const visible = ref(false);
const running = ref(false);
const scanning = ref(false);
const previewReady = ref(false);
const previewSummary = ref('');
const previewRemaining = ref<string[]>([]);
const options = reactive<RepairProcessOptions>(defaultRepairOptions());

const resolveTypes = RESOLVE_TYPE_OPTIONS;
const leaveTypes = LEAVE_TYPE_OPTIONS;

let getXml: (() => Promise<string>) | null = null;
let applyXml: ((xml: string) => Promise<void>) | null = null;
let previewTimer: ReturnType<typeof setTimeout> | null = null;

function resetOptions() {
  Object.assign(options, defaultRepairOptions());
}

function open(payload: {
  getXml: () => Promise<string>;
  applyXml: (xml: string) => Promise<void>;
}) {
  getXml = payload.getXml;
  applyXml = payload.applyXml;
  resetOptions();
  previewReady.value = false;
  previewSummary.value = '';
  previewRemaining.value = [];
  visible.value = true;
  void refreshPreview();
}

function onClosed() {
  getXml = null;
  applyXml = null;
  previewReady.value = false;
  previewSummary.value = '';
  previewRemaining.value = [];
}

async function refreshPreview() {
  if (!getXml) return;
  scanning.value = true;
  try {
    const xml = await getXml();
    const result = repairProcessDesign(xml, { ...options });
    previewSummary.value = summarizeApplied(result.applied);
    previewRemaining.value = result.remaining;
    previewReady.value = true;
  } catch (e: unknown) {
    previewSummary.value = '';
    previewRemaining.value = [e instanceof Error ? e.message : String(e)];
    previewReady.value = true;
  } finally {
    scanning.value = false;
  }
}

function schedulePreview() {
  if (!visible.value) return;
  if (previewTimer) clearTimeout(previewTimer);
  previewTimer = setTimeout(() => {
    void refreshPreview();
  }, 280);
}

watch(
  () => ({
    cleanOrphans: options.cleanOrphans,
    fillAssignee: options.fillAssignee,
    resolveType: options.resolveType,
    leaveType: options.leaveType,
    leaveParams: options.leaveParams,
    fillButtons: options.fillButtons,
    renameEvents: options.renameEvents
  }),
  () => schedulePreview(),
  { deep: true }
);

async function onApply() {
  if (!getXml || !applyXml) return;
  running.value = true;
  try {
    const xml = await getXml();
    const result = repairProcessDesign(xml, { ...options });
    const changed =
      result.applied.removedFlows.length ||
      result.applied.removedDi.length ||
      result.applied.filledAssignee.length ||
      result.applied.filledButtons.length ||
      result.applied.renamedEvents.length;
    if (changed) {
      await applyXml(result.xml);
    }
    const summary = summarizeApplied(result.applied);
    if (result.remaining.length) {
      ElMessage.warning(`${summary}。仍有 ${result.remaining.length} 项需手修，建议再点「校验」`);
    } else {
      ElMessage.success(`${summary}。建议再点「校验」确认`);
    }
    previewSummary.value = summary;
    previewRemaining.value = result.remaining;
    previewReady.value = true;
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    running.value = false;
  }
}

onMounted(() => {
  designerEvents.on('repair:open', open);
});
onBeforeUnmount(() => {
  designerEvents.off('repair:open', open);
  if (previewTimer) clearTimeout(previewTimer);
});
</script>

<style scoped>
.sf-repair-dialog__preview {
  margin-top: 8px;
}
.sf-repair-dialog__list {
  margin: 6px 0 0;
  padding-left: 18px;
}
</style>
