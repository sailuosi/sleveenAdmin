<template>
  <el-dialog
    v-model="visible"
    title="条件表达式编辑器"
    width="780px"
    destroy-on-close
    @closed="onClosed"
  >
    <div class="sf-expr__toolbar">
      <el-button type="primary" @click="addGroup">添加条件组</el-button>
      <el-button @click="clearAll">清空</el-button>
    </div>

    <div v-for="(group, gi) in groups" :key="group.id" class="sf-expr__group">
      <div class="sf-expr__group-bar">
        <span>组 {{ gi + 1 }}</span>
        <el-select v-if="gi > 0" v-model="group.operator" size="small" style="width: 100px">
          <el-option label="AND" value="AND" />
          <el-option label="OR" value="OR" />
        </el-select>
        <el-button link type="danger" @click="removeGroup(gi)">删除组</el-button>
      </div>
      <div v-for="(item, ii) in group.items" :key="ii" class="sf-expr__item">
        <el-select v-if="ii > 0" v-model="item.logic" size="small" style="width: 88px">
          <el-option label="AND" value="AND" />
          <el-option label="OR" value="OR" />
        </el-select>
        <el-input v-model="item.field" size="small" placeholder="字段" style="width: 140px" />
        <el-select v-model="item.operator" size="small" style="width: 120px">
          <el-option v-for="op in OPS" :key="op" :label="op" :value="op" />
        </el-select>
        <el-input v-model="item.value" size="small" placeholder="值" style="flex: 1" />
        <el-button link type="danger" @click="group.items.splice(ii, 1)">删</el-button>
      </div>
      <el-button size="small" @click="addItem(group)">添加条件</el-button>
    </div>

    <el-divider content-position="left">预览 / 原文</el-divider>
    <el-input v-model="raw" type="textarea" :rows="3" class="sf-expr__raw" @change="onRawChange" />
    <div class="sf-expr__preview">{{ preview || '（空）' }}</div>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" @click="confirm">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import {
  generateExpression,
  newGroupId,
  parseExpression,
  type ExprGroup,
  type ExprOperator
} from '@sail/bpmn/utils/expressionBuilder';

defineOptions({ name: 'ExpressionEditorDialog' });

const OPS: ExprOperator[] = [
  '==',
  '!=',
  '>',
  '>=',
  '<',
  '<=',
  'contains',
  'startsWith',
  'endsWith'
];

const visible = ref(false);
const groups = ref<ExprGroup[]>([]);
const raw = ref('');
let onConfirm: ((expr: string) => void) | null = null;
let syncingFromRaw = false;

const preview = computed(() => generateExpression(groups.value));

watch(
  groups,
  () => {
    if (syncingFromRaw) return;
    raw.value = preview.value;
  },
  { deep: true }
);

function emptyItem(first: boolean) {
  return {
    field: '',
    operator: '==' as ExprOperator,
    value: '',
    logic: (first ? '' : 'AND') as '' | 'AND' | 'OR'
  };
}

function addGroup() {
  groups.value.push({
    id: newGroupId(),
    operator: 'AND',
    items: [emptyItem(true)]
  });
}

function removeGroup(index: number) {
  groups.value.splice(index, 1);
}

function addItem(group: ExprGroup) {
  group.items.push(emptyItem(group.items.length === 0));
}

function clearAll() {
  groups.value = [];
  raw.value = '';
}

function onRawChange() {
  syncingFromRaw = true;
  const parsed = parseExpression(raw.value);
  groups.value = parsed.length
    ? parsed
    : raw.value.trim()
      ? []
      : [];
  syncingFromRaw = false;
}

function onClosed() {
  groups.value = [];
  raw.value = '';
  onConfirm = null;
}

function confirm() {
  const expr = preview.value || raw.value.trim();
  onConfirm?.(expr);
  visible.value = false;
}

function openHandler(payload: { value: string; onConfirm: (expr: string) => void }) {
  onConfirm = payload.onConfirm;
  raw.value = payload.value || '';
  const parsed = parseExpression(raw.value);
  groups.value = parsed.length ? parsed : raw.value.trim() ? [] : [];
  if (!groups.value.length && !raw.value.trim()) addGroup();
  visible.value = true;
}

onMounted(() => designerEvents.on('expression:open', openHandler));
onBeforeUnmount(() => designerEvents.off('expression:open', openHandler));
</script>

<style scoped>
.sf-expr__toolbar {
  display: flex;
  gap: 8px;
  margin-bottom: 12px;
}
.sf-expr__group {
  border: 1px solid var(--el-border-color-lighter, #ebeef5);
  border-radius: 6px;
  padding: 10px;
  margin-bottom: 10px;
  background: var(--el-fill-color-blank, #fff);
}
.sf-expr__group-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
  font-weight: 600;
}
.sf-expr__item {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.sf-expr__raw :deep(textarea) {
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
}
.sf-expr__preview {
  margin-top: 8px;
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  word-break: break-all;
}
</style>
