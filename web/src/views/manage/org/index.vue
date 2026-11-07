<script setup lang="tsx">
import { ElButton, ElPopconfirm, ElSpace } from 'element-plus';
import { fetchCreateOrg, fetchDeleteOrg, fetchGetOrgTree, fetchUpdateOrg } from '@/service/api';
import { useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';

const treeData = ref<Api.SystemManage.Org[]>([]);
const loading = ref(false);
const flatLookup = ref<Api.SystemManage.Org[]>([]);

async function getData() {
  loading.value = true;
  const { data, error } = await fetchGetOrgTree();
  loading.value = false;
  if (!error) treeData.value = data ?? [];
}

getData();

function flattenLookup(list: Api.SystemManage.Org[]): Api.SystemManage.Org[] {
  return list.flatMap(item => {
    const { children, ...rest } = item;
    return [rest as Api.SystemManage.Org, ...flattenLookup(children ?? [])];
  });
}

watch(
  treeData,
  val => {
    flatLookup.value = flattenLookup(val);
  },
  { immediate: true }
);

const columns = [
  { key: 'name', title: '组织名称', minWidth: 180 },
  { key: 'code', title: '编码', minWidth: 120 },
  { key: 'orderNo', title: '排序', width: 80 },
  {
    key: 'operate',
    title: $t('common.operate'),
    width: 160,
    render: (row: Api.SystemManage.Org) => (
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
];

const { drawerVisible, operateType, editingData, handleAdd, handleEdit, onDeleted } = useTableOperate(
  flatLookup,
  'id',
  getData
);

const formModel = reactive<Api.SystemManage.OrgInput>({
  pid: null,
  name: '',
  code: '',
  orderNo: 1
});

watch(editingData, val => {
  if (val && operateType.value === 'edit') {
    formModel.id = val.id;
    formModel.pid = val.pid;
    formModel.name = val.name;
    formModel.code = val.code;
    formModel.orderNo = val.orderNo;
  } else if (operateType.value === 'add') {
    formModel.id = null;
    formModel.pid = null;
    formModel.name = '';
    formModel.code = '';
    formModel.orderNo = 1;
  }
});

async function handleSubmit() {
  if (operateType.value === 'add') await fetchCreateOrg(formModel);
  else await fetchUpdateOrg(formModel);
  window.$message?.success($t('common.updateSuccess'));
  drawerVisible.value = false;
  await getData();
}

async function handleDelete(id: string) {
  await fetchDeleteOrg(id);
  await onDeleted();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">组织管理</span>
          <div class="flex flex-wrap items-center justify-end gap-12px">
        <ElSpace>
          <ElButton type="primary" plain size="small" @click="handleAdd">新增</ElButton>
          <ElButton size="small" @click="getData">{{ $t('common.refresh') }}</ElButton>
        </ElSpace>
      </div>
        </div>
      </template>
      <TableData
        :columns="columns"
        :data="treeData"
        :loading="loading"
        :row-key="row => row.id"
        tree
        default-expand-all
      />
    </ElCard>
    <ElDrawer v-model="drawerVisible" size="480px" :title="operateType === 'add' ? '新增组织' : '编辑组织'">
      <ElForm label-position="left" label-width="80">
          <ElFormItem label="名称"><ElInput v-model="formModel.name" /></ElFormItem>
          <ElFormItem label="编码"><ElInput v-model="formModel.code" /></ElFormItem>
          <ElFormItem label="排序"><ElInputNumber v-model="formModel.orderNo" class="w-full" /></ElFormItem>
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
