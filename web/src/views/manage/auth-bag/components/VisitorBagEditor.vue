<script setup lang="ts">
type OrgTreeNode = { id: string; label: string; children?: OrgTreeNode[] };
import { ElMessage } from 'element-plus';
import {
  fetchAuthVisitorBagSelect,
  fetchAuthVisitorItemPage,
  fetchBatchAuthVisitorItems,
  fetchDeleteAuthVisitorItem,
  fetchGetOrgTree,
  fetchGetUserPage,
  fetchSaveAuthVisitorBag,
  type AuthBagSelectItem,
  type AuthVisitorBag,
  type AuthVisitorItem
} from '@/service/api';

const props = defineProps<{
  model: AuthVisitorBag | null;
}>();

const emit = defineEmits<{
  saved: [];
  changed: [];
  cancel: [];
}>();

const message = ElMessage;
const saving = ref(false);
const form = reactive<AuthVisitorBag>({
  name: '',
  code: '',
  note: '',
  remark: '',
  type: '',
  keyWord: '',
  idx: 0,
  parentId: null
});

const bagId = computed(() => form.id);
const parentOptions = ref<{ label: string; value: string }[]>([]);
const members1 = ref<AuthVisitorItem[]>([]);
const members2 = ref<AuthVisitorItem[]>([]);
const members3 = ref<AuthVisitorItem[]>([]);

const userModal = ref(false);
const orgModal = ref(false);
const orgBindType = ref<'2' | '3'>('2');
const userKeyword = ref('');
const userOptions = ref<{ label: string; value: string; account: string; name: string }[]>([]);
const selectedUserIds = ref<string[]>([]);
const orgTree = ref<OrgTreeNode[]>([]);
const orgMap = ref<Map<string, Api.SystemManage.Org>>(new Map());
const selectedOrgKeys = ref<string[]>([]);

function resetFromModel() {
  Object.assign(
    form,
    {
      id: undefined,
      name: '',
      code: '',
      note: '',
      remark: '',
      type: '',
      keyWord: '',
      idx: 0,
      parentId: null
    },
    props.model ?? {}
  );
}

async function loadParents() {
  const { data, error } = await fetchAuthVisitorBagSelect();
  if (error || !data) return;
  parentOptions.value = data
    .filter((x: AuthBagSelectItem) => x.id !== form.id)
    .map(x => ({
      label: x.code ? `${x.name ?? ''} (${x.code})` : (x.name ?? x.id),
      value: x.id
    }));
}

async function loadMembers() {
  if (!bagId.value) {
    members1.value = [];
    members2.value = [];
    members3.value = [];
    return;
  }
  const load = async (type: string) => {
    const { data, error } = await fetchAuthVisitorItemPage({
      page: 1,
      pageSize: 500,
      bagId: bagId.value!,
      type
    });
    return error ? [] : (data?.records ?? []);
  };
  members1.value = await load('1');
  members2.value = await load('2');
  members3.value = await load('3');
}

function flattenOrgs(list: Api.SystemManage.Org[], map: Map<string, Api.SystemManage.Org>): OrgTreeNode[] {
  return list.map(o => {
    map.set(o.id, o);
    return {
      id: o.id,
      label: o.code ? `${o.name} (${o.code})` : o.name,
      children: o.children?.length ? flattenOrgs(o.children, map) : undefined
    };
  });
}

async function loadOrgTree() {
  const { data, error } = await fetchGetOrgTree();
  if (error || !data) return;
  const map = new Map<string, Api.SystemManage.Org>();
  orgTree.value = flattenOrgs(data, map);
  orgMap.value = map;
}

async function searchUsers() {
  const { data, error } = await fetchGetUserPage({
    page: 1,
    pageSize: 50,
    keyword: userKeyword.value || null
  });
  if (error || !data) return;
  userOptions.value = (data.records ?? []).map(u => ({
    label: `${u.realName || u.account} (${u.account})`,
    value: u.id,
    account: u.account,
    name: u.realName || u.account
  }));
}

watch(() => props.model, async () => {
  resetFromModel();
  await loadParents();
  await loadMembers();
}, { immediate: true });

onMounted(() => {
  loadOrgTree();
});

async function saveBag() {
  if (!form.name?.trim()) {
    ElMessage.warning('请填写名称');
    return;
  }
  saving.value = true;
  try {
    const { data, error } = await fetchSaveAuthVisitorBag(form);
    if (error) return;
    form.id = data?.id ?? form.id;
    ElMessage.success('集合已保存');
    await loadParents();
    await loadMembers();
    emit('changed');
  } finally {
    saving.value = false;
  }
}

async function removeMember(item: AuthVisitorItem) {
  if (!item.id) return;
  const { error } = await fetchDeleteAuthVisitorItem(item.id);
  if (error) return;
  ElMessage.success('已移除');
  await loadMembers();
}

async function openUserPicker() {
  if (!bagId.value) {
    ElMessage.warning('请先保存访客集合');
    return;
  }
  selectedUserIds.value = [];
  await searchUsers();
  userModal.value = true;
}

function openOrgPicker(type: '2' | '3') {
  if (!bagId.value) {
    ElMessage.warning('请先保存访客集合');
    return;
  }
  orgBindType.value = type;
  selectedOrgKeys.value = [];
  orgModal.value = true;
}

