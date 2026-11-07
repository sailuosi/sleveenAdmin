<script setup lang="tsx">
import { ElButton, ElPopconfirm, ElTag, ElSpace } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { fetchCreateNotice, fetchDeleteNotice, fetchGetNoticePage, fetchPublishNotice } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';

const searchParams = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const showModal = ref(false);
const formModel = reactive<Api.SystemP1.NoticeInput>({ title: '', content: '', type: 1 });

const typeLabel: Record<number, string> = { 1: '公告', 2: '站内信' };
const statusLabel: Record<number, string> = { 1: '草稿', 2: '已发布' };
const noticeTypeOptions = [
  { label: '公告', value: 1 },
  { label: '站内信', value: 2 }
];

const { columns, columnChecks, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemP1.Notice>>,
  Api.SystemP1.Notice
>({
  api: () => fetchGetNoticePage(searchParams),
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
      render: (row: Api.SystemP1.Notice) => typeLabel[row.type] ?? row.type
    },
    {
      key: 'status',
      title: '状态',
      width: 90,
      render: (row: Api.SystemP1.Notice) => (
        <ElTag type={row.status === 2 ? 'success' : 'default'}>{statusLabel[row.status] ?? row.status}</ElTag>
      )
    },
    { key: 'publishTime', title: '发布时间', minWidth: 160 },
    {
      key: 'operate',
      title: '操作',
      width: 200,
      render: (row: Api.SystemP1.Notice) => (
        <div class="flex gap-8px">
          {row.status === 1 && (
            <ElButton size="small" type="primary" plain onClick={() => handlePublish(row.id)}>
              发布
            </ElButton>
          )}
          <ElPopconfirm title={'确认删除？'} onConfirm={() => handleDelete(row.id)}>{{ reference: () => (
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

async function handleCreate() {
  await fetchCreateNotice(formModel);
  window.$message?.success('创建成功');
  showModal.value = false;
  formModel.title = '';
  formModel.content = '';
  formModel.type = 1;
  await getData();
}

async function handlePublish(id: string) {
  await fetchPublishNotice(id);
  window.$message?.success('已发布');
  await getData();
}

async function handleDelete(id: string) {
  await fetchDeleteNotice(id);
  window.$message?.success('已删除');
  await getData();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">通知管理</span>
          <TableHeaderOperation v-model:columns="columnChecks" :loading="loading" @add="showModal = true" @refresh="getData" />
        </div>
      </template>
      <TableData
        :columns="columns"
        :data="data"
        :loading="loading"
        :pagination="mobilePagination"
        :row-key="(row: Api.SystemP1.Notice) => row.id"
        size="small"
        class="sm:h-full"
      />
    </ElCard>
    <ElDialog v-model="showModal" title="新建通知" width="600px">
      <ElForm label-position="left" label-width="80">
        <ElFormItem label="标题"><ElInput v-model="formModel.title" /></ElFormItem>
        <ElFormItem label="类型">
          <ElSelect v-model="formModel.type" class="w-full">
            <ElOption
              v-for="opt in noticeTypeOptions"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </ElSelect>
        </ElFormItem>
        <ElFormItem label="内容"><ElInput v-model="formModel.content" type="textarea" :rows="6" /></ElFormItem>
      </ElForm>
      <template #footer>
        <ElSpace class="w-full justify-end">
          <ElButton @click="showModal = false">取消</ElButton>
          <ElButton type="primary" @click="handleCreate">确定</ElButton>
        </ElSpace>
      </template>
    </ElDialog>
  </div>
</template>
