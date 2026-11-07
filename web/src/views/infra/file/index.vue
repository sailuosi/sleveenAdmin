<script setup lang="tsx">
import { ElButton, ElPopconfirm } from 'element-plus';
import type { UploadRequestOptions } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { downloadFile, fetchDeleteFile, fetchGetFilePage, fetchUploadFile } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';

const searchParams = reactive({ page: 1, pageSize: 10, keyword: null as string | null });

function formatSize(size: number) {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
  return `${(size / 1024 / 1024).toFixed(2)} MB`;
}

const { columns, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SysInfra.SysFile>>,
  Api.SysInfra.SysFile
>({
  api: () => fetchGetFilePage(searchParams),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { key: 'originalName', title: '文件名', minWidth: 180 },
    { key: 'extension', title: '类型', width: 80 },
    { key: 'size', title: '大小', width: 100, render: (row: Api.SysInfra.SysFile) => formatSize(row.size) },
    { key: 'url', title: '访问地址', minWidth: 200, ellipsis: { tooltip: true } },
    { key: 'createTime', title: '上传时间', width: 170 },
    {
      key: 'operate',
      title: $t('common.operate'),
      width: 180,
      render: (row: Api.SysInfra.SysFile) => (
        <div class="flex gap-8px">
          <ElButton size="small" type="primary" plain onClick={() => handleDownload(row)}>
            下载
          </ElButton>
          <ElPopconfirm title={$t('common.confirmDelete')} onConfirm={() => handleDelete(row.id)}>{{ reference: () => (
                <ElButton size="small" type="danger" plain>
                  {$t('common.delete')}
                </ElButton>
              )
            }}
          </ElPopconfirm>
        </div>
      )
    }
  ]
});

async function handleDownload(row: Api.SysInfra.SysFile) {
  try {
    await downloadFile(row.id, row.originalName);
  } catch {
    window.$message?.error('下载失败');
  }
}

async function handleDelete(id: string) {
  await fetchDeleteFile(id);
  window.$message?.success($t('common.deleteSuccess'));
  await getData();
}

async function handleUpload(options: UploadRequestOptions) {
  try {
    await fetchUploadFile(options.file as File);
    window.$message?.success('上传成功');
    options.onSuccess?.({});
    await getData();
  } catch {
    window.$message?.error('上传失败');
    options.onError?.(new Error('upload failed'));
  }
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">文件管理</span>
          <div class="flex items-center gap-12px">
            <ElInput
              v-model="searchParams.keyword"
              placeholder="搜索文件名"
              clearable
              class="w-200px"
              @keyup.enter="getData"
            />
            <ElButton @click="getData">{{ $t('common.refresh') }}</ElButton>
            <ElUpload :show-file-list="false" :http-request="handleUpload">
              <ElButton type="primary">上传文件</ElButton>
            </ElUpload>
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
