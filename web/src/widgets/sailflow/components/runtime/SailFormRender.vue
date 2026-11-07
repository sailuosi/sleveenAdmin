<template>
  <div v-if="hasForm" class="sf-form-render">
    <div v-if="title" class="sf-form-render__title">{{ title }}</div>
    <form-create
      v-model="displayValue"
      v-model:api="fApi"
      :option="option"
      :rule="rule"
      :disabled="disabled"
    />
  </div>
  <el-empty v-else-if="showEmpty" description="未绑定动态表单" :image-size="48" />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import {
  applyFieldPermissions,
  decodeConf,
  decodeFields
} from '@sail/utils/formCreate';
import {
  fieldNameOf,
  maskDisplayValue,
  resolvePermissionMode,
  type FieldPermissionLike
} from '@sail/utils/fieldModes';

const props = withDefaults(
  defineProps<{
    conf?: string | null;
    fieldsJson?: string | string[] | null;
    modelValue?: Record<string, unknown>;
    permissions?: FieldPermissionLike[] | null;
    /** §C 可选：field → mode；与 permissions 合并，permissions 优先逐项 Mode */
    fieldModes?: Record<string, string> | null;
    title?: string;
    disabled?: boolean;
    showEmpty?: boolean;
  }>(),
  { showEmpty: false, disabled: false }
);

const emit = defineEmits<{ 'update:modelValue': [Record<string, unknown>] }>();

const fApi = ref<any>(null);
const rawValue = ref<Record<string, unknown>>({});
const displayValue = ref<Record<string, unknown>>({});
const option = ref<Record<string, unknown>>({});
const rule = ref<any[]>([]);
/** 正在把外部 model 同步进 display，忽略 display→emit 回环 */
let syncingFromParent = false;
let lastEmittedJson = '';

const hasForm = computed(() => !!(props.conf || props.fieldsJson));

const maskedFields = computed(() => {
  const set = new Set<string>();
  const perms = props.permissions || [];
  perms.forEach(p => {
    const name = fieldNameOf(p);
    if (name && resolvePermissionMode(p) === 'masked') set.add(name);
  });
  if (props.fieldModes) {
    Object.entries(props.fieldModes).forEach(([k, mode]) => {
      if (resolvePermissionMode({ permission: mode }) === 'masked') set.add(k);
    });
  }
  return set;
});

function mergedPermissions(): FieldPermissionLike[] {
  const list = [...(props.permissions || [])];
  if (props.fieldModes) {
    const have = new Set(list.map(fieldNameOf).filter(Boolean));
    Object.entries(props.fieldModes).forEach(([field, mode]) => {
      if (!field || have.has(field)) return;
      list.push({ FieldName: field, Mode: mode });
    });
  }
  return list;
}

function toDisplay(raw: Record<string, unknown>): Record<string, unknown> {
  const out = { ...raw };
  maskedFields.value.forEach(f => {
    if (f in out) out[f] = maskDisplayValue(out[f]);
  });
  return out;
}

function stableJson(v: Record<string, unknown> | null | undefined) {
  try {
    return JSON.stringify(v || {});
  } catch {
    return '';
  }
}

function applyParentModel(v: Record<string, unknown> | null | undefined) {
  const next = { ...(v || {}) };
  const json = stableJson(next);
  if (json === lastEmittedJson && json === stableJson(rawValue.value)) return;
  syncingFromParent = true;
  rawValue.value = next;
  displayValue.value = toDisplay(next);
  lastEmittedJson = json;
  // form-create 可能异步改 display；微任务后再放行 emit
  queueMicrotask(() => {
    syncingFromParent = false;
  });
}

watch(
  () => [props.conf, props.fieldsJson, props.permissions, props.fieldModes, props.disabled] as const,
  () => {
    const confObj = decodeConf(props.conf || '{}') as Record<string, unknown>;
    option.value = {
      ...confObj,
      submitBtn: false,
      resetBtn: false,
      form: {
        ...((confObj as any)?.form || {}),
        disabled: props.disabled
      }
    };
    let rules = decodeFields(props.fieldsJson || '[]');
    rules = applyFieldPermissions(rules, mergedPermissions());
    rule.value = rules;
    applyParentModel(rawValue.value);
  },
  { immediate: true }
);

watch(
  () => props.modelValue,
  v => {
    if (!v) {
      if (Object.keys(rawValue.value).length) applyParentModel({});
      return;
    }
    applyParentModel(v);
  },
  { immediate: true, deep: true }
);

watch(
  displayValue,
  v => {
    if (syncingFromParent) return;
    const next = { ...rawValue.value };
    Object.entries(v || {}).forEach(([k, val]) => {
      if (maskedFields.value.has(k)) return;
      next[k] = val;
    });
    const json = stableJson(next);
    if (json === lastEmittedJson) return;
    rawValue.value = next;
    lastEmittedJson = json;
    emit('update:modelValue', next);
  },
  { deep: true }
);

defineExpose({
  fApi,
  getValue: () => ({ ...(rawValue.value || {}) }),
  validate: async () => {
    if (!fApi.value?.validate) return true;
    await fApi.value.validate();
    return true;
  }
});
</script>

<style scoped>
.sf-form-render__title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 12px;
}
</style>
