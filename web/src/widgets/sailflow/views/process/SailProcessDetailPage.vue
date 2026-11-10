<template>
  <div class="sf-detail-page" v-loading="loading">
    <el-alert
      v-if="err"
      :title="err"
      type="error"
      show-icon
      :closable="false"
      class="sf-detail-page__alert sf-no-print"
    />

    <div class="sf-detail-page__main">
      <el-card shadow="never" class="sf-detail-page__card">
        <div class="sf-detail-page__head">
          <div class="sf-detail-page__head-top sf-no-print">
            <span class="sf-detail-page__sn">
              编号：{{ field(task || processInstance, 'AppInstanceId', 'appInstanceId') || '-' }}
            </span>
            <div class="sf-detail-page__head-actions">
              <el-button link :loading="loading" @click="load">刷新</el-button>
              <el-button
                link
                type="warning"
                :loading="nextTreeLoading"
                :disabled="!task"
                @click="probeNextActivityTree"
              >
                调试下一跳
              </el-button>
              <el-button link @click="onPrint">打印</el-button>
              <el-button link @click="router.back()">返回</el-button>
            </div>
          </div>
          <el-divider class="sf-detail-page__divider" />
          <div class="sf-detail-page__title-row">
            <h1 class="sf-detail-page__title">
              {{
                field(processInstance, 'ProcessName', 'processName') ||
                field(task || processInstance, 'AppName', 'appName') ||
                '流程办理'
              }}
            </h1>
            <el-tag :type="processStateTag.type" size="small" effect="light">
              {{ processStateTag.label }}
            </el-tag>
          </div>
          <div class="sf-detail-page__meta">
            <div class="sf-detail-page__starter">
              <el-avatar :size="28" class="sf-detail-page__avatar">
                {{ starterInitial }}
              </el-avatar>
              <span>{{ field(processInstance, 'CreatedUserName', 'createdUserName') || '-' }}</span>
            </div>
            <span class="sf-detail-page__meta-muted">
              {{ formatTime(field(processInstance, 'CreatedDateTime', 'createdDateTime')) }} 提交
            </span>
            <span
              v-if="field(task, 'ActivityName', 'activityName')"
              class="sf-detail-page__meta-muted"
            >
              当前节点 · {{ field(task, 'ActivityName', 'activityName') }}
            </span>
          </div>
        </div>

        <el-tabs v-model="activeTab" class="sf-detail-page__tabs">
          <el-tab-pane label="审批详情" name="form">
            <div class="sf-detail-page__pane">
              <SailFormRender
                v-if="formConf || formFieldsJson"
                ref="formRef"
                v-model="formValues"
                :conf="formConf"
                :fields-json="formFieldsJson"
                :permissions="fieldPermissions"
                :field-modes="fieldModes"
                :title="formTitle || '业务表单'"
                :disabled="!canHandle"
              />
              <el-empty v-else description="未绑定动态表单" :image-size="72" />

              <div
                v-if="ruleSetCodes.length || aiConfigUuids.length"
                class="sf-detail-page__refs sf-no-print"
              >
                <span class="sf-detail-page__refs-label">引用</span>
                <el-tag
                  v-for="code in ruleSetCodes"
                  :key="`r-${code}`"
                  type="success"
                  effect="plain"
                  class="sf-detail-page__ref-tag"
                  @click="router.push({ path: '/sail/rulesets', query: { code } })"
                >
                  规则 {{ code }}
                </el-tag>
                <el-tag
                  v-for="uuid in aiConfigUuids"
                  :key="`a-${uuid}`"
                  type="warning"
                  effect="plain"
                  class="sf-detail-page__ref-tag"
                  @click="router.push({ path: '/sail/ai-models', query: { configUUID: uuid } })"
                >
                  AI {{ uuid }}
                </el-tag>
              </div>
            </div>
          </el-tab-pane>

          <el-tab-pane label="流转记录" name="record">
            <div class="sf-detail-page__pane">
              <el-timeline v-if="activityInstances.length">
                <el-timeline-item
                  v-for="a in activityInstances"
                  :key="String(field(a, 'ActivityInstanceId', 'activityInstanceId', 'Id', 'id'))"
                  :timestamp="formatTime(field(a, 'CreatedDateTime', 'createdDateTime'))"
                  placement="top"
                  :type="activityTimelineType(a)"
                >
                  <div class="sf-detail-page__tl-title">
                    {{
                      field(a, 'ActivityName', 'activityName') ||
                      field(a, 'ActivityId', 'activityId')
                    }}
                    <el-tag size="small" effect="plain">{{ activityStateLabel(a) }}</el-tag>
                  </div>
                  <div class="sf-detail-page__tl-sub">
                    <span v-if="field(a, 'AssignedUserNames', 'assignedUserNames')">
                      办理人 {{ field(a, 'AssignedUserNames', 'assignedUserNames') }}
                    </span>
                    <span v-if="field(a, 'EndedUserName', 'endedUserName')">
                      · 完成人 {{ field(a, 'EndedUserName', 'endedUserName') }}
                    </span>
                    <span v-if="field(a, 'EndedDateTime', 'endedDateTime')">
                      · {{ formatTime(field(a, 'EndedDateTime', 'endedDateTime')) }}
                    </span>
                  </div>
                </el-timeline-item>
              </el-timeline>
              <el-empty v-else description="暂无活动记录" :image-size="72" />
            </div>
          </el-tab-pane>
        </el-tabs>
      </el-card>

      <div
        v-if="
          activeTab === 'form' &&
          task &&
          (canHandle || canWithdraw || canResend || canClose || canJump)
        "
        class="sf-detail-page__footer sf-no-print"
      >
        <SailOperationBar
          :task="task"
          :process-instance="processInstance"
          :next-nodes="nextNodes"
          :previous-nodes="previousNodes"
          :form-values="formValues"
          :field-modes="fieldModes"
          :form-ref="formRef"
          :can-handle="canHandle"
          :show-approve="showApproveByPolicy"
          :show-withdraw="canWithdraw"
          :show-resend="canResend"
          :show-close="canClose"
          :show-jump="canJump"
          :has-back-src="hasBackSrc"
          :buttons-setting="buttonsSetting"
          @done="onDone"
        />
      </div>
    </div>

    <el-dialog
      v-model="nextTreeVisible"
      title="GetNextActivityRoleUserTree 调试"
      width="720px"
      append-to-body
      destroy-on-close
      class="sf-no-print"
    >
      <el-alert
        :title="nextTreeSummary"
        :type="nextTreeOk ? 'success' : 'error'"
        show-icon
        :closable="false"
        class="sf-detail-page__next-tree-alert"
      />
      <pre class="sf-detail-page__next-tree-pre">{{ nextTreeJson }}</pre>
      <template #footer>
        <el-button
          v-if="nextTreeOk && probedNextNodes.length"
          type="primary"
          @click="applyProbedNextNodes"
        >
          应用到本页办理栏（{{ probedNextNodes.length }}）
        </el-button>
        <el-button @click="nextTreeVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { getApprovalWorkbench, getNextActivityRoleUserTree } from '@sail/api/runtime';
