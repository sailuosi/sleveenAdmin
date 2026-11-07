<template>
  <div class="sf-list-page">
    <header class="sf-list-page__bar">
      <div class="sf-list-page__title">流程实例管理</div>
      <div class="sf-list-page__actions">
        <el-select v-model="processState" clearable placeholder="状态" style="width: 120px" @change="reload">
          <el-option label="运行中" :value="2" />
          <el-option label="完成" :value="4" />
          <el-option label="取消/关闭" :value="5" />
        </el-select>
        <el-input
          v-model="keyword"
          clearable
          placeholder="应用名称"
          style="width: 180px"
          @keyup.enter="reload"
        />
        <el-button :loading="loading" @click="reload">刷新</el-button>
      </div>
    </header>
    <main class="sf-list-page__body">
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-list-page__alert" />
      <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无实例">
        <el-table-column label="流程" min-width="140">
          <template #default="{ row }">{{ field(row, 'ProcessName', 'processName') }}</template>
        </el-table-column>
        <el-table-column label="应用" min-width="120">
          <template #default="{ row }">{{ field(row, 'AppName', 'appName') }}</template>
        </el-table-column>
        <el-table-column label="业务单号" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'AppInstanceId', 'appInstanceId') }}</template>
        </el-table-column>
        <el-table-column label="发起人" width="110">
          <template #default="{ row }">{{ field(row, 'CreatedUserName', 'createdUserName') }}</template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default="{ row }">{{ field(row, 'ProcessState', 'processState') }}</template>
        </el-table-column>
        <el-table-column label="发起时间" width="170">
          <template #default="{ row }">{{ field(row, 'CreatedDateTime', 'createdDateTime') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="goDetail(row)">详情</el-button>
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
import { queryProcessInstances } from '@sail/api/runtime';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailProcessManagerPage' });

const router = useRouter();
const loading = ref(false);
const err = ref('');
const rows = ref<Record<string, unknown>[]>([]);
const keyword = ref('');
const processState = ref<number | undefined>(undefined);
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
    const data = await queryProcessInstances({
      AppName: keyword.value || undefined,
      ProcessState: processState.value ?? null,
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
  router.push({
    path: '/sail/process-detail',
    query: { processInstanceId: String(field(row, 'Id', 'id')) }
  });
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
  align-items: center;
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
