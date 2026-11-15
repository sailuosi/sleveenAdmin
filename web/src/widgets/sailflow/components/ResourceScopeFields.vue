<template>
  <template v-if="canRender">
    <el-form-item :label="`${resourceLabel}归属`">
      <el-select
        v-if="!existing && !fixedScopeType && context?.isPlatformAdmin"
        :model-value="authType || 'PUBLIC'"
        style="width: 100%"
        @change="onAuthTypeChange"
      >
        <el-option :label="`公共${resourceLabel}`" value="PUBLIC" />
        <el-option :label="`单位${resourceLabel}`" value="UNIT" />
        <el-option v-if="allowTemplate" :label="`模板${resourceLabel}`" value="TEMPLATE" />
      </el-select>
      <el-text v-else>{{ authTypeLabel }}</el-text>
    </el-form-item>
    <el-form-item v-if="authType === 'UNIT'" label="所属单位" required>
      <el-select
        :model-value="authId || ''"
        filterable
        clearable
        placeholder="选择所属单位"
        style="width: 100%"
        :disabled="lockOrg"
        @change="onAuthIdChange"
      >
        <el-option v-for="u in unitOptions" :key="u.id" :label="u.label" :value="u.id" />
      </el-select>
    </el-form-item>
    <el-form-item v-if="authType === 'PUBLIC' || authType === 'TEMPLATE'" label="共享单位">
      <el-select
        :model-value="allowedUnitIds"
        multiple
        filterable
        clearable
        collapse-tags
        collapse-tags-tooltip
        placeholder="空=全部组织可用"
        style="width: 100%"
        @change="onAllowedChange"
      >
        <el-option v-for="u in unitOptions" :key="u.id" :label="u.label" :value="u.id" />
      </el-select>
    </el-form-item>
  </template>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { getUnitScopeContext } from '@sail/api/unitScope';
import { unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailResourceScopeFields' });

export type UnitContextLite = {
  isPlatformAdmin?: boolean;
  managedUnitIds?: string[];
  orgs?: Array<{ id?: string; Id?: string; name?: string; Name?: string; parentId?: string }>;
  units?: Array<{ id?: string; name?: string; label?: string }>;
};

const props = withDefaults(
  defineProps<{
    existing?: boolean;
    fixedScopeType?: 'PUBLIC' | 'UNIT' | 'TEMPLATE';
    lockOrg?: boolean;
    resourceLabel?: string;
    allowTemplate?: boolean;
    unitContext?: UnitContextLite | null;
  }>(),
  {
    resourceLabel: '资源',
    allowTemplate: false
  }
);

const authType = defineModel<string>('authType', { default: '' });
const authId = defineModel<string>('authId', { default: '' });
const authCode = defineModel<string>('authCode', { default: '' });

const context = ref<UnitContextLite | null>(props.unitContext || null);

const canRender = computed(() => !!context.value || props.existing || !!authType.value);

const authTypeLabel = computed(() => {
  if (authType.value === 'UNIT') return `单位${props.resourceLabel}`;
  if (authType.value === 'TEMPLATE') return `模板${props.resourceLabel}`;
  if (authType.value === 'PUBLIC') return `公共${props.resourceLabel}`;
  return '待分配';
});

const unitOptions = computed(() => {
  const src = context.value?.orgs?.length
    ? context.value.orgs
    : context.value?.units || [];
  return (src || []).map((u: any) => {
    const id = String(u.id ?? u.Id ?? '');
    const name = String(u.label ?? u.name ?? u.Name ?? id);
    return { id, label: name };
  }).filter((u) => u.id);
});

const allowedUnitIds = computed(() => {
  const raw = String(authCode.value || '').trim();
  if (!raw) return [] as string[];
  if (raw.startsWith('[')) {
    try {
      const arr = JSON.parse(raw);
      return Array.isArray(arr) ? arr.map(String) : [];
    } catch {
      /* fallthrough */
    }
  }
  return raw.split(/[,;|]/).map((x) => x.trim()).filter(Boolean);
});

function onAuthTypeChange(v: string) {
  authType.value = v;
  if (v !== 'UNIT') authId.value = '';
  if (v === 'UNIT') authCode.value = '';
}

function onAuthIdChange(v: string) {
  authId.value = v || '';
}

function onAllowedChange(ids: string[]) {
  authCode.value = (ids || []).join(',');
}

async function ensureContext() {
  if (context.value) return;
  try {
    const data = await getUnitScopeContext();
    const r = unpackSailResult<UnitContextLite>(data);
    context.value = r.entity || (data?.Entity ?? data?.entity) || null;
  } catch {
    context.value = { isPlatformAdmin: false, orgs: [], units: [] };
  }
}

watch(
  () => props.unitContext,
  (v) => {
    if (v) context.value = v;
  }
);

watch(
  () => props.fixedScopeType,
  (v) => {
    if (v) authType.value = v;
  },
  { immediate: true }
);

onMounted(() => {
  void ensureContext();
});
</script>