import { queryProcessFile } from '@sail/api/process';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import { formatTime } from '@sail/utils/formatTime';
import { parseProcessAssets } from '@sail/utils/parseProcessAssets';
import { parseButtonsSettingForActivity } from '@sail/utils/parseButtonsSetting';
import {
  defaultButtonsSetting,
  mergeButtonsSetting,
  type SailButtonSetting
} from '@sail/bpmn/buttonsCatalog';
import SailOperationBar from '@sail/components/runtime/SailOperationBar.vue';
import SailFormRender from '@sail/components/runtime/SailFormRender.vue';
import { filterFormValuesByFields } from '@sail/utils/formCreate';

defineOptions({ name: 'SailProcessDetailPage' });

const route = useRoute();
const router = useRouter();
const { userId, userName } = useSailCurrentUser();

const loading = ref(false);
const err = ref('');
const activeTab = ref('form');
const task = ref<Record<string, unknown> | null>(null);
const processInstance = ref<Record<string, unknown> | null>(null);
const nextNodes = ref<Record<string, unknown>[]>([]);
const previousNodes = ref<Record<string, unknown>[]>([]);
const nextTreeLoading = ref(false);
const nextTreeVisible = ref(false);
const nextTreeOk = ref(false);
const nextTreeSummary = ref('');
const nextTreeJson = ref('');
const probedNextNodes = ref<Record<string, unknown>[]>([]);
const activityInstances = ref<Record<string, unknown>[]>([]);
const formConf = ref('');
const formFieldsJson = ref('');
const formTitle = ref('');
const formValues = ref<Record<string, unknown>>({});
const fieldPermissions = ref<any[]>([]);
/** §C：field → editable|readonly|hidden|masked */
const fieldModes = ref<Record<string, string>>({});
const formRef = ref<InstanceType<typeof SailFormRender> | null>(null);
const ruleSetCodes = ref<string[]>([]);
const aiConfigUuids = ref<string[]>([]);
const buttonsSetting = ref<SailButtonSetting[]>(defaultButtonsSetting());
/** §G：当前活动带退回源 → 可返送 */
const hasBackSrc = ref(false);
/** §J 维②：ForwardNormal | ReturnToSender */
const resubmitMode = ref<'ForwardNormal' | 'ReturnToSender'>('ReturnToSender');

