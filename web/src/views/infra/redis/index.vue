<script setup lang="tsx">
import { ElButton, ElPopconfirm } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { fetchDeleteRedisKey, fetchGetRedisKeyPage, fetchGetRedisKeyValue } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';

const searchParams = reactive({
  page: 1,
  pageSize: 20,
  keyword: null as string | null,
  pattern: '*'
});

const detailVisible = ref(false);
const detailLoading = ref(false);
const detail = ref<Api.SysInfra.RedisKeyValue | null>(null);

const { columns, columnChecks, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SysInfra.RedisKey>>,
  Api.SysInfra.RedisKey
>({
  api: () =>
    fetchGetRedisKeyPage({
      page: searchParams.page,
      pageSize: searchParams.pageSize,
      keyword: searchParams.keyword,
      pattern: searchParams.pattern
    }),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 20;
  },
  columns: () => [
    { key: 'key', title: 'Key', minWidth: 200, ellipsis: { tooltip: true } },
    { key: 'type', title: '类型', width: 100 },
    {
      key: 'ttlSeconds',
      title: 'TTL(秒)',
      width: 100,
      render: (row: Api.SysInfra.RedisKey) => (row.ttlSeconds == null ? '永久' : row.ttlSeconds)
    },
    {
      key: 'operate',
      title: '操作',
      width: 160,
      render: (row: Api.SysInfra.RedisKey) => (
        <div class="flex gap-8px">
          <ElButton size="small" type="primary" plain onClick={() => openDetail(row.key)}>
            查看
          </ElButton>
          <ElPopconfirm title={'确认删除该 Key？'} onConfirm={() => handleDelete(row.key)}>{{ reference: () => (
                <ElButton size="small" type="danger" plain>
                  删除
                </ElButton>
              )
            }}
          </ElPopconfirm>
        </div>
      )
    }
  ]
});

async function openDetail(key: string) {
  detailVisible.value = true;
  detailLoading.value = true;
  detail.value = null;
  try {
    const { data: val } = await fetchGetRedisKeyValue(key);
    detail.value = val ?? null;
  } finally {
    detailLoading.value = false;
  }
}

async function handleDelete(key: string) {
  await fetchDeleteRedisKey(key);
  window.$message?.success('已删除');
  await getData();
}

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
          <span class="font-medium">Redis 键值管理</span>
          <div class="flex items-center gap-8px">
            <ElInput v-model="searchParams.pattern" placeholder="匹配模式，如 sleveen-admin:*" class="w-200px" />
            <ElButton type="primary" :loading="loading" @click="handleSearch">查询</ElButton>
            <ElButton :loading="loading" @click="getData">刷新</ElButton>
          </div>
        </div>
      </template>
      <ElAlert type="info" class="mb-12px">最多扫描 5000 个 Key；删除操作仅超级管理员可用。</ElAlert>
      <TableData
        :columns="columns"
        :data="data"
        :loading="loading"
        :pagination="mobilePagination"
        :row-key="(row: Api.SysInfra.RedisKey) => row.key"
        size="small"
        class="sm:h-full"
      />
    </ElCard>

    <ElDialog v-model="detailVisible" title="Key 详情" width="640px">
      <div v-loading="detailLoading">
        <ElDescriptions v-if="detail" :column="1" label-position="left" border size="small">
          <ElDescriptionsItem label="Key">{{ detail.key }}</ElDescriptionsItem>
          <ElDescriptionsItem label="类型">{{ detail.type }}</ElDescriptionsItem>
          <ElDescriptionsItem label="TTL">{{ detail.ttlSeconds ?? '永久' }}</ElDescriptionsItem>
          <ElDescriptionsItem label="值">
            <pre class="max-h-300px overflow-auto whitespace-pre-wrap text-12px">{{ detail.value }}</pre>
          </ElDescriptionsItem>
        </ElDescriptions>
      </div>
    </ElDialog>
  </div>
</template>