async function confirmUsers() {
  const picked = userOptions.value.filter(u => selectedUserIds.value.includes(u.value));
  if (!picked.length || !bagId.value) return;
  const { error } = await fetchBatchAuthVisitorItems({
    visitorBagId: bagId.value,
    items: picked.map(u => ({
      type: '1',
      name: u.name,
      code: u.account,
      oid: u.value,
      isOn: true,
      idx: 0
    }))
  });
  if (error) return;
  userModal.value = false;
  ElMessage.success('人员已添加');
  await loadMembers();
}

async function confirmOrgs() {
  if (!selectedOrgKeys.value.length || !bagId.value) return;
  const type = orgBindType.value;
  const items = selectedOrgKeys.value
    .map(id => orgMap.value.get(id))
    .filter(Boolean)
    .map(org => {
      const o = org!;
      const code = type === '3' ? (o.classCode || o.code || '') : (o.code || '');
      return {
        type,
        name: o.name,
        code,
        oid: o.id,
        isOn: true,
        idx: 0
      };
    });
  const { error } = await fetchBatchAuthVisitorItems({
    visitorBagId: bagId.value,
    items
  });
  if (error) return;
  orgModal.value = false;
  ElMessage.success('组织已添加');
  await loadMembers();
}
</script>

<template>
  <div>
    <ElForm label-position="left" label-width="100">
      <ElFormItem label="名称" required>
        <ElInput v-model="form.name" />
      </ElFormItem>
      <ElFormItem label="编号">
        <ElInput v-model="form.code" />
      </ElFormItem>
      <ElFormItem label="描述">
        <ElInput v-model="form.note" type="textarea" :rows="2" />
      </ElFormItem>
      <ElFormItem label="备注">
        <ElInput v-model="form.remark" />
      </ElFormItem>
      <ElFormItem label="类型">
        <ElInput v-model="form.type" />
      </ElFormItem>
      <ElFormItem label="关键成员">
        <ElInput v-model="form.keyWord" />
      </ElFormItem>
      <ElFormItem label="排序">
        <ElInputNumber v-model="form.idx" class="w-full" />
      </ElFormItem>
      <ElFormItem label="父级集合">
        <ElSelect v-model="form.parentId"  clearable filterable><ElOption v-for="opt in parentOptions" :key="opt.value" :label="opt.label" :value="opt.value" /></ElSelect>
      </ElFormItem>
      <ElSpace class="mb-12px w-full justify-end">
        <ElButton @click="emit('cancel')">关闭</ElButton>
        <ElButton type="primary" :loading="saving" @click="saveBag">保存集合</ElButton>
      </ElSpace>
    </ElForm>

    <div class="mb-16px">
      <div class="mb-8px flex items-center justify-between">
        <span class="font-medium">包含人员</span>
        <ElButton size="small" type="primary" :disabled="!bagId" @click="openUserPicker">添加人员</ElButton>
      </div>
      <ElSpace>
        <ElTag v-for="m in members1" :key="m.id" closable @close="removeMember(m)">
          {{ m.name || m.code }}
        </ElTag>
        <span v-if="!members1.length" class="text-gray-400">暂无</span>
      </ElSpace>
    </div>

    <div class="mb-16px">
      <div class="mb-8px flex items-center justify-between">
        <span class="font-medium">包含部门</span>
        <ElButton size="small" type="primary" :disabled="!bagId" @click="openOrgPicker('2')">添加部门</ElButton>
      </div>
      <ElSpace>
        <ElTag v-for="m in members2" :key="m.id" closable type="info" @close="removeMember(m)">
          {{ m.name || m.code }}
        </ElTag>
        <span v-if="!members2.length" class="text-gray-400">暂无</span>
      </ElSpace>
    </div>

    <div class="mb-16px">
      <div class="mb-8px flex items-center justify-between">
        <span class="font-medium">包含部门及下级</span>
        <ElButton size="small" type="primary" :disabled="!bagId" @click="openOrgPicker('3')">添加部门</ElButton>
      </div>
      <ElSpace>
        <ElTag v-for="m in members3" :key="m.id" closable type="warning" @close="removeMember(m)">
          {{ m.name || m.code }}
        </ElTag>
        <span v-if="!members3.length" class="text-gray-400">暂无</span>
      </ElSpace>
    </div>

    <ElDialog v-model="userModal" title="选择人员" width="520px">
      <ElSpace class="mb-12px" vertical>
        <ElSpace>
          <ElInput v-model="userKeyword" placeholder="搜索账号/姓名" clearable @keyup.enter="searchUsers" />
          <ElButton type="primary" @click="searchUsers">查询</ElButton>
        </ElSpace>
        <ElSelect
          v-model="selectedUserIds"
          multiple
          filterable
          
          placeholder="勾选人员"
       ><ElOption v-for="opt in userOptions" :key="opt.value" :label="opt.label" :value="opt.value" /></ElSelect>
      </ElSpace>
      <ElSpace class="w-full justify-end">
        <ElButton @click="userModal = false">取消</ElButton>
        <ElButton type="primary" @click="confirmUsers">确定</ElButton>
      </ElSpace>
    </ElDialog>

    <ElDialog v-model="orgModal" title="选择组织" width="480px">
      <ElTree
        node-key="id"
        show-checkbox
        check-strictly
        :data="orgTree"
        :default-checked-keys="selectedOrgKeys"
        style="max-height: 420px; overflow: auto"
        @check="(_data, ctx) => (selectedOrgKeys.value = ctx.checkedKeys as string[])"
      />
      <ElSpace class="mt-12px w-full justify-end">
        <ElButton @click="orgModal = false">取消</ElButton>
        <ElButton type="primary" @click="confirmOrgs">确定</ElButton>
      </ElSpace>
    </ElDialog>
  </div>
</template>
