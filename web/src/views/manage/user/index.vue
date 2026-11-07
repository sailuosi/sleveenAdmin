<script setup lang="tsx">
import { ElButton, ElPopconfirm, ElTag, ElSpace } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import {
  fetchCreateUser,
  fetchDeleteUser,
  fetchGetUserPage,
  fetchUpdateUser
} from '@/service/api';
import { defaultTransform, useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { useAuth } from '@/hooks/business/auth';
import { $t } from '@/locales';

const { hasAuth } = useAuth();

const searchParams = reactive({
  page: 1,
  pageSize: 10,
  keyword: null as string | null
});

const { columns, columnChecks, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemManage.User>>,
  Api.SystemManage.User
>({
  api: () => fetchGetUserPage(searchParams),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    searchParams.page = page ?? 1;
    searchParams.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { key: 'id', title: 'ID', width: 0, render: () => null },
    { key: 'index', title: $t('common.index'), width: 64, render: (_, index) => index + 1 },
    { key: 'account', title: '账号', minWidth: 120 },
    { key: 'realName', title: '姓名', minWidth: 120 },
    {
      key: 'status',
      title: '状态',
      width: 90,
      render: row =>
        row.status === 1 ? <ElTag type="success">启用</ElTag> : <ElTag type="warning">禁用</ElTag>
    },
    { key: 'createTime', title: '创建时间', minWidth: 160 },
    {
      key: 'operate',
      title: $t('common.operate'),
      width: 160,
      render: row => (
        <div class="flex gap-8px">
          {hasAuth('manage:user:edit') ? (
            <ElButton size="small" type="primary" plain onClick={() => handleEdit(row.id)}>
              {$t('common.edit')}
            </ElButton>
          ) : null}
          {hasAuth('manage:user:delete') ? (
            <ElPopconfirm title={$t('common.confirmDelete')} onConfirm={() => handleDelete(row.id)}>{{ reference: () => (
                  <ElButton size="small" type="danger" plain>
                    {$t('common.delete')}
                  </ElButton>
                )
              }}
            </ElPopconfirm>
          ) : null}
        </div>
      )
    }
  ]
});

const { drawerVisible, operateType, editingData, handleAdd, handleEdit, checkedRowKeys, onBatchDeleted, onDeleted } =
  useTableOperate(data, 'id', getData);

const formModel = reactive<Api.SystemManage.UserInput>({
  account: '',
  password: '',
  realName: '',
  status: 1,
  orgId: null
});

watch(editingData, val => {
  if (val && operateType.value === 'edit') {
    formModel.id = val.id;
    formModel.account = val.account;
    formModel.password = '';
    formModel.realName = val.realName;
    formModel.status = val.status;
    formModel.orgId = val.orgId;
  } else if (operateType.value === 'add') {
    formModel.id = null;
    formModel.account = '';
    formModel.password = '';
    formModel.realName = '';
    formModel.status = 1;
    formModel.orgId = null;
  }
});

async function handleSubmit() {
  if (operateType.value === 'add') {
    await fetchCreateUser(formModel);
  } else {
    await fetchUpdateUser(formModel);
  }
  window.$message?.success($t('common.updateSuccess'));
  drawerVisible.value = false;
  await getData();
}

async function handleDelete(id: string) {
  await fetchDeleteUser(id);
  await onDeleted();
}

async function handleBatchDelete() {
  await Promise.all(checkedRowKeys.value.map(id => fetchDeleteUser(id)));
  await onBatchDeleted();
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
          <span class="font-medium">用户管理</span>
          <TableHeaderOperation
            v-model:columns="columnChecks"
            :disabled-delete="checkedRowKeys.length === 0"
            :loading="loading"
            @refresh="getData"
          >
            <template v-if="hasAuth('manage:user:add') || hasAuth('manage:user:delete')" #default>
              <ElButton v-if="hasAuth('manage:user:add')" size="small" plain type="primary" @click="handleAdd">
                {{ $t('common.add') }}
              </ElButton>
              <ElPopconfirm v-if="hasAuth('manage:user:delete')" :title="$t('common.confirmDelete')" @confirm="handleBatchDelete">
                <template #reference>
                  <ElButton size="small" plain type="danger" :disabled="checkedRowKeys.length === 0">
                    {{ $t('common.batchDelete') }}
                  </ElButton>
                </template>
              </ElPopconfirm>
            </template>
          </TableHeaderOperation>
        </div>
      </template>
      <div class="mb-12px flex gap-12px">
        <ElInput v-model="searchParams.keyword" placeholder="账号/姓名" clearable class="w-240px" />
        <ElButton type="primary" @click="handleSearch">搜索</ElButton>
      </div>
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
    <ElDrawer v-model="drawerVisible" size="480px" :title="operateType === 'add' ? '新增用户' : '编辑用户'">
      <ElForm label-position="left" label-width="80">
          <ElFormItem label="账号">
            <ElInput v-model="formModel.account" />
          </ElFormItem>
          <ElFormItem :label="operateType === 'add' ? '密码' : '新密码'">
            <ElInput v-model="formModel.password" type="password" :placeholder="operateType === 'edit' ? '留空不修改' : ''" />
          </ElFormItem>
          <ElFormItem label="姓名">
            <ElInput v-model="formModel.realName" />
          </ElFormItem>
          <ElFormItem label="状态">
            <ElRadioGroup v-model="formModel.status">
              <ElRadio :label="1">启用</ElRadio>
              <ElRadio :label="2">禁用</ElRadio>
            </ElRadioGroup>
          </ElFormItem>
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