const PROCESS_STATE: Record<number, { label: string; type: 'info' | 'success' | 'warning' | 'danger' }> = {
  0: { label: '未启动', type: 'info' },
  1: { label: '就绪', type: 'info' },
  2: { label: '运行中', type: 'warning' },
  4: { label: '已完成', type: 'success' },
  5: { label: '已挂起', type: 'info' },
  6: { label: '已取消', type: 'danger' }
};

const ACTIVITY_STATE: Record<number, string> = {
  1: '待办',
  2: '办理中',
  4: '已完成',
  5: '已挂起',
  6: '已撤销',
  7: '已退回'
};

const canHandle = computed(() => {
  if (!task.value) return false;
  const assigned = String(pickField(task.value, 'AssignedUserId', 'assignedUserId') || '');
  const state = Number(pickField(task.value, 'ActivityState', 'activityState') || 0);
  return assigned === userId.value && (state === 1 || state === 2);
});

const processRunning = computed(() => {
  const st = Number(pickField(processInstance.value || {}, 'ProcessState', 'processState') || 0);
  return st === 2;
});

const processStateTag = computed(() => {
  const st = Number(pickField(processInstance.value || {}, 'ProcessState', 'processState') || 0);
  return PROCESS_STATE[st] || { label: `状态 ${st}`, type: 'info' as const };
});

const canWithdraw = computed(() => {
  if (!task.value || canHandle.value) return false;
  const ended = String(pickField(task.value, 'EndedUserId', 'endedUserId') || '');
  const taskState = Number(pickField(task.value, 'TaskState', 'taskState') || 0);
  return ended === userId.value && taskState === 4 && processRunning.value;
});

/**
 * §G/§J：有 BackSrc 且策略允许返送时露出 Resend。
 * ReturnToSender → 突出返送；ForwardNormal → 默认不露 Resend（仍可管理例外，此处收紧）。
 */
const canResend = computed(() => {
  if (!hasBackSrc.value) return false;
  if (resubmitMode.value === 'ForwardNormal' && canHandle.value) return false;
  return (
    (canHandle.value && resubmitMode.value === 'ReturnToSender') ||
    (canWithdraw.value && hasBackSrc.value)
  );
});

/** ForwardNormal + BackSrc：正常发送；ReturnToSender：仍可显示通过作管理例外？——互斥时默认只返送 */
const showApproveByPolicy = computed(() => {
  if (!canHandle.value) return false;
  if (!hasBackSrc.value) return true;
  return resubmitMode.value === 'ForwardNormal';
});
const canClose = computed(() => processRunning.value && !!task.value);
const canJump = computed(() => processRunning.value && !!task.value);

