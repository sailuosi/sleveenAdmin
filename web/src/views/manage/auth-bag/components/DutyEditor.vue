<script setup lang="ts">
import { ElMessage } from 'element-plus';
import {
  fetchAuthGoodsBagSelect,
  fetchAuthVisitorBagSelect,
  fetchAuthWordBagSelect,
  fetchSaveAuthDuty,
  type AuthBagSelectItem,
  type AuthDutyItem
} from '@/service/api';

const props = defineProps<{
  model: AuthDutyItem | null;
}>();

const emit = defineEmits<{
  saved: [];
  cancel: [];
}>();

const message = ElMessage;
const saving = ref(false);
const form = reactive<AuthDutyItem>({
  name: '',
  code: '',
  note: '',
  remark: '',
  idx: 0,
  visitorBagId: null,
  goodsBagId: null,
  wordBagId: null
});

const visitorOptions = ref<{ label: string; value: string }[]>([]);
const goodsOptions = ref<{ label: string; value: string }[]>([]);
const wordOptions = ref<{ label: string; value: string }[]>([]);

function toOptions(list: AuthBagSelectItem[]) {
  return list.map(x => ({
    label: x.code ? `${x.name ?? ''} (${x.code})` : (x.name ?? x.id),
    value: x.id
  }));
}

async function loadSelects() {
  const [v, g, w] = await Promise.all([
    fetchAuthVisitorBagSelect(),
    fetchAuthGoodsBagSelect(),
    fetchAuthWordBagSelect()
  ]);
  if (!v.error && v.data) visitorOptions.value = toOptions(v.data);
  if (!g.error && g.data) goodsOptions.value = toOptions(g.data);
  if (!w.error && w.data) wordOptions.value = toOptions(w.data);
}

function resetFromModel() {
  Object.assign(form, {
    id: undefined,
    name: '',
    code: '',
    note: '',
    remark: '',
    idx: 0,
    visitorBagId: null,
    goodsBagId: null,
    wordBagId: null
  }, props.model ?? {});
}

watch(() => props.model, resetFromModel, { immediate: true });

onMounted(loadSelects);

async function save() {
  if (!form.name?.trim()) {
    ElMessage.warning('请填写名称');
    return;
  }
  saving.value = true;
  try {
    const { error } = await fetchSaveAuthDuty(form);
    if (error) return;
    ElMessage.success('保存成功');
    emit('saved');
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <ElForm label-position="left" label-width="100">
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
      <ElInput v-model="form.remark" type="textarea" :rows="2" />
    </ElFormItem>
    <ElFormItem label="排序">
      <ElInputNumber v-model="form.idx" class="w-full" />
    </ElFormItem>
    <ElFormItem label="访客集合">
      <ElSelect
        v-model="form.visitorBagId"
        
        clearable
        filterable
        placeholder="选择访客集合"
     ><ElOption v-for="opt in visitorOptions" :key="opt.value" :label="opt.label" :value="opt.value" /></ElSelect>
    </ElFormItem>
    <ElFormItem label="资源集合">
      <ElSelect
        v-model="form.goodsBagId"
        
        clearable
        filterable
        placeholder="选择资源集合"
     ><ElOption v-for="opt in goodsOptions" :key="opt.value" :label="opt.label" :value="opt.value" /></ElSelect>
    </ElFormItem>
    <ElFormItem label="词条集合">
      <ElSelect
        v-model="form.wordBagId"
        
        clearable
        filterable
        placeholder="选择词条集合"
     ><ElOption v-for="opt in wordOptions" :key="opt.value" :label="opt.label" :value="opt.value" /></ElSelect>
    </ElFormItem>
    <ElSpace class="w-full justify-end">
      <ElButton @click="emit('cancel')">取消</ElButton>
      <ElButton type="primary" :loading="saving" @click="save">保存</ElButton>
    </ElSpace>
  </ElForm>
</template>
