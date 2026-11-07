<template>
  <el-form ref="formRef" :model="model" :rules="rules" label-width="120px" class="sf-edit-form">
    <el-form-item label="表单类型" prop="formType">
      <el-radio-group v-model="model.formType">
        <el-radio :value="FORM_TYPE_NORMAL">流程表单</el-radio>
        <el-radio :value="FORM_TYPE_CUSTOM">业务表单</el-radio>
      </el-radio-group>
    </el-form-item>
    <el-form-item v-if="model.formType === FORM_TYPE_NORMAL" label="流程表单" prop="formId">
      <el-select v-model="model.formId" clearable filterable placeholder="请选择表单" style="width: 100%">
        <el-option
          v-for="f in formList"
          :key="String(f.id)"
          :label="String(f.name)"
          :value="String(f.id)"
        />
      </el-select>
      <div class="sf-edit-form__hint">
        表单在「流程表单」维护；发起时会按绑定自动带出
        <el-button link type="primary" @click="goForms">去维护</el-button>
      </div>
    </el-form-item>
    <el-form-item
      v-if="model.formType === FORM_TYPE_CUSTOM"
      label="表单提交路由"
      prop="formCustomCreatePath"
    >
      <el-input v-model="model.formCustomCreatePath" placeholder="例如 /sail/leave" />
    </el-form-item>
    <el-form-item
      v-if="model.formType === FORM_TYPE_CUSTOM"
      label="表单查看地址"
      prop="formCustomViewPath"
    >
      <el-input v-model="model.formCustomViewPath" placeholder="例如 /sail/process-detail" />
    </el-form-item>

    <div
      v-if="model.formType === FORM_TYPE_NORMAL && model.formId && preview.rule.length"
      class="sf-form-preview"
    >
      <div class="sf-form-preview__title">表单预览</div>
      <form-create
        v-model="preview.formData"
        :rule="preview.rule"
        :option="preview.option"
      />
    </div>
  </el-form>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import type { FormInstance, FormRules } from 'element-plus';
import { getForm } from '@sail/api/form';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import { setConfAndFields2 } from '@sail/utils/formCreate';
import {
  FORM_TYPE_CUSTOM,
  FORM_TYPE_NORMAL,
  type ProcessEditFormModel
} from './types';

defineOptions({ name: 'SailProcessFormDesign' });

defineProps<{
  formList: Array<{ id: string; name: string }>;
}>();

const model = defineModel<ProcessEditFormModel>({ required: true });
const router = useRouter();
const formRef = ref<FormInstance>();
const preview = reactive({
  formData: {} as Record<string, unknown>,
  rule: [] as any[],
  option: { submitBtn: false, resetBtn: false } as Record<string, unknown>
});

const rules: FormRules = {
  formType: [{ required: true, message: '表单类型不能为空', trigger: 'change' }],
  formId: [{ required: true, message: '流程表单不能为空', trigger: 'change' }],
  formCustomCreatePath: [{ required: true, message: '表单提交路由不能为空', trigger: 'blur' }],
  formCustomViewPath: [{ required: true, message: '表单查看地址不能为空', trigger: 'blur' }]
};

watch(
  () => [model.value.formId, model.value.formType] as const,
  async ([formId, formType]) => {
    if (!formId || formType !== FORM_TYPE_NORMAL) {
      preview.rule = [];
      return;
    }
    try {
      const data = await getForm(formId);
      const r = unpackSailResult<Record<string, unknown>>(data);
      const entity = r.entity || {};
      const conf = String(pickField(entity, 'Conf', 'conf') || '{}');
      const fields = pickField(entity, 'FieldsJson', 'fieldsJson') || '[]';
      setConfAndFields2(preview, conf, fields as any);
      preview.rule.forEach((item: any) => {
        item.props = { ...item.props, disabled: true };
      });
      preview.option = { ...preview.option, submitBtn: false, resetBtn: false };
    } catch {
      preview.rule = [];
    }
  },
  { immediate: true }
);

function goForms() {
  router.push('/sail/forms');
}

async function validate() {
  await formRef.value?.validate();
}

defineExpose({ validate });
</script>

<style scoped>
.sf-edit-form {
  margin-top: 12px;
}
.sf-edit-form__hint {
  margin-top: 4px;
  color: #909399;
  font-size: 12px;
}
.sf-form-preview {
  margin-top: 24px;
  padding: 16px;
  border: 1px solid #ebeef5;
  border-radius: 6px;
  background: #fafafa;
}
.sf-form-preview__title {
  font-weight: 600;
  margin-bottom: 12px;
  padding-left: 8px;
  border-left: 3px solid #3473ff;
}
</style>
