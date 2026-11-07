<template>
  <div class="sf-list-page">
    <header class="sf-list-page__bar">
      <div class="sf-list-page__title">流程表单</div>
      <div class="sf-list-page__actions">
        <el-input
          v-model="keyword"
          clearable
          placeholder="名称 / 编码"
          style="width: 200px"
          @keyup.enter="load"
        />
        <el-button :loading="loading" @click="load">刷新</el-button>
        <el-button type="primary" @click="openEditor()">新建表单</el-button>
      </div>
    </header>
    <main class="sf-list-page__body">
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-list-page__alert" />
      <el-table v-loading="loading" :data="filtered" stripe border empty-text="暂无表单（可先 Probe/init-db）">
        <el-table-column label="Id" width="80">
          <template #default="{ row }">{{ field(row, 'FormId', 'formId') }}</template>
        </el-table-column>
        <el-table-column label="名称" min-width="160">
          <template #default="{ row }">{{ field(row, 'FormName', 'formName') }}</template>
        </el-table-column>
        <el-table-column label="编码" min-width="120">
          <template #default="{ row }">{{ field(row, 'FormCode', 'formCode') }}</template>
        </el-table-column>
        <el-table-column label="版本" width="80">
          <template #default="{ row }">{{ field(row, 'Version', 'version') || '1' }}</template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="openEditor(field(row, 'FormId', 'formId'))">编辑</el-button>
            <el-button link type="danger" @click="onDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import { deleteForm, getFormList } from '@sail/api/form';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailFormListPage' });

const router = useRouter();
const loading = ref(false);
const err = ref('');
const rows = ref<Record<string, unknown>[]>([]);
const keyword = ref('');

const filtered = computed(() => {
  const kw = keyword.value.trim().toLowerCase();
  if (!kw) return rows.value;
  return rows.value.filter(r => JSON.stringify(r).toLowerCase().includes(kw));
});

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const data = await getFormList();
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '加载失败';
      rows.value = [];
      return;
    }
    rows.value = Array.isArray(r.entity) ? r.entity : [];
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function openEditor(id?: unknown) {
  router.push({
    path: '/sail/form-editor',
    query: id ? { formId: String(id) } : undefined
  });
}

async function onDelete(row: Record<string, unknown>) {
  const id = field(row, 'FormId', 'formId');
  try {
    await ElMessageBox.confirm(`确认删除表单 ${field(row, 'FormName', 'formName')}？`, '删除', {
      type: 'warning'
    });
  } catch {
    return;
  }
  try {
    const data = await deleteForm(String(id));
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '删除失败');
      return;
    }
    ElMessage.success('已删除');
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

onMounted(load);
</script>

<style scoped>
.sf-list-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page, #f5f7fa);
}
.sf-list-page__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 16px;
  background: var(--el-bg-color);
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.sf-list-page__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-list-page__actions {
  display: flex;
  gap: 8px;
  align-items: center;
}
.sf-list-page__body {
  padding: 16px;
  flex: 1;
}
.sf-list-page__alert {
  margin-bottom: 12px;
}
</style>
