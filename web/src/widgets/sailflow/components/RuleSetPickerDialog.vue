<template>
  <el-dialog v-model="visible" title="选择规则集" width="560px" destroy-on-close @closed="onClosed">
    <el-input v-model="keyword" clearable placeholder="筛选编码 / 名称" class="sf-rs-pick__filter" />
    <el-alert v-if="loadErr" :title="loadErr" type="warning" show-icon :closable="false" />
    <el-table
      v-loading="loading"
      :data="filtered"
      highlight-current-row
      height="320"
      size="small"
      empty-text="暂无规则集"
      @current-change="onSelect"
      @row-dblclick="row => confirmRow(row)"
    >
      <el-table-column prop="RuleSetCode" label="编码" min-width="140" />
      <el-table-column prop="RuleSetName" label="名称" min-width="140" />
      <el-table-column prop="Mode" label="模式" width="110" />
    </el-table>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :disabled="!selected?.RuleSetCode" @click="confirm">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { getRuleSetList, type RuleSetRow } from '@sail/api/ruleset';

defineOptions({ name: 'RuleSetPickerDialog' });

const visible = ref(false);
const loading = ref(false);
const loadErr = ref('');
const keyword = ref('');
const rows = ref<RuleSetRow[]>([]);
const selected = ref<RuleSetRow | null>(null);
let onConfirm: ((code: string) => void) | null = null;
let currentCode = '';

const filtered = computed(() => {
  const kw = keyword.value.trim().toLowerCase();
  if (!kw) return rows.value;
  return rows.value.filter(r => JSON.stringify(r).toLowerCase().includes(kw));
});

function onClosed() {
  keyword.value = '';
  selected.value = null;
  onConfirm = null;
  currentCode = '';
}

function onSelect(row: RuleSetRow | null) {
  selected.value = row;
}

async function load() {
  loading.value = true;
  loadErr.value = '';
  try {
    const data = await getRuleSetList();
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '加载失败');
    }
    const list = data?.Entity ?? data?.entity ?? data;
    rows.value = Array.isArray(list) ? list : [];
    if (currentCode) {
      selected.value = rows.value.find(r => r.RuleSetCode === currentCode) || null;
    }
  } catch (e: unknown) {
    loadErr.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function confirmRow(row: RuleSetRow) {
  selected.value = row;
  confirm();
}

function confirm() {
  const code = selected.value?.RuleSetCode;
  if (!code) {
    ElMessage.warning('请选择规则集');
    return;
  }
  onConfirm?.(code);
  visible.value = false;
}

function openHandler(payload: { currentCode?: string; onConfirm: (code: string) => void }) {
  currentCode = payload.currentCode || '';
  onConfirm = payload.onConfirm;
  visible.value = true;
  load();
}

onMounted(() => designerEvents.on('ruleset:pick', openHandler));
onBeforeUnmount(() => designerEvents.off('ruleset:pick', openHandler));
</script>

<style scoped>
.sf-rs-pick__filter {
  margin-bottom: 10px;
}
</style>
