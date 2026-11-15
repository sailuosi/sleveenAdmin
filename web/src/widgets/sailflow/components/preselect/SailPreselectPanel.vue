<template>
  <div v-if="visible" class="sf-preselect-panel">
    <div class="sf-preselect-panel__title">后续节点预选办理人</div>
    <el-alert
      v-if="!targets.length && !loading"
      type="info"
      :closable="false"
      show-icon
      title="本流程无 Preselect 节点"
      class="sf-preselect-panel__alert"
    />
    <el-skeleton v-else-if="loading" :rows="2" animated />
    <div v-for="t in targets" :key="t.ActivityId" class="sf-preselect-panel__row">
      <div class="sf-preselect-panel__label">
        <span>{{ t.ActivityName || t.ActivityId }}</span>
        <el-tag v-if="t.Required" size="small" type="danger" effect="plain">必选</el-tag>
      </div>
      <SailCandidatePicker
        v-model="picks[t.ActivityId]"
        :required="!!t.Required"
        :selector-type="'HostUserSearch'"
        candidate-mode="anyselect"
        :query-context="pickerContext"
        @users-change="list => onUsersChange(t.ActivityId, list)"
      />
    </div>
    <div v-if="err" class="sf-preselect-panel__err">{{ err }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import SailCandidatePicker from '@sail/components/runtime/SailCandidatePicker.vue';
import {
  listPreselectTargets,
  replacePreselectAssignees,
  type PreselectPerformerDto,
  type PreselectTargetDto
} from '@sail/api/preselect';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailPreselectPanel' });

const props = withDefaults(
  defineProps<{
    processId?: string;
    version?: string;
    /** false 时完全不渲染 */
    enabled?: boolean;
  }>(),
  {
    processId: '',
    version: '1',
    enabled: true
  }
);

const loading = ref(false);
const err = ref('');
const targets = ref<PreselectTargetDto[]>([]);
/** activityId → userIds */
const picks = reactive<Record<string, string[]>>({});
/** activityId → 用户行（补 UserName） */
const userRows = reactive<Record<string, Record<string, unknown>[]>>({});

const visible = computed(() => props.enabled && !!props.processId);
const pickerContext = computed(() => ({
  ProcessId: props.processId,
  Version: props.version || '1'
}));

watch(
  () => [props.processId, props.version, props.enabled] as const,
  () => {
    void reload();
  },
  { immediate: true }
);

function userKey(u: Record<string, unknown>) {
  return String(
    pickField(u, 'Account', 'account', 'UserCode', 'userCode', 'UserId', 'userId', 'Id', 'id') || ''
  );
}
function userLabel(u: Record<string, unknown>) {
  return String(
    pickField(u, 'UserName', 'userName', 'RealName', 'realName', 'Name', 'name') || userKey(u)
  );
}

function onUsersChange(activityId: string, list: Record<string, unknown>[]) {
  userRows[activityId] = list || [];
}

async function reload() {
  err.value = '';
  targets.value = [];
  Object.keys(picks).forEach(k => delete picks[k]);
  Object.keys(userRows).forEach(k => delete userRows[k]);
  if (!props.value || !props.processId) return;

  loading.value = true;
  try {
    const data = await listPreselectTargets(props.processId, props.version || '1');
    const r = unpackSailResult<PreselectTargetDto[]>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '加载预选节点失败';
      return;
    }
    const list = Array.isArray(r.entity) ? r.entity : [];
    targets.value = list.map(t => ({
      ActivityId: String(pickField(t as never, 'ActivityId', 'activityId') || ''),
      ActivityName: String(pickField(t as never, 'ActivityName', 'activityName') || ''),
      Required: !!(pickField(t as never, 'Required', 'required') ?? false),
      AllowMulti: pickField(t as never, 'AllowMulti', 'allowMulti') !== false,
      ParamsJson: (pickField(t as never, 'ParamsJson', 'paramsJson') as string) || null
    })).filter(t => !!t.ActivityId);
    for (const t of targets.value) {
      picks[t.ActivityId] = [];
      userRows[t.ActivityId] = [];
    }
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  } finally {
    loading.value = false;
  }
}

/** 发起前校验：required 节点必须已选人 */
function validate(): string | null {
  for (const t of targets.value) {
    if (!t.Required) continue;
    const ids = picks[t.ActivityId] || [];
    if (!ids.length) return `请为「${t.ActivityName || t.ActivityId}」预选办理人`;
  }
  return null;
}

function buildPerformersMap(): Record<string, PreselectPerformerDto[]> {
  const map: Record<string, PreselectPerformerDto[]> = {};
  for (const t of targets.value) {
    const ids = picks[t.ActivityId] || [];
    if (!ids.length) continue;
    const rows = userRows[t.ActivityId] || [];
    map[t.ActivityId] = ids.map(uid => {
      const u = rows.find(x => userKey(x) === uid);
      return {
        UserId: uid,
        UserName: u ? userLabel(u) : uid
      };
    });
  }
  return map;
}

/**
 * Start 成功后写入预选表（独立 api/WfPreselect/Replace）。
 * @returns 写入的节点数；无预选返回 0
 */
async function persist(processInstanceId: string): Promise<number> {
  const map = buildPerformersMap();
  if (!Object.keys(map).length) return 0;
  if (!processInstanceId?.trim()) throw new Error('processInstanceId required');
  const data = await replacePreselectAssignees({
    ProcessInstanceId: processInstanceId.trim(),
    Performers: map
  });
  if ((data?.Status ?? data?.status) === -1) {
    const r = unpackSailResult(data);
    throw new Error(r.message || '写入预选办理人失败');
  }
  return Object.keys(map).length;
}

defineExpose({
  validate,
  persist,
  reload,
  hasTargets: () => targets.value.length > 0,
  buildPerformersMap
});
</script>

<style scoped>
.sf-preselect-panel {
  margin: 12px 0 18px;
  padding: 12px 14px;
  border: 1px dashed var(--el-border-color);
  border-radius: 8px;
  background: var(--el-fill-color-blank);
}
.sf-preselect-panel__title {
  font-weight: 600;
  margin-bottom: 10px;
  color: var(--el-text-color-primary);
}
.sf-preselect-panel__alert {
  margin-bottom: 0;
}
.sf-preselect-panel__row {
  margin-bottom: 12px;
}
.sf-preselect-panel__row:last-child {
  margin-bottom: 0;
}
.sf-preselect-panel__label {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
  font-size: 13px;
  color: var(--el-text-color-regular);
}
.sf-preselect-panel__err {
  margin-top: 8px;
  color: var(--el-color-danger);
  font-size: 12px;
}
</style>