const starterInitial = computed(() => {
  const name = String(field(processInstance.value, 'CreatedUserName', 'createdUserName') || '');
  return name ? name.slice(0, 1) : '?';
});

function field(row: Record<string, unknown> | null | undefined, ...keys: string[]) {
  return pickField(row || undefined, ...keys) ?? '';
}

function activityStateLabel(a: Record<string, unknown>) {
  const st = Number(field(a, 'ActivityState', 'activityState') || 0);
  return ACTIVITY_STATE[st] || `状态 ${st}`;
}

function activityTimelineType(a: Record<string, unknown>) {
  const st = Number(field(a, 'ActivityState', 'activityState') || 0);
  if (st === 4) return 'success';
  if (st === 1 || st === 2) return 'primary';
  if (st === 6 || st === 7) return 'warning';
  return 'info';
}

/** 直接调 GetNextActivityRoleUserTree，便于对照 workbench 被吞掉的异常/空列表 */
async function probeNextActivityTree() {
  if (!task.value) {
    ElMessage.warning('当前无待办任务');
    return;
  }
  nextTreeLoading.value = true;
  try {
    const t = task.value;
    const data = await getNextActivityRoleUserTree({
      TaskId: String(pickField(t, 'TaskId', 'taskId', 'Id', 'id') || ''),
      ProcessId: String(pickField(t, 'ProcessId', 'processId') || ''),
      Version: String(pickField(t, 'Version', 'version') || '1'),
      AppInstanceId: String(pickField(t, 'AppInstanceId', 'appInstanceId') || ''),
      AppName: String(pickField(t, 'AppName', 'appName') || ''),
      UserId: userId.value,
      UserName: userName.value,
      Conditions: Object.fromEntries(
        Object.entries(formValues.value || {}).map(([k, v]) => [k, v == null ? '' : String(v)])
      )
    });
    const status = data?.Status ?? data?.status;
    const msg = String(data?.Message ?? data?.message ?? '');
    const entity = data?.Entity ?? data?.entity ?? data;
    const list = Array.isArray(entity)
      ? (entity as Record<string, unknown>[])
      : Array.isArray((entity as any)?.StepList)
        ? ((entity as any).StepList as Record<string, unknown>[])
        : [];
    probedNextNodes.value = list;
    nextTreeOk.value = status !== -1 && status !== '-1';
    nextTreeSummary.value = nextTreeOk.value
      ? `成功：共 ${list.length} 个下一跳节点`
      : `失败：${msg || '未知错误'}`;
    nextTreeJson.value = JSON.stringify(
      {
        status,
        message: msg || null,
        count: list.length,
        entity: entity
      },
      null,
      2
    );
    nextTreeVisible.value = true;
  } catch (e: unknown) {
    nextTreeOk.value = false;
    probedNextNodes.value = [];
    nextTreeSummary.value = e instanceof Error ? e.message : String(e);
    nextTreeJson.value = nextTreeSummary.value;
    nextTreeVisible.value = true;
  } finally {
    nextTreeLoading.value = false;
  }
}

