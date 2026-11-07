<script setup lang="tsx">
import { ElButton, ElTag } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { fetchGetMyNoticePage, fetchMarkNoticeRead } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';

const searchParams = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const detailVisible = ref(false);
const current = ref<Api.SystemP1.MyNotice | null>(null);

const typeLabel: Record<number, string> = { 1: '公告', 2: '站内信' };

const { columns, columnChecks, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemP1.MyNotice>>,
  Api.SystemP1.MyNotice
>({
  api: () => fetchGetMyNoticePage(searchParams),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { key: 'title', title: '标题', minWidth: 160 },
    {
      key: 'type',
      title: '类型',
      width: 90,
      render: (row: Api.SystemP1.MyNotice) => typeLabel[row.type] ?? row.type
    },
    {
      key: 'isRead',
      title: '状态',
      width: 90,
      render: (row: Api.SystemP1.MyNotice) => (
        <ElTag type={row.isRead ? 'default' : 'warning'}>{row.isRead ? '已读' : '未读'}</ElTag>
      )
    },
    { key: 'publishTime', title: '时间', minWidth: 160 },
    {
      key: 'operate',
      title: '操作',
      width: 100,
      render: (row: Api.SystemP1.MyNotice) => (
        <ElButton size="small" type="primary" plain onClick={() => openDetail(row)}>
          查看
        </ElButton>
      )
    }
  ]
});

async function openDetail(row: Api.SystemP1.MyNotice) {
  current.value = row;
  detailVisible.value = true;
  if (!row.isRead) {
    await fetchMarkNoticeRead(row.id);
    await getData();
  }
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">我的通知</span>
          <TableHeaderOperation v-model:columns="columnChecks" :loading="loading" @refresh="getData" />
        </div>
      </template>
      <TableData
        :columns="columns"
        :data="data"
        :loading="loading"
        :pagination="mobilePagination"
        :row-key="(row: Api.SystemP1.MyNotice) => row.id"
        size="small"
        class="sm:h-full"
      />
    </ElCard>
    <ElDialog v-model="detailVisible" :title="current?.title ?? '通知详情'" width="640px">
      <div class="whitespace-pre-wrap">{{ current?.content }}</div>
    </ElDialog>
  </div>
</template>
