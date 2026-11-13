<template>
  <div class="sf-candidate-picker">
    <el-select
      v-model="innerIds"
      multiple
      filterable
      clearable
      collapse-tags
      collapse-tags-tooltip
      remote
      :remote-method="onRemote"
      :loading="loading"
      :disabled="disabled"
      :placeholder="placeholderText"
      style="width: 100%"
      @visible-change="onVisible"
    >
      <el-option
        v-for="u in options"
        :key="userKey(u)"
        :label="userLabel(u)"
        :value="userKey(u)"
      />
    </el-select>
    <div v-if="hint" class="sf-candidate-picker__hint">{{ hint }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';
import { queryNextActivityCandidates } from '@sail/api/runtime';
import { loadHostCandidateUsers } from '@sail/adapters/hostUsers';

defineOptions({ name: 'SailCandidatePicker' });

export type SailCandidateQueryContext = {
  ProcessId?: string;
  Version?: string;
  ActivityId?: string;
  TaskId?: string;
};

const props = withDefaults(
  defineProps<{
    modelValue?: string[];
    required?: boolean;
    disabled?: boolean;
    placeholder?: string;
    /** HostUserSearch | InlineList | RoleUserSearch */
    selectorType?: string | null;
    selectorParams?: string | null;
    /** bound | anyselect | search */
    candidateMode?: string | null;
    inlineUsers?: Record<string, unknown>[] | null;
    candidateFilter?: { RoleIds?: string[]; roleIds?: string[]; ReceiverType?: number } | null;
    queryContext?: SailCandidateQueryContext | null;
  }>(),
  {
    modelValue: () => [],
    required: false,
    disabled: false,
    selectorType: '',
    selectorParams: '',
    candidateMode: '',
    inlineUsers: () => [],
    candidateFilter: null,
    queryContext: null
  }
);

const emit = defineEmits<{
  'update:modelValue': [string[]];
  'users-change': [Record<string, unknown>[]];
}>();

const loading = ref(false);
const remoteUsers = ref<Record<string, unknown>[]>([]);
const knownUsers = ref<Record<string, unknown>[]>([]);

const innerIds = computed({
  get: () => props.modelValue || [],
  set: (v: string[]) => emit('update:modelValue', v || [])
});

const mode = computed(() => String(props.candidateMode || '').toLowerCase());
const selector = computed(() => String(props.selectorType || '').trim());

const useInline = computed(() => {
  if (selector.value === 'InlineList') return true;
  if (mode.value === 'bound' && (props.inlineUsers?.length || 0) > 0 && selector.value !== 'HostUserSearch')
    return true;
  return false;
});

const useRemote = computed(() => !useInline.value);

const placeholderText = computed(() => {
  if (props.placeholder) return props.placeholder;
  if (props.required) return useRemote.value ? '必选：搜索并指定办理人' : '必选：请指定下一办理人';
  return useRemote.value ? '可选：搜索办理人' : '可选：覆盖默认候选人';
});

const hint = computed(() => {
  if (!props.required) return '';
  if (useInline.value && !(props.inlineUsers?.length || 0)) return '暂无候选人，请检查角色配置';
  if (useRemote.value && !innerIds.value.length) return '请输入关键字搜索人员';
  return '';
});

const options = computed(() => {
  const map = new Map<string, Record<string, unknown>>();
  const push = (list: Record<string, unknown>[] | null | undefined) => {
    (list || []).forEach(u => {
      const k = userKey(u);
      if (k) map.set(k, u);
    });
  };
  if (useInline.value) push(props.inlineUsers || []);
  else push(remoteUsers.value);
  push(knownUsers.value);
  // 保证已选值仍有 label
  innerIds.value.forEach(id => {
    if (id && !map.has(id)) map.set(id, { Account: id, UserName: id });
  });
  return Array.from(map.values());
});

watch(
  options,
  list => emit('users-change', list),
  { immediate: true, deep: true }
);

function userKey(u: Record<string, unknown>) {
  return String(
    pickField(u, 'Account', 'account', 'UserCode', 'userCode', 'UserId', 'userId', 'Id', 'id') || ''
  );
}
function userLabel(u: Record<string, unknown>) {
  return String(pickField(u, 'UserName', 'userName', 'RealName', 'realName', 'Name', 'name') || userKey(u));
}

function roleIdsFromProps(): string[] {
  const f = props.candidateFilter;
  const fromFilter = (f?.RoleIds || f?.roleIds || []) as string[];
  if (fromFilter.length) return fromFilter.map(String);
  try {
    const p = props.selectorParams ? JSON.parse(props.selectorParams) : {};
    const ids = p?.roleIds || p?.RoleIds;
    return Array.isArray(ids) ? ids.map(String) : [];
  } catch {
    return [];
  }
}

function pageSizeFromParams(): number {
  try {
    const p = props.selectorParams ? JSON.parse(props.selectorParams) : {};
    const n = Number(p?.pageSize ?? p?.PageSize ?? 100);
    return Number.isFinite(n) && n > 0 ? Math.min(n, 100) : 100;
  } catch {
    return 100;
  }
}

async function fetchRemote(keyword?: string) {
  if (!useRemote.value) return;
  loading.value = true;
  try {
    const ctx = props.queryContext || {};
    const data = await queryNextActivityCandidates({
      ProcessId: ctx.ProcessId,
      Version: ctx.Version,
      ActivityId: ctx.ActivityId,
      TaskId: ctx.TaskId,
      Keyword: keyword || undefined,
      Page: 1,
      PageSize: pageSizeFromParams(),
      RoleIds: roleIdsFromProps(),
      CandidateMode: props.candidateMode || undefined,
      CandidateSelectorType: props.selectorType || undefined
    });
    const r = unpackSailResult<{ Users?: unknown[]; users?: unknown[] }>(data);
    const entity = r.entity || {};
    const list = (pickField<unknown[]>(entity as any, 'Users', 'users') || []) as Record<
      string,
      unknown
    >[];
    if (Array.isArray(list) && list.length) {
      remoteUsers.value = list.filter(u => !!userKey(u));
      knownUsers.value = mergeUsers(knownUsers.value, remoteUsers.value);
      return;
    }
    // 接口空/失败：回落宿主分页
    const host = await loadHostCandidateUsers(keyword, pageSizeFromParams());
    remoteUsers.value = host as unknown as Record<string, unknown>[];
    knownUsers.value = mergeUsers(knownUsers.value, remoteUsers.value);
  } catch {
    try {
      const host = await loadHostCandidateUsers(keyword, pageSizeFromParams());
      remoteUsers.value = host as unknown as Record<string, unknown>[];
      knownUsers.value = mergeUsers(knownUsers.value, remoteUsers.value);
    } catch {
      remoteUsers.value = [];
    }
  } finally {
    loading.value = false;
  }
}

function mergeUsers(a: Record<string, unknown>[], b: Record<string, unknown>[]) {
  const map = new Map<string, Record<string, unknown>>();
  [...a, ...b].forEach(u => {
    const k = userKey(u);
    if (k) map.set(k, u);
  });
  return Array.from(map.values());
}

let remoteTimer: ReturnType<typeof setTimeout> | null = null;
function onRemote(q: string) {
  if (!useRemote.value) return;
  if (remoteTimer) clearTimeout(remoteTimer);
  remoteTimer = setTimeout(() => fetchRemote(q), 250);
}

function onVisible(open: boolean) {
  if (open && useRemote.value && !remoteUsers.value.length) fetchRemote('');
}

watch(
  () => [props.inlineUsers, props.selectorType, props.candidateMode, props.queryContext?.ActivityId],
  () => {
    if (useInline.value) {
      remoteUsers.value = [];
      knownUsers.value = mergeUsers(knownUsers.value, props.inlineUsers || []);
    } else {
      remoteUsers.value = [];
    }
  },
  { deep: true }
);

defineExpose({ userKey, userLabel, options });
</script>

<style scoped>
.sf-candidate-picker__hint {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-color-warning);
}
</style>
