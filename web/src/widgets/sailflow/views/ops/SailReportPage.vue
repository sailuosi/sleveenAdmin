<template>
  <div class="sf-report-page">
    <header class="sf-report-page__bar">
      <div class="sf-report-page__title">流程报表</div>
      <div class="sf-report-page__actions">
        <el-button :loading="loading" @click="load">刷新</el-button>
        <el-button @click="router.push('/sail/processes')">流程定义</el-button>
        <el-button @click="router.push('/sail/outbox')">死信</el-button>
        <el-button @click="router.push('/sail/categories')">分类</el-button>
      </div>
    </header>
    <main v-loading="loading" class="sf-report-page__main">
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-report-page__alert" />
      <el-row :gutter="12">
        <el-col v-for="card in cards" :key="card.label" :xs="12" :sm="8" :md="6">
          <div class="sf-report-page__card">
            <div class="sf-report-page__card-val">{{ card.value }}</div>
            <div class="sf-report-page__card-label">{{ card.label }}</div>
          </div>
        </el-col>
      </el-row>
      <h3 class="sf-report-page__h">实例量 Top</h3>
      <el-table :data="top" stripe border empty-text="暂无实例">
        <el-table-column prop="ProcessName" label="流程" min-width="160" />
        <el-table-column prop="ProcessId" label="ProcessId" min-width="180" show-overflow-tooltip />
        <el-table-column prop="Version" label="版本" width="80" />
        <el-table-column prop="InstanceCount" label="实例数" width="100" />
      </el-table>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { getReportOverview, type ProcessInstanceRankItem, type WorkflowReportOverview } from '@sail/api/report';
import { unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailReportPage' });

const router = useRouter();
const loading = ref(false);
const err = ref('');
const overview = ref<WorkflowReportOverview>({});
const top = ref<ProcessInstanceRankItem[]>([]);

const cards = computed(() => {
  const o = overview.value;
  return [
    { label: '流程定义', value: o.ProcessDefinitionTotal ?? 0 },
    { label: '已启用', value: o.ProcessDefinitionEnabled ?? 0 },
    { label: '已停用', value: o.ProcessDefinitionDisabled ?? 0 },
    { label: '实例合计', value: o.InstanceTotal ?? 0 },
    { label: '运行中', value: o.InstanceRunning ?? 0 },
    { label: '已完成', value: o.InstanceCompleted ?? 0 },
    { label: '挂起', value: o.InstanceSuspended ?? 0 },
    { label: '待办任务', value: o.TaskReady ?? 0 },
    { label: '近7日已办', value: o.TaskCompletedRecent ?? 0 },
    { label: 'Outbox 死信', value: o.OutboxDead ?? 0 },
    { label: '请假审批中', value: o.LeaveRunning ?? 0 }
  ];
});

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const data = await getReportOverview(10);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = data?.Message || data?.message || '加载失败';
      overview.value = {};
      top.value = [];
      return;
    }
    const r = unpackSailResult<WorkflowReportOverview>(data);
    overview.value = r.entity || {};
    top.value = overview.value.TopProcesses || [];
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  } finally {
    loading.value = false;
  }
}

onMounted(load);
</script>

<style scoped>
.sf-report-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page, #f5f7fa);
}
.sf-report-page__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 16px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
}
.sf-report-page__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-report-page__actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.sf-report-page__main {
  flex: 1;
  padding: 16px;
}
.sf-report-page__alert {
  margin-bottom: 12px;
}
.sf-report-page__card {
  background: var(--el-bg-color, #fff);
  border: 1px solid var(--el-border-color-lighter, #ebeef5);
  border-radius: 6px;
  padding: 14px 16px;
  margin-bottom: 12px;
}
.sf-report-page__card-val {
  font-size: 22px;
  font-weight: 700;
  line-height: 1.2;
}
.sf-report-page__card-label {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.sf-report-page__h {
  margin: 8px 0 12px;
  font-size: 14px;
}
</style>
