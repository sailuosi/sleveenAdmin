<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import dayjs from 'dayjs';
import { todoList_Init } from '@f/client/api/basis/toDoList';
import { getRunninglist } from '@f/client/api/basis/run';
import { Complete_Init } from '@f/client/api/basis/complete.js';
import { getCCsendlist } from '@f/client/api/form/send.js';
import todolistPage from '@f/client/views/pages/basis/todolistPage.vue';
import runingPage from '@f/client/views/pages/basis/runingPage.vue';
import completePage from '@f/client/views/pages/basis/completePage.vue';
import mdialog from '@/ui/common/modal/mdialog.vue';
import { Refresh } from '@element-plus/icons-vue';
import { openHomePortalTask, type HomeTaskKey } from '../utils/homePortalTaskFlow';

defineOptions({ name: 'HomePortalTaskPanel' });

interface TaskTabDef {
  key: HomeTaskKey;
  title: string;
  shortTitle: string;
  color: string;
  visible: boolean;
  timeLabel: string;
  timeField: string;
}

const channelId = import.meta.env.VITE_CCflowSysID;

const tabDefs: TaskTabDef[] = [
  { key: 'todo', title: '我的待办', shortTitle: '待办', color: '#f15643', visible: true, timeLabel: '期限', timeField: 'SDT' },
  { key: 'running', title: '我的在办', shortTitle: '在办', color: '#ff943d', visible: true, timeLabel: '到达', timeField: 'RDT' },
  { key: 'finished', title: '我的已办', shortTitle: '已办', color: '#00b853', visible: true, timeLabel: '完成', timeField: 'SendDT' },
  { key: 'toRead', title: '我的待阅', shortTitle: '待阅', color: '#f15643', visible: false, timeLabel: '时间', timeField: 'RDT' },
  { key: 'readed', title: '我的已阅', shortTitle: '已阅', color: '#00b853', visible: false, timeLabel: '时间', timeField: 'RDT' },
];

const visibleTabs = computed(() => tabDefs.filter(tab => tab.visible));
const activeKey = ref<HomeTaskKey>('todo');
const dialogVisible = ref(false);

const baseQuery = {
  FlowName: '',
  RageDate: '',
  channelId,
};

const tabPageState = reactive<Record<HomeTaskKey, { pageNum: number; pageSize: number }>>({
  todo: { pageNum: 1, pageSize: 10 },
  running: { pageNum: 1, pageSize: 10 },
  finished: { pageNum: 1, pageSize: 10 },
  toRead: { pageNum: 1, pageSize: 10 },
  readed: { pageNum: 1, pageSize: 10 },
});

const taskData = reactive<Record<HomeTaskKey, { total: number; list: Record<string, any>[]; loading: boolean }>>({
  todo: { total: 0, list: [], loading: false },
  running: { total: 0, list: [], loading: false },
  finished: { total: 0, list: [], loading: false },
  toRead: { total: 0, list: [], loading: false },
  readed: { total: 0, list: [], loading: false },
});

const currentTab = computed(() => tabDefs.find(tab => tab.key === activeKey.value)!);
const currentList = computed(() => taskData[activeKey.value].list);
const currentTotal = computed(() => taskData[activeKey.value].total);
const currentLoading = computed(() => taskData[activeKey.value].loading);
const currentPageState = computed(() => tabPageState[activeKey.value]);

const tableAreaRef = ref<HTMLElement | null>(null);
const tableHeight = ref(200);
let resizeObserver: ResizeObserver | null = null;

async function fetchTaskList(key: HomeTaskKey, pageNum: number, pageSize: number) {
  const params = { ...baseQuery, pageNum, pageSize };
  if (key === 'todo') return todoList_Init(params);
  if (key === 'running') return getRunninglist(params);
  if (key === 'finished') return Complete_Init(params);
  if (key === 'toRead') return getCCsendlist({ ...params, Sta: '0', Title: '', Rec: '' });
  return getCCsendlist({ ...params, Sta: '1', Title: '', Rec: '' });
}

async function loadActiveList() {
  const key = activeKey.value;
  const { pageNum, pageSize } = tabPageState[key];
  taskData[key].loading = true;
  try {
    const res: any = await fetchTaskList(key, pageNum, pageSize);
    if (res?.code === 200) {
      taskData[key].total = res.data?.total ?? 0;
      taskData[key].list = res.data?.records ?? [];
    }
  } finally {
    taskData[key].loading = false;
  }
}

async function loadAllTabCounts() {
  const jobs = tabDefs.map(async (tab) => {
    const res: any = await fetchTaskList(tab.key, 1, 1);
    if (res?.code === 200) {
      taskData[tab.key].total = res.data?.total ?? 0;
    }
  });
  await Promise.all(jobs);
}

function switchTab(key: HomeTaskKey) {
  activeKey.value = key;
}

function handlePageChange(page: number) {
  tabPageState[activeKey.value].pageNum = page;
  loadActiveList();
}

