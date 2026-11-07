<template>
  <div class="sf-list-page">
    <header class="sf-list-page__bar">
      <div class="sf-list-page__title">Sailflow 流程定义</div>
      <div class="sf-list-page__actions">
        <el-button :loading="loading" @click="load">刷新</el-button>
        <el-button @click="importDialogRef?.open()">导入</el-button>
        <el-button @click="templateDialogRef?.open()">从模板创建</el-button>
        <el-button @click="router.push('/sail/task-todo')">待办</el-button>
        <el-button @click="router.push('/sail/outbox')">死信</el-button>
        <el-button @click="router.push('/sail/templates')">模板库</el-button>
        <el-button @click="router.push('/sail/categories')">分类</el-button>
        <el-button @click="router.push('/sail/report')">报表</el-button>
        <el-button @click="router.push('/sail/leave')">请假</el-button>
        <el-button @click="router.push('/sail/forms')">表单</el-button>
        <el-button @click="router.push('/sail/knowledge')">知识库</el-button>
        <el-button type="primary" @click="goCreate()">新建流程</el-button>
        <el-button @click="goDesigner()">打开设计器</el-button>
      </div>
    </header>
    <main class="sf-list-page__body">
      <el-alert
        v-if="err"
        :title="err"
        type="error"
        show-icon
        :closable="false"
        class="sf-list-page__alert"
      />
      <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无流程（可先调 Probe/init-db）">
        <el-table-column label="名称" min-width="160">
          <template #default="{ row }">{{ field(row, 'ProcessName', 'processName') }}</template>
        </el-table-column>
        <el-table-column label="编码" width="140">
          <template #default="{ row }">{{ field(row, 'ProcessCode', 'processCode') }}</template>
        </el-table-column>
        <el-table-column label="版本" width="80">
          <template #default="{ row }">{{ field(row, 'Version', 'version') }}</template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="Number(field(row, 'Status', 'status')) === 1 ? 'success' : 'info'" size="small">
              {{ Number(field(row, 'Status', 'status')) === 1 ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="ProcessId" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">{{ field(row, 'ProcessId', 'processId') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="360" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="goEdit(row)">修改</el-button>
            <el-button link @click="goCopy(row)">复制</el-button>
            <el-button link type="primary" @click="goDesigner(row)">设计</el-button>
            <el-button
              link
              type="success"
              v-if="Number(field(row, 'Status', 'status')) !== 1"
              @click="setUsing(row, 1)"
            >
              启用
            </el-button>
            <el-button
              link
              type="warning"
              v-else
              @click="setUsing(row, 0)"
            >
              停用
            </el-button>
            <el-button link @click="onUpgrade(row)">升级</el-button>
            <el-button link type="danger" @click="onDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </main>
    <ImportProcessDialog ref="importDialogRef" @imported="load" />
    <TemplateGalleryDialog ref="templateDialogRef" @created="load" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import {
  deleteProcess,
  deleteProcessById,
  getProcessListSimple,
  updateProcessUsingState,
  upgradeProcess
} from '@sail/api/process';
import { pickField } from '@sail/utils/sailResult';
import ImportProcessDialog from '@sail/components/ImportProcessDialog.vue';
import TemplateGalleryDialog from '@sail/components/TemplateGalleryDialog.vue';

defineOptions({ name: 'SailProcessListPage' });

const router = useRouter();
const loading = ref(false);
const err = ref('');
const rows = ref<Record<string, unknown>[]>([]);
const importDialogRef = ref<InstanceType<typeof ImportProcessDialog> | null>(null);
const templateDialogRef = ref<InstanceType<typeof TemplateGalleryDialog> | null>(null);

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}

async function load() {
  loading.value = true;
  err.value = '';
  try {
    const data = await getProcessListSimple();
    const status = data?.Status ?? data?.status;
    if (status === -1 || status === '-1') {
      err.value = data?.Message || data?.message || '列表失败';
      rows.value = [];
      return;
    }
    const list = data?.Entity ?? data?.entity ?? data?.EntityList ?? data?.entityList ?? data;
    rows.value = Array.isArray(list) ? list : [];
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function goCreate() {
  router.push({ path: '/sail/process-edit', query: { type: 'create' } });
}

function goEdit(row: Record<string, unknown>) {
  router.push({
    path: '/sail/process-edit',
    query: {
      type: 'update',
      processId: String(field(row, 'ProcessId', 'processId')),
      version: String(field(row, 'Version', 'version') || '1')
    }
  });
}

function goCopy(row: Record<string, unknown>) {
  router.push({
    path: '/sail/process-edit',
    query: {
      type: 'copy',
      processId: String(field(row, 'ProcessId', 'processId')),
      version: String(field(row, 'Version', 'version') || '1')
    }
  });
}

function goDesigner(row?: Record<string, unknown>) {
  const processId = row ? field(row, 'ProcessId', 'processId') : undefined;
  const version = row ? field(row, 'Version', 'version') || '1' : undefined;
  const q = processId ? { processId: String(processId), version: String(version) } : undefined;
  router.push({ path: '/sail/designer', query: q });
}

async function setUsing(row: Record<string, unknown>, usingState: number) {
  try {
    const data = await updateProcessUsingState({
      ProcessId: String(field(row, 'ProcessId', 'processId')),
      Version: String(field(row, 'Version', 'version') || '1'),
      UsingState: usingState
    });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '操作失败');
      return;
    }
    ElMessage.success(usingState === 1 ? '已启用' : '已停用');
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onUpgrade(row: Record<string, unknown>) {
  const ver = String(field(row, 'Version', 'version') || '1');
  try {
    await ElMessageBox.confirm(`将从版本 ${ver} 升级到下一版本？`, '升级流程', { type: 'info' });
  } catch {
    return;
  }
  try {
    const data = await upgradeProcess({
      ProcessId: String(field(row, 'ProcessId', 'processId')),
      Version: ver
    });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '升级失败');
      return;
    }
    ElMessage.success(data?.Message || data?.message || '升级成功');
    load();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onDelete(row: Record<string, unknown>) {
  const name = field(row, 'ProcessName', 'processName');
  try {
    await ElMessageBox.confirm(`确认删除流程「${name}」？此操作不可恢复。`, '删除', { type: 'warning' });
  } catch {
    return;
  }
  try {
    const id = field(row, 'Id', 'id');
    const data = id
      ? await deleteProcessById(String(id))
      : await deleteProcess({
          ProcessId: String(field(row, 'ProcessId', 'processId')),
          Version: String(field(row, 'Version', 'version') || '1')
        });
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
  padding: 12px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
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
}
.sf-list-page__body {
  padding: 20px;
}
.sf-list-page__alert {
  margin-bottom: 16px;
}
</style>
