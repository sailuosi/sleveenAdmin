<template>
  <el-dialog v-model="visible" title="添加表单" width="640px" destroy-on-close @closed="onClosed">
    <el-input
      v-model="keyword"
      clearable
      placeholder="筛选名称 / 编码 / Id"
      class="sf-form-pick__filter"
    />
    <el-alert v-if="loadErr" :title="loadErr" type="warning" show-icon :closable="false" class="sf-form-pick__alert" />
    <el-table
      v-loading="loading"
      :data="filtered"
      highlight-current-row
      height="320"
      size="small"
      empty-text="无表单数据（可检查 GetFormAll / 种子）"
      @current-change="onSelect"
      @row-dblclick="row => confirmRow(row)"
    >
      <el-table-column prop="FormId" label="FormId" min-width="120" show-overflow-tooltip />
      <el-table-column prop="FormName" label="名称" min-width="140" />
      <el-table-column prop="FormCode" label="编码" min-width="120" />
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
import { designerEvents, type FormDraft } from '@sail/bpmn/ui/designerEvents';
import { getFormAll, unwrapList, type FormRow } from '@sail/api/resource';

defineOptions({ name: 'FormPickerDialog' });

const visible = ref(false);
const loading = ref(false);
const loadErr = ref('');
const keyword = ref('');
const rows = ref<FormRow[]>([]);
const selected = ref<FormRow | null>(null);
let onConfirm: ((draft: FormDraft) => void) | null = null;

const filtered = computed(() => {
  const kw = keyword.value.trim().toLowerCase();
  if (!kw) return rows.value;
  return rows.value.filter(row => JSON.stringify(row).toLowerCase().includes(kw));
});

function reset() {
  keyword.value = '';
  selected.value = null;
  loadErr.value = '';
  onConfirm = null;
}

function onClosed() {
  reset();
}

function onSelect(row: FormRow | null) {
  selected.value = row;
}

async function loadList() {
  loading.value = true;
  loadErr.value = '';
  try {
    const data = await getFormAll();
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '加载表单失败');
    }
    rows.value = unwrapList<FormRow>(data);
  } catch (e: unknown) {
    loadErr.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function confirmRow(row: FormRow) {
  selected.value = row;
  confirm();
}

function confirm() {
  if (!selected.value?.FormId || !selected.value?.FormName) {
    ElMessage.warning('请选择一条表单');
    return;
  }
  onConfirm?.({
    name: selected.value.FormName,
    outerId: String(selected.value.FormId),
    outerCode: selected.value.FormCode || ''
  });
  visible.value = false;
}

function openHandler(payload: { onConfirm: (draft: FormDraft) => void }) {
  reset();
  onConfirm = payload.onConfirm;
  visible.value = true;
  loadList();
}

onMounted(() => designerEvents.on('form:open', openHandler));
onBeforeUnmount(() => designerEvents.off('form:open', openHandler));
</script>

<style scoped>
.sf-form-pick__filter {
  margin-bottom: 10px;
}
.sf-form-pick__alert {
  margin-bottom: 8px;
}
</style>
