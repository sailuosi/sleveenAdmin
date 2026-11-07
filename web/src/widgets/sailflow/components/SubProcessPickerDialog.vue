<template>
  <el-dialog v-model="visible" title="选择子流程" width="640px" destroy-on-close @closed="onClosed">
    <el-input v-model="keyword" clearable placeholder="筛选名称 / 编码 / Id" class="sf-sub-pick__filter" />
    <el-alert v-if="loadErr" :title="loadErr" type="warning" show-icon :closable="false" />
    <el-table
      v-loading="loading"
      :data="filtered"
      highlight-current-row
      height="320"
      size="small"
      empty-text="暂无流程"
      @current-change="onSelect"
      @row-dblclick="row => confirmRow(row)"
    >
      <el-table-column prop="ProcessName" label="名称" min-width="140" />
      <el-table-column prop="ProcessCode" label="编码" width="120" />
      <el-table-column prop="Version" label="版本" width="70" />
      <el-table-column prop="ProcessId" label="ProcessId" min-width="160" show-overflow-tooltip />
    </el-table>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :disabled="!selected" @click="confirm">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents, type SubProcessDraft } from '@sail/bpmn/ui/designerEvents';
import { getProcessListSimple } from '@sail/api/process';

defineOptions({ name: 'SubProcessPickerDialog' });

const visible = ref(false);
const loading = ref(false);
const loadErr = ref('');
const keyword = ref('');
const rows = ref<Record<string, unknown>[]>([]);
const selected = ref<Record<string, unknown> | null>(null);
let onConfirm: ((draft: SubProcessDraft) => void) | null = null;

const filtered = computed(() => {
  const kw = keyword.value.trim().toLowerCase();
  if (!kw) return rows.value;
  return rows.value.filter(r => JSON.stringify(r).toLowerCase().includes(kw));
});

function onClosed() {
  keyword.value = '';
  selected.value = null;
  onConfirm = null;
}

function onSelect(row: Record<string, unknown> | null) {
  selected.value = row;
}

async function load() {
  loading.value = true;
  loadErr.value = '';
  try {
    const data = await getProcessListSimple();
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '加载失败');
    }
    const list = data?.Entity ?? data?.EntityList ?? data;
    rows.value = Array.isArray(list) ? list : [];
  } catch (e: unknown) {
    loadErr.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function confirmRow(row: Record<string, unknown>) {
  selected.value = row;
  confirm();
}

function confirm() {
  if (!selected.value?.ProcessId) {
    ElMessage.warning('请选择流程');
    return;
  }
  onConfirm?.({
    subId: String(selected.value.Id ?? selected.value.ProcessId),
    subProcessId: String(selected.value.ProcessId),
    subProcessName: String(selected.value.ProcessName || selected.value.ProcessCode || '')
  });
  visible.value = false;
}

function openHandler(payload: { onConfirm: (draft: SubProcessDraft) => void }) {
  onConfirm = payload.onConfirm;
  visible.value = true;
  load();
}

onMounted(() => designerEvents.on('subprocess:pick', openHandler));
onBeforeUnmount(() => designerEvents.off('subprocess:pick', openHandler));
</script>

<style scoped>
.sf-sub-pick__filter {
  margin-bottom: 10px;
}
</style>
