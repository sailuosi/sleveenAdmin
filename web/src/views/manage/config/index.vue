<script setup lang="tsx">
import { ElButton, ElPopconfirm, ElSpace } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { fetchCreateConfig, fetchDeleteConfig, fetchGetConfigPage, fetchUpdateConfig } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';

const searchParams = reactive({ page: 1, pageSize: 10, keyword: null as string | null });

const { columns, columnChecks, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemManage.Config>>,
  Api.SystemManage.Config
>({
  api: () => fetchGetConfigPage(searchParams),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { key: 'id', title: 'ID', width: 0, render: () => null },
    { key: 'code', title: '编码', minWidth: 140 },
    { key: 'value', title: '值', minWidth: 200 },
    { key: 'remark', title: '备注', minWidth: 160 },
    {
      key: 'operate',
      title: $t('common.operate'),
      width: 160,
      render: (row: Api.SystemManage.Config) => (
        <div class="flex gap-8px">
          <ElButton size="small" type="primary" plain onClick={() => handleEdit(row.id)}>
            {$t('common.edit')}
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

const { drawerVisible, operateType, editingData, handleAdd, handleEdit, checkedRowKeys, onBatchDeleted, onDeleted } =
  useTableOperate(data, 'id', getData);

const formModel = reactive<Api.SystemManage.ConfigInput>({ code: '', value: '', remark: '' });

watch(editingData, val => {
  if (val && operateType.value === 'edit') {
    formModel.id = val.id;
    formModel.code = val.code;
    formModel.value = val.value;
    formModel.remark = val.remark;
  } else if (operateType.value === 'add') {
    formModel.id = null;
    formModel.code = '';
    formModel.value = '';
    formModel.remark = '';
  }
});

async function handleSubmit() {
  if (operateType.value === 'add') await fetchCreateConfig(formModel);
  else await fetchUpdateConfig(formModel);
  window.$message?.success($t('common.updateSuccess'));
  drawerVisible.value = false;
  await getData();
}

async function handleDelete(id: string) {
  await fetchDeleteConfig(id);
  await onDeleted();
}

async function handleBatchDelete() {
  await Promise.all(checkedRowKeys.value.map(id => fetchDeleteConfig(id)));
  await onBatchDeleted();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">配置管理</span>
          <TableHeaderOperation
            v-model:columns="columnChecks"
            :disabled-delete="checkedRowKeys.length === 0"
            :loading="loading"
            @add="handleAdd"
            @delete="handleBatchDelete"
            @refresh="getData"
          />
        </div>
      </template>
      <TableData
        v-model:checked-row-keys="checkedRowKeys"
        :columns="columns"
        :data="data"
        :loading="loading"
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-full"
      />
    </ElCard>
    <ElDrawer v-model="drawerVisible" size="480px" :title="operateType === 'add' ? '新增配置' : '编辑配置'">
      <ElForm label-position="left" label-width="80">
          <ElFormItem label="编码"><ElInput v-model="formModel.code" /></ElFormItem>
          <ElFormItem label="值"><ElInput v-model="formModel.value" type="textarea" /></ElFormItem>
          <ElFormItem label="备注"><ElInput v-model="formModel.remark" type="textarea" /></ElFormItem>
        </ElForm>
        <template #footer>
          <ElSpace>
            <ElButton @click="drawerVisible = false">{{ $t('common.cancel') }}</ElButton>
            <ElButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
          </ElSpace>
      </template>
    </ElDrawer>
  </div>
</template>

<style scoped></style>
