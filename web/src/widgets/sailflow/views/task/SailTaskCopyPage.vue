<template>
  <div class="sf-list-page">
    <header class="sf-list-page__bar">
      <div class="sf-list-page__title">抄送我的</div>
      <div class="sf-list-page__actions">
        <el-button :loading="loading" @click="reload">刷新</el-button>
      </div>
    </header>
    <main class="sf-list-page__body">
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-list-page__alert" />
      <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无抄送">
        <el-table-column label="流程" min-width="140">
          <template #default="{ row }">{{ field(row, 'ProcessName', 'processName') }}</template>
        </el-table-column>
        <el-table-column label="任务" min-width="120">
          <template #default="{ row }">{{ field(row, 'TaskName', 'taskName') }}</template>
        </el-table-column>
        <el-table-column label="业务单号" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'AppInstanceId', 'appInstanceId') }}</template>
        </el-table-column>
        <el-table-column label="抄送人" width="110">
          <template #default="{ row }">{{ field(row, 'CreatorName', 'creatorName') }}</template>
        </el-table-column>
        <el-table-column label="原因" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'Reason', 'reason') }}</template>
        </el-table-column>
        <el-table-column label="时间" width="170">
          <template #default="{ row }">{{ field(row, 'CreateTime', 'createTime') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="goDetail(row)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div class="sf-list-page__pager">
        <el-pagination
          v-model:current-page="pageIndex"
          v-model:page-size="pageSize"
          layout="total, prev, pager, next"
          :total="total"
          @current-change="load"
          @size-change="reload"
        />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { queryMyCopyPage } from '@sail/api/runtime';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailTaskCopyPage' });

const router = useRouter();
const { userId } = useSailCurrentUser();
const loading = ref(false);
const err = ref('');
const rows = ref<Record<string, unknown>[]>([]);
const pageIndex = ref(1);
const pageSize = ref(20);
const total = ref(0);

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const data = await queryMyCopyPage({
      UserId: userId.value,
      PageIndex: pageIndex.value,
      PageSize: pageSize.value
    });
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '查询失败';
      rows.value = [];
      return;
    }
    rows.value = Array.isArray(r.entity) ? r.entity : [];
    total.value = r.total || rows.value.length;
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function reload() {
  pageIndex.value = 1;
  load();
}

function goDetail(row: Record<string, unknown>) {
  const q: Record<string, string> = {};
  const taskId = field(row, 'TaskId', 'taskId');
  const pi = field(row, 'ProcessInstanceId', 'processInstanceId');
  if (taskId) q.taskId = String(taskId);
  if (pi) q.processInstanceId = String(pi);
  router.push({ path: '/sail/process-detail', query: q });
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
}
.sf-list-page__body {
  padding: 16px;
  flex: 1;
}
.sf-list-page__alert {
  margin-bottom: 12px;
}
.sf-list-page__pager {
  margin-top: 12px;
  display: flex;
  justify-content: flex-end;
}
</style>
