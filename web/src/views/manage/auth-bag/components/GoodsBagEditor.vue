<script setup lang="ts">
import { ElMessage } from 'element-plus';

type MenuTreeNode = { key: string; label: string; children?: MenuTreeNode[] };
import {
  fetchGetMenuTree,
  fetchLoadMenuBindKeys,
  fetchSaveAuthGoodsBag,
  fetchSaveMenuBindKeys,
  type AuthGoodsBag
} from '@/service/api';

const props = defineProps<{
  model: AuthGoodsBag | null;
}>();

const emit = defineEmits<{
  saved: [];
  changed: [];
  cancel: [];
}>();

const message = ElMessage;
const saving = ref(false);
const binding = ref(false);
const form = reactive<AuthGoodsBag>({
  name: '',
  code: '',
  note: '',
  remark: '',
  type: '',
  idx: 0
});

const bagId = computed(() => form.id);
const menuTree = ref<MenuTreeNode[]>([]);
const checkedKeys = ref<string[]>([]);

function toTree(list: Api.SystemManage.Menu[]): MenuTreeNode[] {
  return list.map(m => ({
    key: m.id,
    label: m.title || m.name,
    children: m.children?.length ? toTree(m.children) : undefined
  }));
}

function resetFromModel() {
  Object.assign(
    form,
    { id: undefined, name: '', code: '', note: '', remark: '', type: '', idx: 0 },
    props.model ?? {}
  );
}

async function loadMenuTree() {
  const { data, error } = await fetchGetMenuTree();
  if (!error && data) menuTree.value = toTree(data);
}

async function loadBinds() {
  if (!bagId.value) {
    checkedKeys.value = [];
    return;
  }
  const { data, error } = await fetchLoadMenuBindKeys(bagId.value, '1');
  if (error) return;
  checkedKeys.value = (data ?? []).map(x => x.oid).filter(Boolean);
}

watch(
  () => props.model,
  async () => {
    resetFromModel();
    await loadMenuTree();
    await loadBinds();
  },
  { immediate: true }
);

async function saveBag() {
  if (!form.name?.trim()) {
    ElMessage.warning('请填写名称');
    return;
  }
  saving.value = true;
  try {
    const { data, error } = await fetchSaveAuthGoodsBag(form);
    if (error) return;
    form.id = data?.id ?? form.id;
    ElMessage.success('集合已保存');
    emit('changed');
  } finally {
    saving.value = false;
  }
}

async function saveBinds() {
  if (!bagId.value) {
    ElMessage.warning('请先保存资源集合');
    return;
  }
  binding.value = true;
  try {
    const { error } = await fetchSaveMenuBindKeys({
      goodsBagId: bagId.value,
      type: '1',
      checkMenus: checkedKeys.value
    });
    if (error) return;
    ElMessage.success('菜单绑定已保存');
    emit('changed');
  } finally {
    binding.value = false;
  }
}
</script>

<template>
  <div>
    <ElForm label-position="left" label-width="80">
      <ElFormItem label="名称" required>
        <ElInput v-model="form.name" />
      </ElFormItem>
      <ElFormItem label="编号">
        <ElInput v-model="form.code" />
      </ElFormItem>
      <ElFormItem label="说明">
        <ElInput v-model="form.note" type="textarea" :rows="2" />
      </ElFormItem>
      <ElFormItem label="备注">
        <ElInput v-model="form.remark" />
      </ElFormItem>
      <ElFormItem label="类型">
        <ElInput v-model="form.type" />
      </ElFormItem>
      <ElFormItem label="排序">
        <ElInputNumber v-model="form.idx" class="w-full" />
      </ElFormItem>
      <ElSpace class="mb-12px w-full justify-end">
        <ElButton @click="emit('cancel')">关闭</ElButton>
        <ElButton type="primary" :loading="saving" @click="saveBag">保存集合</ElButton>
      </ElSpace>
    </ElForm>

    <div class="mb-8px flex items-center justify-between">
      <span class="font-medium">绑定菜单</span>
      <ElButton size="small" type="primary" :disabled="!bagId" :loading="binding" @click="saveBinds">
        保存绑定
      </ElButton>
    </div>
    <ElTree
      node-key="key"
      show-checkbox
      :data="menuTree"
      :default-checked-keys="checkedKeys"
      :disabled="!bagId"
      style="max-height: 420px; overflow: auto"
      @check="(_data, ctx) => (checkedKeys.value = ctx.checkedKeys as string[])"
    />
  </div>
</template>
