<script setup lang="tsx">
import { computed, reactive, ref, watch } from 'vue';
import {
  ElButton,
  ElCascader,
  ElCol,
  ElForm,
  ElFormItem,
  ElInput,
  ElInputNumber,
  ElOption,
  ElPopconfirm,
  ElRadio,
  ElRadioGroup,
  ElRow,
  ElSelect,
  ElSpace,
  ElSwitch
} from 'element-plus';
import { fetchCreateMenu, fetchDeleteMenu, fetchGetMenuTree, fetchUpdateMenu } from '@/service/api';
import { useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';

defineOptions({ name: 'ManageMenu' });

const treeData = ref<Api.SystemManage.Menu[]>([]);
const loading = ref(false);

async function getData() {
  loading.value = true;
  const { data, error } = await fetchGetMenuTree();
  loading.value = false;
  if (!error) treeData.value = data ?? [];
}

getData();

const columns = [
  { key: 'title', title: '标题', minWidth: 180 },
  { key: 'name', title: '路由名', minWidth: 120 },
  { key: 'path', title: '路径', minWidth: 160 },
  {
    key: 'type',
    title: '类型',
    width: 90,
    render: (row: Api.SystemManage.Menu) => {
      const map: Record<number, string> = { 1: '目录', 2: '菜单', 3: '按钮' };
      return map[row.type] ?? row.type;
    }
  },
  { key: 'orderNo', title: '排序', width: 80 },
  {
    key: 'operate',
    title: $t('common.operate'),
    width: 160,
    render: (row: Api.SystemManage.Menu) => (
      <div class="flex gap-8px">
        <ElButton size="small" type="primary" plain onClick={() => handleEdit(row.id)}>
          {$t('common.edit')}
        </ElButton>
        <ElPopconfirm title={$t('common.confirmDelete')} onConfirm={() => handleDelete(row.id)}>
          {{
            reference: () => (
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

/** 仅供编辑查找用的扁平索引（不含 children，避免误用） */
const flatLookup = ref<Api.SystemManage.Menu[]>([]);

function flattenLookup(list: Api.SystemManage.Menu[]): Api.SystemManage.Menu[] {
  return list.flatMap(item => {
    const { children, ...rest } = item;
    return [rest as Api.SystemManage.Menu, ...flattenLookup(children ?? [])];
  });
}

watch(
  treeData,
  val => {
    flatLookup.value = flattenLookup(val);
  },
  { immediate: true }
);

const {
  drawerVisible,
  operateType,
  editingData,
  handleAdd: openAddDrawer,
  handleEdit,
  onDeleted
} = useTableOperate(flatLookup, 'id', getData);

function emptyForm(): Api.SystemManage.MenuInput {
  return {
    id: null,
    pid: null,
    name: '',
    title: '',
    path: '',
    component: '',
    icon: '',
    type: 2,
    orderNo: 100,
    permission: '',
    isHide: false,
    menuCode: '',
    menuType: '1',
    redirect: '',
    outLink: '',
    isIframe: false,
    isKeepAlive: true,
    isAffix: false,
    status: 1,
    remark: '',
    extPara: '',
    menuTheme: '',
    isApp: false
  };
}

const formModel = reactive<Api.SystemManage.MenuInput>(emptyForm());

function resetForm() {
  Object.assign(formModel, emptyForm());
}

function fillFormFromRow(val: Api.SystemManage.Menu) {
  Object.assign(formModel, emptyForm(), {
    id: val.id,
    pid: val.pid,
    name: val.name,
    title: String(val.title || ''),
    path: val.path,
    component: val.component,
    icon: val.icon,
    type: val.type,
    orderNo: val.orderNo,
    permission: val.permission,
    isHide: val.isHide,
    menuCode: val.menuCode ?? '',
    menuType: val.menuType ?? '1',
    redirect: val.redirect ?? '',
    outLink: val.outLink ?? '',
    isIframe: val.isIframe ?? false,
    isKeepAlive: val.isKeepAlive ?? true,
    isAffix: val.isAffix ?? false,
    status: val.status ?? 1,
    remark: val.remark ?? '',
    extPara: val.extPara ?? '',
    menuTheme: val.menuTheme ?? '',
    isApp: val.isApp ?? false
  });
}

/** 每次点新增都清空（仅依赖 editingData 时第二次 add 不会触发 watch） */
function handleAdd() {
  resetForm();
  openAddDrawer();
}

watch(editingData, val => {
  if (operateType.value === 'edit' && val) {
    fillFormFromRow(val as Api.SystemManage.Menu);
  }
});

watch(drawerVisible, open => {
  if (open && operateType.value === 'add') {
    resetForm();
  }
});

/** 级联选项：根节点「顶级」+ 菜单树（camelCase children） */
type CascaderNode = {
  value: string | null;
  label: string;
  children?: CascaderNode[];
};

const pidCascaderOptions = computed<CascaderNode[]>(() => {
  const mapNode = (m: Api.SystemManage.Menu): CascaderNode => ({
    value: m.id,
    label: m.title,
    children: (m.children ?? []).map(mapNode)
  });
  return [{ value: null, label: '顶级（无上级）' }, ...treeData.value.map(mapNode)];
});

const cascaderProps = {
  checkStrictly: true,
  emitPath: false,
  value: 'value',
  label: 'label',
  children: 'children'
};

const showRouteFields = computed(() => formModel.type === 1 || formModel.type === 2);

const menuTypeOptions = [
  { label: '目录', value: 1 },
  { label: '菜单', value: 2 },
  { label: '按钮', value: 3 }
];

const menuTypeDict = [
  { label: 'Vue页面', value: '1' },
  { label: '内嵌页面', value: '2' },
  { label: '网页链接', value: '3' },
  { label: '无界Vue组件', value: '4' }
];

function normalizeComponentPath(raw: string) {
  let e = raw || '';
  if (e.startsWith('pcweb\\') || e.startsWith('pcweb/')) e = e.replace(/^pcweb[/\\]/, '');
  if (e.startsWith('web\\') || e.startsWith('web/')) e = e.replace(/^web[/\\]/, '');
  if (e.startsWith('src\\') || e.startsWith('src/') || e.startsWith('widgets')) {
    const widgetName = e.replaceAll('\\', '/');
    formModel.component = widgetName;
    if (!formModel.name) {
      const lastSlashIndex = widgetName.lastIndexOf('/');
      const filename = lastSlashIndex >= 0 ? widgetName.substring(lastSlashIndex + 1) : widgetName;
      const vname = filename.replace(/\.vue$/i, '');
      formModel.name = vname;
      formModel.menuCode = vname;
      formModel.path = `/${vname.replaceAll('_', '/')}`;
    }
  }
}

async function handleSubmit() {
  if (!formModel.name?.trim() || !formModel.title?.trim()) {
    window.$message?.warning('路由名和标题不能为空');
    return;
  }
  if (operateType.value === 'add') await fetchCreateMenu({ ...formModel });
  else await fetchUpdateMenu({ ...formModel });
  window.$message?.success($t('common.updateSuccess'));
  drawerVisible.value = false;
  await getData();
}

async function handleDelete(id: string) {
  await fetchDeleteMenu(id);
  await onDeleted();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">菜单管理</span>
          <ElSpace>
            <ElButton type="primary" plain size="small" @click="handleAdd">新增</ElButton>
            <ElButton size="small" @click="getData">{{ $t('common.refresh') }}</ElButton>
          </ElSpace>
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

    <ElDrawer
      v-model="drawerVisible"
      size="640px"
      :title="operateType === 'add' ? '新增菜单' : '编辑菜单'"
    >
      <ElForm label-position="top" class="menu-edit-form">
        <ElFormItem label="上级菜单">
          <ElCascader
            v-model="formModel.pid"
            class="w-full"
            clearable
            filterable
            placeholder="请选择上级菜单"
            :options="pidCascaderOptions"
            :props="cascaderProps"
          />
        </ElFormItem>

        <ElFormItem label="菜单类型（目录/菜单/按钮）" required>
          <ElRadioGroup v-model="formModel.type">
            <ElRadio v-for="opt in menuTypeOptions" :key="opt.value" :value="opt.value" :label="opt.value">
              {{ opt.label }}
            </ElRadio>
          </ElRadioGroup>
        </ElFormItem>

        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem label="菜单名称（标题）" required>
              <ElInput v-model="formModel.title" placeholder="显示名称，如：流程定义" clearable />
            </ElFormItem>
          </ElCol>
          <ElCol v-if="showRouteFields" :span="12">
            <ElFormItem label="路由名称" required>
              <ElInput v-model="formModel.name" placeholder="如：sail_processes" clearable />
            </ElFormItem>
          </ElCol>
          <ElCol v-else :span="12">
            <ElFormItem label="路由名称" required>
              <ElInput v-model="formModel.name" placeholder="按钮可填权限短名" clearable />
            </ElFormItem>
          </ElCol>
        </ElRow>

        <template v-if="showRouteFields">
          <ElRow :gutter="16">
            <ElCol :span="12">
              <ElFormItem label="路由路径" required>
                <ElInput v-model="formModel.path" placeholder="如：/sail/processes" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :span="12">
              <ElFormItem label="菜单编码">
                <ElInput v-model="formModel.menuCode" placeholder="业务编码，可与路由名相同" clearable />
              </ElFormItem>
            </ElCol>
          </ElRow>

          <ElFormItem label="组件路径">
            <ElInput
              v-model="formModel.component"
              type="textarea"
              :rows="2"
              placeholder="目录填 layout.base；菜单填 widgets/.../Xxx.vue"
              clearable
              @change="normalizeComponentPath"
            />
          </ElFormItem>

          <ElFormItem label="菜单图标">
            <ElInput v-model="formModel.icon" placeholder="如：mdi:sitemap / ele-Setting" clearable />
          </ElFormItem>

          <ElRow :gutter="16">
            <ElCol :span="12">
              <ElFormItem label="菜单排序">
                <ElInputNumber v-model="formModel.orderNo" class="w-full" :min="0" />
              </ElFormItem>
            </ElCol>
            <ElCol :span="12">
              <ElFormItem label="页面形态">
                <ElSelect v-model="formModel.menuType" class="w-full" clearable placeholder="Vue / 内嵌 / 外链 / 无界">
                  <ElOption v-for="opt in menuTypeDict" :key="opt.value" :label="opt.label" :value="opt.value" />
                </ElSelect>
              </ElFormItem>
            </ElCol>
          </ElRow>

          <ElFormItem label="链接地址（外链/内嵌）">
            <ElInput v-model="formModel.outLink" type="textarea" :rows="2" placeholder="外链或内嵌 URL" clearable />
          </ElFormItem>

          <ElRow :gutter="16">
            <ElCol :span="12">
              <ElFormItem label="是否隐藏">
                <ElRadioGroup v-model="formModel.isHide">
                  <ElRadio :value="true" :label="true">隐藏</ElRadio>
                  <ElRadio :value="false" :label="false">不隐藏</ElRadio>
                </ElRadioGroup>
              </ElFormItem>
            </ElCol>
            <ElCol :span="12">
              <ElFormItem label="是否缓存">
                <ElRadioGroup v-model="formModel.isKeepAlive">
                  <ElRadio :value="true" :label="true">缓存</ElRadio>
                  <ElRadio :value="false" :label="false">不缓存</ElRadio>
                </ElRadioGroup>
              </ElFormItem>
            </ElCol>
            <ElCol :span="12">
              <ElFormItem label="是否固定">
                <ElRadioGroup v-model="formModel.isAffix">
                  <ElRadio :value="true" :label="true">固定</ElRadio>
                  <ElRadio :value="false" :label="false">不固定</ElRadio>
                </ElRadioGroup>
              </ElFormItem>
            </ElCol>
            <ElCol :span="12">
              <ElFormItem label="是否内嵌">
                <ElRadioGroup v-model="formModel.isIframe">
                  <ElRadio :value="true" :label="true">内嵌</ElRadio>
                  <ElRadio :value="false" :label="false">不内嵌</ElRadio>
                </ElRadioGroup>
              </ElFormItem>
            </ElCol>
          </ElRow>
        </template>

        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem label="是否启用">
              <ElRadioGroup v-model="formModel.status">
                <ElRadio :value="1" :label="1">启用</ElRadio>
                <ElRadio :value="2" :label="2">停用</ElRadio>
              </ElRadioGroup>
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem label="是否 APP 端">
              <ElRadioGroup v-model="formModel.isApp">
                <ElRadio :value="true" :label="true">是</ElRadio>
                <ElRadio :value="false" :label="false">否</ElRadio>
              </ElRadioGroup>
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem label="权限标识">
              <ElInput v-model="formModel.permission" placeholder="如：sysMenu:add" clearable />
            </ElFormItem>
          </ElCol>
          <ElCol v-if="!showRouteFields" :span="12">
            <ElFormItem label="菜单排序">
              <ElInputNumber v-model="formModel.orderNo" class="w-full" :min="0" />
            </ElFormItem>
          </ElCol>
        </ElRow>

        <ElFormItem label="重定向">
          <ElInput v-model="formModel.redirect" placeholder="重定向地址" clearable />
        </ElFormItem>
        <ElFormItem label="主题">
          <ElInput v-model="formModel.menuTheme" placeholder="菜单主题" clearable />
        </ElFormItem>
        <ElFormItem label="额外参数">
          <ElInput v-model="formModel.extPara" placeholder="ExtPara" clearable />
        </ElFormItem>
        <ElFormItem label="备注">
          <ElInput v-model="formModel.remark" type="textarea" :rows="2" placeholder="备注" clearable />
        </ElFormItem>

        <ElFormItem v-if="!showRouteFields" label="隐藏">
          <ElSwitch v-model="formModel.isHide" />
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

<style scoped>
.menu-edit-form :deep(.el-form-item) {
  margin-bottom: 14px;
}
.w-full {
  width: 100%;
}
</style>
