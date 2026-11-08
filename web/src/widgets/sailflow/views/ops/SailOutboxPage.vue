<template>
  <div class="sf-list-page">
    <header class="sf-list-page__bar">
      <div class="sf-list-page__title">回调死信 Outbox</div>
      <div class="sf-list-page__actions">
        <el-select v-model="status" style="width: 130px" @change="load">
          <el-option label="Dead" value="Dead" />
          <el-option label="Pending" value="Pending" />
          <el-option label="Failed" value="Failed" />
          <el-option label="Succeeded" value="Succeeded" />
        </el-select>
        <el-input v-model="processCode" clearable placeholder="ProcessCode" style="width: 160px" />
        <el-button :loading="loading" @click="load">刷新</el-button>
        <el-button
          type="warning"
          :disabled="!selectedIds.length"
          :loading="batching"
          @click="onResetBatch"
        >
          重试选中
        </el-button>
        <el-button type="danger" plain :loading="batching" @click="onResetAll">全部重试死信</el-button>
        <el-button @click="router.push('/sail/processes')">流程定义</el-button>
      </div>
    </header>
    <main class="sf-list-page__body">
      <el-alert
        :title="`当前状态「${status}」共 ${rows.length} 条（上限 Limit）`"
        type="info"
        show-icon
        :closable="false"
        class="sf-list-page__alert"
      />
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-list-page__alert" />
      <el-table
        v-loading="loading"
        :data="rows"
        stripe
        border
        empty-text="暂无记录"
        @selection-change="onSel"
      >
        <el-table-column type="selection" width="48" />
        <el-table-column label="Id" width="70">
          <template #default="{ row }">{{ field(row, 'Id', 'id') }}</template>
        </el-table-column>
        <el-table-column label="Kind" width="90">
          <template #default="{ row }">{{ field(row, 'Kind', 'kind') }}</template>
        </el-table-column>
        <el-table-column label="Handler" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'HandlerName', 'handlerName') }}</template>
        </el-table-column>
        <el-table-column label="Event" min-width="120" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'EventType', 'eventType') }}</template>
        </el-table-column>
        <el-table-column label="ProcessCode" width="120" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'ProcessCode', 'processCode') }}</template>
        </el-table-column>
        <el-table-column label="Attempt" width="80">
          <template #default="{ row }">{{ field(row, 'Attempt', 'attempt') }}</template>
        </el-table-column>
        <el-table-column label="LastError" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'LastError', 'lastError') }}</template>
        </el-table-column>
        <el-table-column label="Updated" width="170">
          <template #default="{ row }">{{ field(row, 'UpdatedAt', 'updatedAt') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="String(field(row, 'Status', 'status')) === 'Dead'"
              link
              type="primary"
              @click="onResetOne(row)"
            >
              重试
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import {
  queryOutbox,
  resetAllOutboxDead,
  resetOutboxDead,
  resetOutboxDeadBatch
} from '@sail/api/outbox';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailOutboxPage' });

const router = useRouter();
const loading = ref(false);
const batching = ref(false);
const err = ref('');
const rows = ref<Record<string, unknown>[]>([]);
const status = ref('Dead');
const processCode = ref('');
const selectedIds = ref<string[]>([]);

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}

function onSel(list: Record<string, unknown>[]) {
  selectedIds.value = list
    .map(r => String(field(r, 'Id', 'id') || '').trim())
    .filter(id => id !== '');
}

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const data = await queryOutbox({
      Status: status.value,
      ProcessCode: processCode.value || undefined,
      Limit: 200
    });
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '查询失败';
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

async function onResetOne(row: Record<string, unknown>) {
  const id = String(field(row, 'Id', 'id') || '').trim();
  if (!id) return;
  try {
    const data = await resetOutboxDead(id);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '重试失败');
      return;
    }
    ElMessage.success('已重回 Pending');
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onResetBatch() {
  if (!selectedIds.value.length) return;
  batching.value = true;
  try {
    const data = await resetOutboxDeadBatch(selectedIds.value);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '批量失败');
      return;
    }
    ElMessage.success(data?.Message || data?.message || '已重试');
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    batching.value = false;
  }
}

async function onResetAll() {
  try {
    await ElMessageBox.confirm('将当前死信全部重置为 Pending？', '全部重试', { type: 'warning' });
  } catch {
    return;
  }
  batching.value = true;
  try {
    const data = await resetAllOutboxDead(500);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '失败');
      return;
    }
    ElMessage.success(data?.Message || data?.message || '完成');
    status.value = 'Dead';
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    batching.value = false;
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
  flex-wrap: wrap;
}
.sf-list-page__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-list-page__actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
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
