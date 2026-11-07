<script setup lang="tsx">
import { ElButton, ElTag } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { fetchGetVisLogPage } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';

const searchParams = reactive({
  page: 1,
  pageSize: 10,
  keyword: null as string | null,
  startTime: null as string | null,
  endTime: null as string | null
});

const { columns, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SysInfra.SysLogVis>>,
  Api.SysInfra.SysLogVis
>({
  api: () => fetchGetVisLogPage(searchParams),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { key: 'account', title: '账号', width: 100 },
    { key: 'actionName', title: '接口', minWidth: 180, ellipsis: { tooltip: true } },
    { key: 'httpMethod', title: '方法', width: 72 },
    { key: 'requestUrl', title: '请求地址', minWidth: 200, ellipsis: { tooltip: true } },
    {
      key: 'statusCode',
      title: '状态码',
      width: 90,
      render: (row: Api.SysInfra.SysLogVis) => (
        <ElTag type={row.statusCode < 400 ? 'success' : 'warning'} size="small">
          {row.statusCode}
        </ElTag>
      )
    },
    { key: 'elapsedMs', title: '耗时(ms)', width: 90 },
    { key: 'ip', title: 'IP', width: 120 },
    { key: 'createTime', title: '时间', width: 170 }
  ]
});

function handleSearch() {
  searchParams.page = 1;
  getData();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">访问日志</span>
          <div class="flex flex-wrap items-center gap-12px">
            <ElInput v-model="searchParams.keyword" placeholder="账号/URL" clearable class="w-180px" />
            <ElDatePicker
              v-model="searchParams.startTime"
              type="datetime"
              clearable
              placeholder="开始时间"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
            <ElDatePicker
              v-model="searchParams.endTime"
              type="datetime"
              clearable
              placeholder="结束时间"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
            <ElButton type="primary" @click="handleSearch">查询</ElButton>
            <ElButton @click="getData">刷新</ElButton>
          </div>
        </div>
      </template>
      <TableData
        :columns="columns"
        :data="data"
        :loading="loading"
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-full"
      />
    </ElCard>
  </div>
</template>

<style scoped></style>
