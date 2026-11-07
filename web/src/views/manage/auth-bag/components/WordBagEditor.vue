<script setup lang="tsx">
import { ElMessage, ElButton, ElPopconfirm, ElSpace } from 'element-plus';
import {
  fetchAuthWordItemPage,
  fetchDeleteAuthWordItem,
  fetchSaveAuthWordBag,
  fetchSaveAuthWordItem,
  fetchWordDefineSelect,
  type AuthWordBag,
  type AuthWordItem
} from '@/service/api';

const props = defineProps<{
  model: AuthWordBag | null;
}>();

const emit = defineEmits<{
  saved: [];
  changed: [];
  cancel: [];
}>();

const message = ElMessage;
const saving = ref(false);
const form = reactive<AuthWordBag>({
  name: '',
  code: '',
  note: '',
  remark: '',
  type: '',
  idx: 0
});

const bagId = computed(() => form.id);
const items = ref<AuthWordItem[]>([]);
const defineOptions = ref<{ label: string; value: string; name?: string; code?: string; type?: string }[]>([]);
const itemModal = ref(false);
const itemForm = reactive<AuthWordItem>({
  name: '',
  code: '',
  type: '',
  wordDefineId: null,
  idx: 0
});

function resetFromModel() {
  Object.assign(
    form,
    { id: undefined, name: '', code: '', note: '', remark: '', type: '', idx: 0 },
    props.model ?? {}
  );
}

async function loadDefines() {
  const { data, error } = await fetchWordDefineSelect();
  if (error || !data) return;
  defineOptions.value = data.map(d => ({
    label: d.code ? `${d.name ?? ''} (${d.code})` : (d.name ?? d.id),
    value: d.id,
    name: d.name,
    code: d.code,
    type: d.type
  }));
}

async function loadItems() {
  if (!bagId.value) {
    items.value = [];
    return;
  }
  const { data, error } = await fetchAuthWordItemPage({
    page: 1,
    pageSize: 200,
    bagId: bagId.value
  });
  items.value = error ? [] : (data?.records ?? []);
}

watch(
  () => props.model,
  async () => {
    resetFromModel();
    await loadDefines();
    await loadItems();
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
    const { data, error } = await fetchSaveAuthWordBag(form);
    if (error) return;
    form.id = data?.id ?? form.id;
    ElMessage.success('集合已保存');
    emit('changed');
  } finally {
    saving.value = false;
  }
}

function openItem(row?: AuthWordItem) {
  if (!bagId.value) {
    ElMessage.warning('请先保存词条集合');
    return;
  }
  Object.assign(
    itemForm,
    { id: undefined, wordBagId: bagId.value, name: '', code: '', type: '', wordDefineId: null, idx: 0 },
    row ?? {}
  );
  itemForm.wordBagId = bagId.value!;
  itemModal.value = true;
}

function onDefineChange(id: string | null) {
  itemForm.wordDefineId = id;
  const def = defineOptions.value.find(d => d.value === id);
  if (def) {
    if (!itemForm.name) itemForm.name = def.name;
    if (!itemForm.code) itemForm.code = def.code;
    if (!itemForm.type) itemForm.type = def.type;
  }
}

async function saveItem() {
  if (!itemForm.wordBagId) return;
  const { error } = await fetchSaveAuthWordItem(itemForm);
  if (error) return;
  itemModal.value = false;
  ElMessage.success('词条项已保存');
  await loadItems();
}

async function removeItem(id: string) {
  const { error } = await fetchDeleteAuthWordItem(id);
  if (error) return;
  ElMessage.success('已删除');
  await loadItems();
}

const itemColumns = [
  { key: 'name', title: '名称', minWidth: 120 },
  { key: 'code', title: '编号', width: 100 },
  { key: 'type', title: '类型', width: 80 },
  {
    key: 'operate',
    title: '操作',
    width: 140,
    render: (row: AuthWordItem) => (
      <ElSpace>
        <ElButton size="tiny" onClick={() => openItem(row)}>
          编辑
        </ElButton>
        <ElPopconfirm title={'确认删除?'} onConfirm={() => removeItem(row.id!)}>{{ reference: () => (
              <ElButton size="tiny" type="danger">
                删除
              </ElButton>
            )
          }}
        </ElPopconfirm>
      </ElSpace>
    )
  }
];
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
      <ElFormItem label="描述">
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
      <span class="font-medium">词条项</span>
      <ElButton size="small" type="primary" :disabled="!bagId" @click="openItem()">新增词条项</ElButton>
    </div>
    <TableData size="small" :columns="itemColumns" :data="items" />

    <ElDialog v-model="itemModal" title="词条项" width="480px">
      <ElForm label-position="left" label-width="90">
        <ElFormItem label="词条定义">
          <ElSelect
            v-model="itemForm.wordDefineId"
            clearable
            filterable
            class="w-full"
            @change="onDefineChange"
          >
            <ElOption v-for="opt in defineOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </ElSelect>
        </ElFormItem>
        <ElFormItem label="名称">
          <ElInput v-model="itemForm.name" />
        </ElFormItem>
        <ElFormItem label="编号">
          <ElInput v-model="itemForm.code" />
        </ElFormItem>
        <ElFormItem label="类型">
          <ElInput v-model="itemForm.type" />
        </ElFormItem>
        <ElFormItem label="排序">
          <ElInputNumber v-model="itemForm.idx" class="w-full" />
        </ElFormItem>
      </ElForm>
      <ElSpace class="w-full justify-end">
        <ElButton @click="itemModal = false">取消</ElButton>
        <ElButton type="primary" @click="saveItem">保存</ElButton>
      </ElSpace>
    </ElDialog>
  </div>
</template>