function applyProbedNextNodes() {
  nextNodes.value = [...probedNextNodes.value];
  nextTreeVisible.value = false;
  ElMessage.success(`已写入办理栏 nextNodes（${nextNodes.value.length}）`);
}

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const taskId = route.query.taskId
      ? String(route.query.taskId).trim() || null
      : null;
    const processInstanceId = route.query.processInstanceId
      ? String(route.query.processInstanceId).trim() || null
      : null;
    if (!taskId && !processInstanceId) {
      err.value = '缺少 taskId 或 processInstanceId';
      return;
    }
    const data = await getApprovalWorkbench({
      TaskId: taskId,
      ProcessInstanceId: processInstanceId,
      FormId: route.query.formId ? String(route.query.formId) : undefined
    });
    const r = unpackSailResult<Record<string, unknown>>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '加载失败';
      return;
    }
    const entity = r.entity || {};
    task.value = (pickField(entity, 'Task', 'task') as Record<string, unknown>) || null;
    processInstance.value =
      (pickField(entity, 'ProcessInstance', 'processInstance') as Record<string, unknown>) || null;
    const backSrcId = String(
      pickField(entity, 'BackSrcActivityInstanceId', 'backSrcActivityInstanceId') || ''
    ).trim();
    hasBackSrc.value = !!(
      pickField(entity, 'HasBackSrc', 'hasBackSrc') === true ||
      pickField(entity, 'HasBackSrc', 'hasBackSrc') === 'true' ||
      backSrcId !== ''
    );
    const mode = String(
      pickField(entity, 'ResubmitMode', 'resubmitMode') || 'ReturnToSender'
    );
    resubmitMode.value =
      mode === 'ForwardNormal' ? 'ForwardNormal' : 'ReturnToSender';
    nextNodes.value =
      (pickField<Record<string, unknown>[]>(entity, 'NextNodes', 'nextNodes') as Record<
        string,
        unknown
      >[]) || [];
    previousNodes.value =
      (pickField<Record<string, unknown>[]>(entity, 'PreviousNodes', 'previousNodes') as Record<
        string,
        unknown
      >[]) || [];
    activityInstances.value =
      (pickField<Record<string, unknown>[]>(entity, 'ActivityInstances', 'activityInstances') as Record<
        string,
        unknown
      >[]) || [];
    const formMeta = (pickField(entity, 'FormConf', 'formConf') as Record<string, unknown>) || null;
    if (formMeta) {
      formConf.value = String(pickField(formMeta, 'conf', 'Conf') || '');
      formFieldsJson.value = String(pickField(formMeta, 'fieldsJson', 'FieldsJson') || '');
      formTitle.value = String(pickField(formMeta, 'formName', 'FormName') || '');
    } else {
      formConf.value = '';
      formFieldsJson.value = '';
      formTitle.value = '';
    }
    // 回填发起/办理写入的流程变量（按表单字段白名单过滤，对齐 szkfpt）
    const valuesRaw = pickField(entity, 'FormValues', 'formValues');
    if (valuesRaw && typeof valuesRaw === 'object' && !Array.isArray(valuesRaw)) {
      const parsed: Record<string, unknown> = {};
      Object.entries(valuesRaw as Record<string, unknown>).forEach(([k, v]) => {
        if (v == null) return;
        const s = String(v);
        // Conditions 落库时非 string 用 JSON.stringify；回填尝试还原
        if (
          (s.startsWith('{') && s.endsWith('}')) ||
          (s.startsWith('[') && s.endsWith(']')) ||
          s === 'true' ||
          s === 'false' ||
          /^-?\d+(\.\d+)?$/.test(s)
        ) {
          try {
            parsed[k] = JSON.parse(s);
            return;
          } catch {
            /* keep string */
          }
        }
        parsed[k] = s;
      });
      formValues.value = filterFormValuesByFields(parsed, formFieldsJson.value);
    } else {
      formValues.value = {};
    }
    fieldPermissions.value =
      (pickField(entity, 'FieldPermissions', 'fieldPermissions') as any[]) || [];
    const modesRaw = pickField(entity, 'FieldModes', 'fieldModes');
    fieldModes.value =
      modesRaw && typeof modesRaw === 'object' && !Array.isArray(modesRaw)
        ? (modesRaw as Record<string, string>)
        : {};
    const rawButtons = pickField<SailButtonSetting[]>(entity, 'ButtonsSetting', 'buttonsSetting');
    buttonsSetting.value = Array.isArray(rawButtons)
      ? mergeButtonsSetting(rawButtons)
      : defaultButtonsSetting();
    await loadProcessAssets();
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  } finally {
    loading.value = false;
  }
}

