<script setup lang="tsx">
import { ElButton, ElPopconfirm } from 'element-plus';
import type { FlatResponseData } from '@sa/axios';
import {
  fetchCreateDictData,
  fetchCreateDictType,
  fetchDeleteDictData,
  fetchDeleteDictType,
  fetchGetDictDataPage,
  fetchGetDictTypePage,
  fetchUpdateDictData,
  fetchUpdateDictType
} from '@/service/api';
import { defaultTransform, useNaivePaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';

const typeSearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });
const selectedTypeId = ref<string | null>(null);

const {
  columns: typeColumns,
  data: typeData,
  loading: typeLoading,
  getData: getTypeData,
  mobilePagination: typePagination
} = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemManage.DictType>>,
  Api.SystemManage.DictType
>({
  api: () => fetchGetDictTypePage(typeSearch),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    typeSearch.page = page ?? 1;
    typeSearch.pageSize = pageSize ?? 10;
  },
  columns: () => [
    {
      key: 'name',
      title: '字典类型',
      minWidth: 140,
      render: (row: Api.SystemManage.DictType) => (
        <ElButton link type={selectedTypeId.value === row.id ? 'primary' : 'default'} onClick={() => selectType(row.id)}>
          {row.name} ({row.code})
        </ElButton>
      )
    },
    {
      key: 'operate',
      title: '操作',
      width: 140,
      render: (row: Api.SystemManage.DictType) => (
        <ElPopconfirm title={$t('common.confirmDelete')} onConfirm={() => handleDeleteType(row.id)}>{{ reference: () => (
              <ElButton size="small" type="danger" plain>
                {$t('common.delete')}
              </ElButton>
            )
          }}
        </ElPopconfirm>
      )
    }
  ]
});

getTypeData();

function selectType(id: string) {
  selectedTypeId.value = id;
  dataSearch.page = 1;
  getData();
}

const dataSearch = reactive({ page: 1, pageSize: 10, keyword: null as string | null });

const { columns, data, loading, getData, mobilePagination } = useNaivePaginatedTable<
  FlatResponseData<any, Api.Common.PaginatingQueryRecord<Api.SystemManage.DictData>>,
  Api.SystemManage.DictData
>({
  api: () =>
    selectedTypeId.value
      ? fetchGetDictDataPage(selectedTypeId.value, dataSearch)
      : Promise.resolve({ data: { records: [], current: 1, size: 10, total: 0 }, error: null } as any),
  transform: defaultTransform,
  onPaginationParamsChange: ({ page, pageSize }) => {
    dataSearch.page = page ?? 1;
    dataSearch.pageSize = pageSize ?? 10;
  },
  columns: () => [
    { key: 'label', title: '标签', minWidth: 120 },
    { key: 'value', title: '值', minWidth: 120 },
    { key: 'orderNo', title: '排序', width: 80 },
    {
      key: 'operate',
      title: $t('common.operate'),
      width: 160,
      render: (row: Api.SystemManage.DictData) => (
        <div class="flex gap-8px">
          <ElButton size="small" type="primary" plain onClick={() => handleEditData(row)}>
            {$t('common.edit')}
          </ElButton>
          <ElPopconfirm title={$t('common.confirmDelete')} onConfirm={() => handleDeleteData(row.id)}>{{ reference: () => (
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

const typeDrawer = ref(false);
const typeForm = reactive<Api.SystemManage.DictTypeInput>({ code: '', name: '', remark: '' });

function openAddType() {
  typeForm.id = null;
  typeForm.code = '';
  typeForm.name = '';
  typeForm.remark = '';
  typeDrawer.value = true;
}

async function submitType() {
  await fetchCreateDictType(typeForm);
  typeDrawer.value = false;
  await getTypeData();
}

async function handleDeleteType(id: string) {
  await fetchDeleteDictType(id);
  if (selectedTypeId.value === id) selectedTypeId.value = null;
  await getTypeData();
}

const dataDrawer = ref(false);
const dataOperate = ref<'add' | 'edit'>('add');
const dataForm = reactive<Api.SystemManage.DictDataInput>({
  typeId: '',
  label: '',
  value: '',
  orderNo: 1
});

function openAddData() {
  if (!selectedTypeId.value) {
    window.$message?.warning('请先选择字典类型');
    return;
  }
  dataOperate.value = 'add';
  dataForm.id = null;
  dataForm.typeId = selectedTypeId.value;
  dataForm.label = '';
  dataForm.value = '';
  dataForm.orderNo = 1;
  dataDrawer.value = true;
}

function handleEditData(row: Api.SystemManage.DictData) {
  dataOperate.value = 'edit';
  Object.assign(dataForm, row);
  dataDrawer.value = true;
}

async function submitData() {
  if (dataOperate.value === 'add') await fetchCreateDictData(dataForm);
  else await fetchUpdateDictData(dataForm);
  dataDrawer.value = false;
  await getData();
}

async function handleDeleteData(id: string) {
  await fetchDeleteDictData(id);
  await getData();
}
</script>

<template>
  <div class="min-h-500px flex gap-16px lt-sm:flex-col">
    <ElCard class="w-360px card-wrapper" size="small" shadow="never">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">字典类型</span>
          <ElButton size="small" type="primary" plain @click="openAddType">新增类型</ElButton>
        </div>
      </template>
      <TableData
        :columns="typeColumns"
        :data="typeData"
        :loading="typeLoading"
        :pagination="typePagination"
        :row-key="row => row.id"
        size="small"
      />
    </ElCard>
    <ElCard class="flex-1 card-wrapper" size="small" shadow="never">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">字典数据</span>
          <ElButton size="small" type="primary" plain :disabled="!selectedTypeId" @click="openAddData">新增数据</ElButton>
        </div>
      </template>
      <TableData
        :columns="columns"
        :data="data"
        :loading="loading"
        :pagination="mobilePagination"
        :row-key="row => row.id"
      />
    </ElCard>
    <ElDrawer v-model="typeDrawer" size="420px" title="新增字典类型">
      <ElForm label-width="80" label-position="left">
          <ElFormItem label="编码"><ElInput v-model="typeForm.code" /></ElFormItem>
          <ElFormItem label="名称"><ElInput v-model="typeForm.name" /></ElFormItem>
          <ElFormItem label="备注"><ElInput v-model="typeForm.remark" type="textarea" /></ElFormItem>
        </ElForm>
        <template #footer>
          <ElButton type="primary" @click="submitType">{{ $t('common.confirm') }}</ElButton>
      </template>
    </ElDrawer>
    <ElDrawer v-model="dataDrawer" size="420px" :title="dataOperate === 'add' ? '新增字典数据' : '编辑字典数据'">
      <ElForm label-width="80" label-position="left">
          <ElFormItem label="标签"><ElInput v-model="dataForm.label" /></ElFormItem>
          <ElFormItem label="值"><ElInput v-model="dataForm.value" /></ElFormItem>
          <ElFormItem label="排序"><ElInputNumber v-model="dataForm.orderNo" class="w-full" /></ElFormItem>
        </ElForm>
        <template #footer>
          <ElButton type="primary" @click="submitData">{{ $t('common.confirm') }}</ElButton>
      </template>
    </ElDrawer>
  </div>
</template>

<style scoped></style>
