<script setup lang="ts">
import { ElMessage } from 'element-plus';
import { fetchSaveAuthWordDefine, type AuthWordDefine } from '@/service/api';

const props = defineProps<{
  model: AuthWordDefine | null;
}>();

const emit = defineEmits<{
  saved: [];
  cancel: [];
}>();

const message = ElMessage;
const saving = ref(false);
const form = reactive<AuthWordDefine>({
  name: '',
  code: '',
  type: '',
  para: '',
  parser: '',
  note: '',
  idx: 0,
  isOn: true
});

function resetFromModel() {
  Object.assign(
    form,
    {
      id: undefined,
      name: '',
      code: '',
      type: '',
      para: '',
      parser: '',
      note: '',
      idx: 0,
      isOn: true
    },
    props.model ?? {}
  );
}

watch(() => props.model, resetFromModel, { immediate: true });

async function save() {
  if (!form.name?.trim()) {
    ElMessage.warning('请填写名称');
    return;
  }
  saving.value = true;
  try {
    const { error } = await fetchSaveAuthWordDefine(form);
    if (error) return;
    ElMessage.success('保存成功');
    emit('saved');
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <ElForm label-position="left" label-width="80">
    <ElFormItem label="名称" required>
      <ElInput v-model="form.name" />
    </ElFormItem>
    <ElFormItem label="编号">
      <ElInput v-model="form.code" />
    </ElFormItem>
    <ElFormItem label="类型">
      <ElInput v-model="form.type" />
    </ElFormItem>
    <ElFormItem label="参数">
      <ElInput v-model="form.para" type="textarea" :rows="2" />
    </ElFormItem>
    <ElFormItem label="说明">
      <ElInput v-model="form.note" type="textarea" :rows="2" />
    </ElFormItem>
    <ElFormItem label="排序">
      <ElInputNumber v-model="form.idx" class="w-full" />
    </ElFormItem>
    <ElFormItem label="启用">
      <ElSwitch v-model="form.isOn" />
    </ElFormItem>
    <ElFormItem label="Parser">
      <ElInput v-model="form.parser" type="textarea" :rows="10" placeholder="ConditionGroup JSON" />
    </ElFormItem>
    <ElSpace class="w-full justify-end">
      <ElButton @click="emit('cancel')">取消</ElButton>
      <ElButton type="primary" :loading="saving" @click="save">保存</ElButton>
    </ElSpace>
  </ElForm>
</template>