function handleSizeChange(size: number) {
  tabPageState[activeKey.value].pageSize = size;
  tabPageState[activeKey.value].pageNum = 1;
  loadActiveList();
}

function openFullDialog() {
  dialogVisible.value = true;
}

function refreshPanel() {
  loadAllTabCounts();
  loadActiveList();
}

function refreshAfterDialog() {
  refreshPanel();
}

function formatRowTime(row: Record<string, any>, field: string) {
  const value = row[field];
  if (!value) return '--';
  if (field === 'RDT') {
    const parsed = dayjs(value);
    return parsed.isValid() ? parsed.format('YYYY-MM-DD HH:mm') : value;
  }
  return value;
}

function handleOpenTask(row: Record<string, any>) {
  openHomePortalTask(row, activeKey.value);
}

function onVisibilityChange() {
  if (document.visibilityState === 'visible') {
    loadAllTabCounts();
    loadActiveList();
  }
}

function bindTableResizeObserver() {
  resizeObserver?.disconnect();
  if (!tableAreaRef.value) return;
  resizeObserver = new ResizeObserver((entries) => {
    const height = entries[0]?.contentRect.height ?? 200;
    tableHeight.value = Math.max(120, Math.floor(height));
  });
  resizeObserver.observe(tableAreaRef.value);
}

watch(activeKey, () => {
  loadActiveList();
  nextTick(bindTableResizeObserver);
});

onMounted(async () => {
  await loadAllTabCounts();
  await loadActiveList();
  nextTick(bindTableResizeObserver);
  document.addEventListener('visibilitychange', onVisibilityChange);
});

onUnmounted(() => {
  resizeObserver?.disconnect();
  document.removeEventListener('visibilitychange', onVisibilityChange);
});
</script>

<template>
  <section class="card home-task-card">
    <div class="card-header">
      <span>流程待办</span>
      <div class="card-header-right">
        <span class="card-hint">待办 / 在办 / 已办</span>
        <el-button type="primary" link size="small" @click="openFullDialog">查看更多</el-button>
        <el-button
          type="success"
          circle
          size="small"
          class="task-refresh-btn"
          :disabled="currentLoading"
          @click="refreshPanel"
          title="刷新"
        >
          <el-icon :class="{ 'is-loading': currentLoading }"><Refresh /></el-icon>
        </el-button>
      </div>
    </div>
    <div class="card-body task-panel-body">
      <div class="task-panel-inner">
        <div class="task-tabs">
          <button
            v-for="tab in visibleTabs"
            :key="tab.key"
            type="button"
            class="task-tab"
            :class="{ active: activeKey === tab.key }"
            :style="{ '--tab-color': tab.color }"
            @click="switchTab(tab.key)"
          >
            <span class="task-tab-num">{{ taskData[tab.key].total }}</span>
            <span class="task-tab-label">{{ tab.shortTitle }}</span>
          </button>
        </div>
        <div class="task-table-wrap">
          <div ref="tableAreaRef" class="task-table-area">
            <el-table
              v-loading="currentLoading"
              :data="currentList"
              stripe
              size="small"
              :height="tableHeight"
              :empty-text="`暂无${currentTab.shortTitle}`"
            >
              <el-table-column prop="Title" label="标题" min-width="160" show-overflow-tooltip>
                <template #default="{ row }">
                  <el-link type="primary" :underline="false" @click="handleOpenTask(row)">
                    {{ row.Title }}
                  </el-link>
                </template>
              </el-table-column>
              <el-table-column
                :prop="currentTab.timeField"
                :label="currentTab.timeLabel"
                width="118"
                align="center"
              >
                <template #default="{ row }">
                  <span class="task-time">{{ formatRowTime(row, currentTab.timeField) }}</span>
                </template>
              </el-table-column>
            </el-table>
          </div>
          <div class="task-table-footer">
            <el-pagination
              :current-page="currentPageState.pageNum"
              :page-size="currentPageState.pageSize"
              :total="currentTotal"
              :page-sizes="[10, 20, 30, 50]"
              layout="total, sizes, prev, pager, next"
              small
              background
              @current-change="handlePageChange"
              @size-change="handleSizeChange"
            />
          </div>
        </div>
      </div>
    </div>
  </section>

  <mdialog
    :title="currentTab.title"
    class="normal"
    v-model="dialogVisible"
    width="80vw"
    top="10vh"
    destroy-on-close
    @close="refreshAfterDialog"
  >
    <div class="hidden-mdialog-inner home-task-ccflow-wrap">
      <div class="hidden-edit-form-shell">
        <todolistPage v-if="dialogVisible && activeKey === 'todo'" />
        <runingPage v-else-if="dialogVisible && activeKey === 'running'" />
        <completePage v-else-if="dialogVisible && activeKey === 'finished'" />
      </div>
    </div>
  </mdialog>
</template>

<style scoped>
.home-task-card {
  --line: #dce7f5;
  --text: #233243;
  --shadow: 0 4px 12px rgba(31, 111, 209, 0.08);
  --radius: 12px;

  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  background: #fff;
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  border: 1px solid #e3ebf5;
  overflow: hidden;
}