async function loadProcessAssets() {
  ruleSetCodes.value = [];
  aiConfigUuids.value = [];
  const processId = String(
    field(processInstance.value, 'ProcessId', 'processId') ||
      field(task.value, 'ProcessId', 'processId') ||
      ''
  );
  if (!processId) return;
  const version = String(
    field(processInstance.value, 'Version', 'version') ||
      field(task.value, 'Version', 'version') ||
      '1'
  );
  try {
    const data = await queryProcessFile({ ProcessId: processId, Version: version });
    if ((data?.Status ?? data?.status) === -1) return;
    const file = (data?.Entity ?? data?.entity ?? data) as Record<string, unknown>;
    const xml = String(pickField(file, 'XmlContent', 'xmlContent') || '');
    const refs = parseProcessAssets(xml);
    ruleSetCodes.value = refs.ruleSetCodes;
    aiConfigUuids.value = refs.aiConfigUuids;
    const activityId = String(field(task.value, 'ActivityId', 'activityId') || '');
    if (activityId && xml) {
      buttonsSetting.value = parseButtonsSettingForActivity(xml, activityId);
    }
  } catch {
    /* ignore */
  }
}

function onDone() {
  router.push({ path: '/sail/task-todo' });
}

function onPrint() {
  window.print();
}

watch(() => route.query, load, { immediate: true });
</script>

<style scoped>
.sf-detail-page {
  width: 100%;
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  background: transparent;
  box-sizing: border-box;
}
.sf-detail-page__alert {
  flex-shrink: 0;
}
.sf-detail-page__main {
  flex: 1 1 auto;
  min-height: 0;
  display: flex;
  flex-direction: column;
  gap: 0;
  position: relative;
}
.sf-detail-page__card {
  flex: 1 1 auto;
  min-height: 0;
  display: flex;
  flex-direction: column;
  border-radius: 10px;
}
.sf-detail-page__card :deep(.el-card__body) {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding: 14px 20px 72px;
}
.sf-detail-page__head-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.sf-detail-page__sn {
  font-size: 13px;
  color: #878c93;
}
.sf-detail-page__head-actions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
}
.sf-detail-page__divider {
  margin: 8px 0 12px;
}
.sf-detail-page__title-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  min-height: 40px;
}
.sf-detail-page__title {
  margin: 0;
  font-size: 26px;
  font-weight: 700;
  line-height: 1.3;
  color: var(--el-text-color-primary);
}
.sf-detail-page__meta {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px 16px;
  margin-top: 10px;
  font-size: 13px;
  min-height: 35px;
}
.sf-detail-page__starter {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 4px 10px 4px 4px;
  border-radius: 999px;
  background: var(--el-fill-color-light);
}
.sf-detail-page__avatar {
  background: var(--el-color-primary);
  color: #fff;
  font-size: 13px;
}
.sf-detail-page__meta-muted {
  color: #878c93;
}
.sf-detail-page__tabs {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  margin-top: 8px;
}
.sf-detail-page__tabs :deep(.el-tabs__header) {
  margin-bottom: 8px;
}
.sf-detail-page__tabs :deep(.el-tabs__content) {
  flex: 1;
  min-height: 0;
  overflow: auto;
}
.sf-detail-page__pane {
  padding: 8px 0 16px;
}
.sf-detail-page__refs {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  margin-top: 16px;
  padding-top: 12px;
  border-top: 1px dashed var(--el-border-color-lighter);
}
.sf-detail-page__refs-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.sf-detail-page__ref-tag {
  cursor: pointer;
}
.sf-detail-page__tl-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}
.sf-detail-page__tl-sub {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.sf-detail-page__footer {
  position: absolute;
  left: 8px;
  right: 8px;
  bottom: 12px;
  z-index: 20;
  border-radius: 8px;
  background: var(--el-bg-color);
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.05);
  overflow: visible;
}
.sf-detail-page__next-tree-alert {
  margin-bottom: 10px;
}
.sf-detail-page__next-tree-pre {
  margin: 0;
  max-height: 420px;
  overflow: auto;
  padding: 12px;
  font-size: 12px;
  line-height: 1.45;
  white-space: pre-wrap;
  word-break: break-word;
  background: var(--el-fill-color-light);
  border-radius: 6px;
}

@media print {
  .sf-no-print {
    display: none !important;
  }
  .sf-detail-page {
    padding: 0;
    background: #fff;
  }
  .sf-detail-page__card {
    border: none;
    box-shadow: none;
  }
  .sf-detail-page__card :deep(.el-card__body) {
    padding-bottom: 16px;
  }
}
</style>
