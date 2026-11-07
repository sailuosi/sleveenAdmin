<template>
  <div class="sf-leave-page">
    <header class="sf-leave-page__bar">
      <div class="sf-leave-page__title">请假样例</div>
      <div class="sf-leave-page__actions">
        <el-button :loading="loading" @click="load">刷新</el-button>
        <el-button @click="router.push('/sail/templates')">模板库</el-button>
        <el-button @click="router.push('/sail/task-todo')">待办</el-button>
        <el-button type="primary" @click="openCreate">新建请假</el-button>
      </div>
    </header>
    <main class="sf-leave-page__main">
      <el-alert
        title="业务只认 StartProcess + 结束态 Hook 回写 Status（通过=2 / 驳回=3 / 关闭=4）；不引 NFlow EventBus。"
        type="info"
        show-icon
        :closable="false"
        class="sf-leave-page__alert"
      />
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-leave-page__alert" />
      <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无请假单">
        <el-table-column prop="Id" label="单号" min-width="160" show-overflow-tooltip />
        <el-table-column prop="UserName" label="申请人" width="100" />
        <el-table-column prop="Days" label="天数" width="80" />
        <el-table-column prop="Reason" label="事由" min-width="160" show-overflow-tooltip />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.Status)" size="small">{{ leaveStatusLabel(row.Status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="ProcessInstanceId" label="流程实例" width="110" />
        <el-table-column prop="CreatedAt" label="创建时间" width="170" />
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="row.ProcessInstanceId"
              link
              type="primary"
              @click="goDetail(row)"
            >
              办理详情
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </main>

    <el-dialog v-model="createVisible" title="新建请假" width="520px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="流程定义" required>
          <el-select
            v-model="draft.ProcessId"
            filterable
            placeholder="选择已启用的流程（可用 AskforLeave 模板创建）"
            style="width: 100%"
            @change="onProcessPick"
          >
            <el-option
              v-for="p in processes"
              :key="String(field(p, 'ProcessId', 'processId'))"
              :label="`${field(p, 'ProcessName', 'processName')} · v${field(p, 'Version', 'version')}`"
              :value="String(field(p, 'ProcessId', 'processId'))"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="天数" required>
          <el-input-number v-model="draft.Days" :min="0.5" :step="0.5" />
        </el-form-item>
        <el-form-item label="开始日期">
          <el-date-picker v-model="draft.StartDate" type="date" value-format="YYYY-MM-DD" />
        </el-form-item>
        <el-form-item label="结束日期">
          <el-date-picker v-model="draft.EndDate" type="date" value-format="YYYY-MM-DD" />
        </el-form-item>
        <el-form-item label="事由">
          <el-input v-model="draft.Reason" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onCreate">提交并发起</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { createLeave, leaveStatusLabel, queryLeave, type LeaveRow } from '@sail/api/leave';
import { createProcessByTemplate, getProcessListSimple } from '@sail/api/process';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailLeavePage' });

const router = useRouter();
const { userId, userName } = useSailCurrentUser();

const loading = ref(false);
const saving = ref(false);
const err = ref('');
const rows = ref<LeaveRow[]>([]);
const processes = ref<Record<string, unknown>[]>([]);
const createVisible = ref(false);
const draft = reactive({
  ProcessId: '',
  Version: '1',
  Days: 1,
  Reason: '',
  StartDate: '' as string,
  EndDate: '' as string
});

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}

function statusType(status?: number) {
  switch (Number(status)) {
    case 2:
      return 'success';
    case 3:
      return 'danger';
    case 4:
      return 'info';
    default:
      return 'warning';
  }
}

async function loadProcesses() {
  const data = await getProcessListSimple();
  const list = data?.Entity ?? data?.entity ?? data;
  const all = Array.isArray(list) ? list : [];
  processes.value = all.filter(
    (p: Record<string, unknown>) => Number(field(p, 'Status', 'status')) === 1
  );
  if (!processes.value.length && all.length) {
    processes.value = all;
  }
}

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const data = await queryLeave({ UserId: userId.value || undefined, Limit: 100 });
    if ((data?.Status ?? data?.status) === -1) {
      err.value = data?.Message || data?.message || '加载失败';
      rows.value = [];
      return;
    }
    const r = unpackSailResult<LeaveRow[]>(data);
    rows.value = Array.isArray(r.entity) ? r.entity : [];
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function onProcessPick(processId: string) {
  const p = processes.value.find(
    x => String(field(x, 'ProcessId', 'processId')) === processId
  );
  draft.Version = String(field(p || {}, 'Version', 'version') || '1');
}

async function openCreate() {
  await loadProcesses();
  if (!processes.value.length) {
    try {
      ElMessage.info('暂无流程，正在从 AskforLeave 模板创建…');
      const data = await createProcessByTemplate('AskforLeave');
      if ((data?.Status ?? data?.status) === -1) {
        ElMessage.error(data?.Message || '模板创建失败');
        return;
      }
      await loadProcesses();
    } catch (e: unknown) {
      ElMessage.error(e instanceof Error ? e.message : String(e));
      return;
    }
  }
  draft.ProcessId = String(field(processes.value[0] || {}, 'ProcessId', 'processId'));
  onProcessPick(draft.ProcessId);
  draft.Days = 1;
  draft.Reason = '';
  draft.StartDate = '';
  draft.EndDate = '';
  createVisible.value = true;
}

async function onCreate() {
  if (!draft.ProcessId) {
    ElMessage.warning('请选择流程');
    return;
  }
  if (draft.Days <= 0) {
    ElMessage.warning('天数须大于 0');
    return;
  }
  saving.value = true;
  try {
    const data = await createLeave({
      UserId: userId.value,
      UserName: userName.value,
      Days: draft.Days,
      Reason: draft.Reason,
      StartDate: draft.StartDate || undefined,
      EndDate: draft.EndDate || undefined,
      ProcessId: draft.ProcessId,
      Version: draft.Version
    });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '发起失败');
      return;
    }
    ElMessage.success('已提交并发起流程');
    createVisible.value = false;
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

function goDetail(row: LeaveRow) {
  router.push({
    path: '/sail/process-detail',
    query: { processInstanceId: String(row.ProcessInstanceId) }
  });
}

onMounted(load);
</script>

<style scoped>
.sf-leave-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page, #f5f7fa);
}
.sf-leave-page__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 16px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
}
.sf-leave-page__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-leave-page__actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.sf-leave-page__main {
  flex: 1;
  padding: 12px 16px;
}
.sf-leave-page__alert {
  margin-bottom: 12px;
}
</style>
