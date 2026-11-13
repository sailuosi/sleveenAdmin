<template>
  <div class="sf-process-design">
    <el-alert
      v-if="!model.processId"
      type="warning"
      show-icon
      :closable="false"
      title="请先保存基本信息，生成流程定义后再设计 BPMN"
      class="sf-process-design__alert"
    />
    <template v-else>
      <div class="sf-process-design__bar">
        <el-tag :type="model.hasBpmn ? 'success' : 'info'" size="small">
          {{ model.hasBpmn ? '已有流程图' : '尚未设计（有默认开始节点）' }}
        </el-tag>
        <el-button @click="openDesignerBpmnJs">用 bpmn-js 设计器打开</el-button>
        <el-button @click="refreshFlag">刷新状态</el-button>
      </div>
      <div class="sf-process-design__vue">
        <SailDesignerVuePage
          :key="vuePageKey"
          ref="vueDesignerRef"
          embedded
          :show-back="false"
          :edit-process-id="String(model.processId)"
          :edit-version="String(model.version || '1')"
          @saved="onDesignerSaved"
        />
      </div>
      <SailDesignerDialog ref="bpmnJsDialogRef" @saved="onDesignerSaved" />
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { queryProcessFile } from '@sail/api/process';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import SailDesignerVuePage from '@sail/views/SailDesignerVuePage.vue';
import SailDesignerDialog from '@sail/components/SailDesignerDialog.vue';
import type { ProcessEditFormModel } from './types';

defineOptions({ name: 'SailProcessDesignStep' });

const model = defineModel<ProcessEditFormModel>({ required: true });

const vueDesignerRef = ref<{ save: () => Promise<boolean> } | null>(null);
const bpmnJsDialogRef = ref<{ open: (opts?: { processId?: string; version?: string }) => void } | null>(
  null,
);

const vuePageKey = computed(
  () => `${model.value.processId || ''}@${model.value.version || '1'}`,
);

function openDesignerBpmnJs() {
  if (!model.value.processId) return;
  bpmnJsDialogRef.value?.open({
    processId: String(model.value.processId),
    version: String(model.value.version || '1'),
  });
}

function onDesignerSaved() {
  model.value.hasBpmn = true;
  void refreshFlag(false);
}

async function refreshFlag(toast = true) {
  if (!model.value.processId) return;
  try {
    const data = await queryProcessFile({
      ProcessId: model.value.processId,
      Version: model.value.version || '1',
    });
    const r = unpackSailResult<Record<string, unknown>>(data);
    const xml = String(pickField(r.entity, 'XmlContent', 'xmlContent') || '').trimEnd();
    model.value.hasBpmn = xml.includes('<process') || xml.includes('<bpmn');
    if (toast) {
      ElMessage.success(model.value.hasBpmn ? '已检测到流程图' : '仍为初始模板');
    }
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

/** 嵌入 Vue 设计器保存 BPMN，再由外层写元数据 */
async function saveDesigner(): Promise<boolean> {
  if (!model.value.processId) return true;
  const ok = await vueDesignerRef.value?.save?.();
  if (ok === false) return false;
  model.value.hasBpmn = true;
  return true;
}

async function validate() {
  if (!model.value.processId) {
    throw new Error('请先保存流程基本信息');
  }
  const ok = await saveDesigner();
  if (!ok) throw new Error('请先在设计器中成功保存流程图');
  await refreshFlag();
}

defineExpose({ validate, refresh: refreshFlag, saveDesigner });
</script>

<style scoped>
.sf-process-design {
  min-height: calc(100vh - 140px);
  display: flex;
  flex-direction: column;
}
.sf-process-design__alert {
  margin: 12px auto;
  max-width: 560px;
}
.sf-process-design__bar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  flex: 0 0 auto;
}
.sf-process-design__vue {
  flex: 1 1 auto;
  min-height: calc(100vh - 220px);
  border: 1px solid #ebeef5;
  border-radius: 4px;
  overflow: hidden;
  background: #fff;
  display: flex;
  flex-direction: column;
}
.sf-process-design__vue :deep(.sf-designer-page) {
  height: 100%;
  min-height: 0;
}
</style>
