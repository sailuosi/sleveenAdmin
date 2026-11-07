<script setup lang="tsx">
import { ElButton, ElPopconfirm, ElTag, ElSpace } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import { fetchCreateTenant, fetchDeleteTenant, fetchGetTenantPage, fetchUpdateTenant } from '@/service/api';
import { defaultTransform, useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';

const searchParams = reactive({ page: 1, pageSize: 10, keyword: null as string | null });

const { columns, columnChecks, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemP1.Tenant>>,
  Api.SystemP1.Tenant
>({
  api: () => fetchGetTenantPage(searchParams),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { key: 'name', title: '名称', minWidth: 120 },
    { key: 'code', title: '编码', minWidth: 100 },
    {
      key: 'status',
      title: '状态',
      width: 90,
      render: (row: Api.SystemP1.Tenant) => (
        <ElTag type={row.status === 1 ? 'success' : 'warning'}>{row.status === 1 ? '启用' : '禁用'}</ElTag>
      )
    },
    { key: 'remark', title: '备注', minWidth: 160 },
    {
      key: 'operate',
      title: $t('common.operate'),
      width: 160,
      render: (row: Api.SystemP1.Tenant) => (
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

const tenantStatusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 }
];

const formModel = reactive<Api.SystemP1.TenantInput>({ name: '', code: '', status: 1, remark: '' });

watch(editingData, val => {
  if (val && operateType.value === 'edit') {
    formModel.id = val.id;
    formModel.name = val.name;
    formModel.code = val.code;
    formModel.status = val.status;
    formModel.remark = val.remark;
  } else if (operateType.value === 'add') {
    formModel.id = null;
    formModel.name = '';
    formModel.code = '';
    formModel.status = 1;
    formModel.remark = '';
  }
});

async function handleSubmit() {
  if (operateType.value === 'add') await fetchCreateTenant(formModel);
  else await fetchUpdateTenant(formModel);
  window.$message?.success($t('common.updateSuccess'));
  drawerVisible.value = false;
  await getData();
}

async function handleDelete(id: string) {
  await fetchDeleteTenant(id);
  await onDeleted();
}

async function handleBatchDelete() {
  await Promise.all(checkedRowKeys.value.map(id => fetchDeleteTenant(id)));
  await onBatchDeleted();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">租户管理</span>
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
        :pagination="mobilePagination"
        :row-key="(row: Api.SystemP1.Tenant) => row.id"
        size="small"
        class="sm:h-full"
      />
    </ElCard>
    <ElDrawer v-model="drawerVisible" size="480px" :title="operateType === 'add' ? '新增租户' : '编辑租户'">
      <ElForm label-position="left" label-width="80">
        <ElFormItem label="名称"><ElInput v-model="formModel.name" /></ElFormItem>
        <ElFormItem label="编码"><ElInput v-model="formModel.code" /></ElFormItem>
        <ElFormItem label="状态">
          <ElSelect v-model="formModel.status" class="w-full">
            <ElOption
              v-for="opt in tenantStatusOptions"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </ElSelect>
        </ElFormItem>
        <ElFormItem label="备注"><ElInput v-model="formModel.remark" type="textarea" /></ElFormItem>
      </ElForm>
      <template #footer>
        <ElSpace class="w-full justify-end">
          <ElButton @click="drawerVisible = false">{{ $t('common.cancel') }}</ElButton>
          <ElButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
        </ElSpace>
      </template>
    </ElDrawer>
  </div>
</template>