.card-header {
  flex-shrink: 0;
  height: 42px;
  padding: 0 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: linear-gradient(95deg, #f5f9ff 0%, #f0f7ff 45%, #fafdff 100%);
  border-bottom: 1px solid var(--line);
  font-weight: 600;
  color: var(--text);
  font-size: 14px;
}

.card-header-right {
  display: flex;
  align-items: center;
  gap: 5px;
}

.task-refresh-btn .el-icon.is-loading {
  animation: rotating 2s linear infinite;
}

.card-hint {
  font-size: 12px;
  color: #7f8ea2;
  font-weight: 400;
}

.task-panel-body {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding: 10px 12px 12px;
}

.task-panel-inner {
  flex: 1;
  min-height: 0;
  display: grid;
  grid-template-columns: 72px minmax(0, 1fr);
  gap: 10px;
}

.task-tabs {
  display: flex;
  flex-direction: column;
  gap: 8px;
  height: 100%;
  min-height: 0;
  padding: 8px 6px;
  border-radius: 10px;
  background: linear-gradient(180deg, #e9f4ff 0%, #f6fbff 55%, #fff 100%);
  border: 1px solid #e3ebf5;
}

.task-tab {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  width: 100%;
  min-height: 0;
  padding: 8px 4px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: transparent;
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s, box-shadow 0.2s;
}

.task-tab:hover {
  background: rgba(255, 255, 255, 0.72);
}

.task-tab.active {
  background: #fff;
  border-color: #d7e6f8;
  box-shadow: 0 2px 8px rgba(31, 111, 209, 0.08);
}

.task-tab-num {
  font-size: 20px;
  font-weight: 700;
  line-height: 1;
  color: var(--tab-color, #2f89f5);
}

.task-tab-label {
  font-size: 12px;
  color: #5f7084;
  font-weight: 600;
}

.task-table-wrap {
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  height: 100%;
  padding: 10px 12px;
  border-radius: 10px;
  background: #fff;
  border: 1px solid #e3ebf5;
}

.task-table-area {
  flex: 1;
  min-height: 0;
}

.task-table-wrap :deep(.el-table) {
  --el-table-header-bg-color: #f5f9ff;
  --el-table-border-color: #e8eef5;
  font-size: 13px;
}

.task-table-wrap :deep(.el-table__header th) {
  color: #5f7084;
  font-weight: 600;
}

.task-time {
  color: #8a99ab;
  font-size: 12px;
}

.task-table-footer {
  flex-shrink: 0;
  display: flex;
  justify-content: flex-end;
  padding-top: 8px;
}

.hidden-mdialog-inner {
  display: flex;
  flex-direction: column;
  min-height: 0;
  height: 100%;
  box-sizing: border-box;
}

.hidden-edit-form-shell {
  flex: 1;
  min-height: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.home-task-ccflow-wrap {
  flex: 1;
  min-height: 0;
  height: 75vh;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

.home-task-ccflow-wrap :deep(.foo-pan) {
  margin: 0 !important;
  width: 100% !important;
  height: 100%;
  flex: 1;
  min-height: 0;
}

.home-task-ccflow-wrap :deep(.flexpan.flexed) {
  height: 100%;
  min-height: 0;
}

.home-task-ccflow-wrap :deep(.vclist-inner > .el-pagination.vcpager) {
  display: none !important;
}

/* 弹窗内 ccflow 列表表头：白底黑字（:deep 穿透 ccflow 页 scoped 蓝底样式） */
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table) {
  --sum-header-bg: #fff !important;
  --sum-header-color: #1f2d3d !important;
}

.home-task-ccflow-wrap :deep(div.vchead.noquery),
.home-task-ccflow-wrap :deep(.vchead.noquery .vccap) {
  background-color: #fff !important;
  color: #1f2d3d !important;
  border-bottom: 1px solid #e3ebf5 !important;
}

.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper th.el-table__cell),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__fixed-header-wrapper th.el-table__cell),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper th.indexcol),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__fixed-header-wrapper th.indexcol) {
  background: #fff !important;
  background-color: #fff !important;
  border-color: #e3ebf5 !important;
  color: #1f2d3d !important;
  font-size: 14px;
  font-weight: 600;
}

.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper .vcthead.nqy),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__fixed-header-wrapper .vcthead.nqy),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper th > .cell.vcthead),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__fixed-header-wrapper th > .cell.vcthead) {
  background-color: #fff !important;
  color: #1f2d3d !important;
}

.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .vccap),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper .cell),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__fixed-header-wrapper .cell),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper th > .cell .vccap) {
  color: #1f2d3d !important;
  background-color: transparent !important;
}

.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .el-table__header-wrapper .caret-wrapper),
.home-task-ccflow-wrap :deep(.vclist.sum-entry-table .vcthead > .caret-wrapper) {
  color: #6f7f92 !important;
}
</style>
