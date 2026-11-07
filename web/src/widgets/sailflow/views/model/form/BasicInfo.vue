<template>
  <el-form ref="formRef" :model="model" :rules="rules" label-width="120px" class="sf-edit-form">
    <el-form-item label="流程标识" prop="processCode">
      <div class="sf-edit-form__row">
        <el-input
          v-model="model.processCode"
          :disabled="!!model.processId"
          placeholder="请输入流程标识，以字母或下划线开头"
        />
        <el-tooltip
          :content="model.processId ? '流程标识不可修改' : '新建后，流程标识不可修改'"
          placement="top"
        >
          <span class="sf-edit-form__tip">?</span>
        </el-tooltip>
      </div>
    </el-form-item>
    <el-form-item label="流程名称" prop="processName">
      <el-input v-model="model.processName" clearable placeholder="请输入流程名称" />
    </el-form-item>
    <el-form-item label="流程分类" prop="category">
      <el-select v-model="model.category" clearable filterable placeholder="请选择流程分类" style="width: 100%">
        <el-option v-for="c in categoryList" :key="c.Code" :label="c.Name" :value="c.Code" />
      </el-select>
    </el-form-item>
    <el-form-item label="流程图标">
      <el-input v-model="model.icon" clearable placeholder="图标 URL 或 emoji（可选）" />
    </el-form-item>
    <el-form-item label="流程描述">
      <el-input v-model="model.description" type="textarea" :rows="3" clearable />
    </el-form-item>
    <el-form-item label="是否可见" prop="visible">
      <el-radio-group v-model="model.visible">
        <el-radio :value="true">是</el-radio>
        <el-radio :value="false">否</el-radio>
      </el-radio-group>
      <div class="sf-edit-form__hint">控制「发起流程」列表是否展示</div>
    </el-form-item>
    <el-form-item label="谁可以发起" prop="startUserType">
      <el-select v-model="model.startUserType" style="width: 100%" @change="onStartTypeChange">
        <el-option label="全员" :value="0" />
        <el-option label="指定人员" :value="1" />
        <el-option label="指定部门" :value="2" />
      </el-select>
      <el-select
        v-if="model.startUserType === 1"
        v-model="model.startUserIds"
        multiple
        filterable
        allow-create
        default-first-option
        placeholder="输入用户 ID 回车添加"
        style="width: 100%; margin-top: 8px"
      />
      <el-select
        v-if="model.startUserType === 2"
        v-model="model.startDeptIds"
        multiple
        filterable
        allow-create
        default-first-option
        placeholder="输入部门 ID 回车添加"
        style="width: 100%; margin-top: 8px"
      />
    </el-form-item>
    <el-form-item label="流程管理员">
      <el-select
        v-model="model.managerUserIds"
        multiple
        filterable
        allow-create
        default-first-option
        placeholder="输入用户 ID 回车添加"
        style="width: 100%"
      />
    </el-form-item>
  </el-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { FormInstance, FormRules } from 'element-plus';
import type { ProcessEditFormModel } from './types';

defineOptions({ name: 'SailProcessBasicInfo' });

defineProps<{
  categoryList: Array<{ Code: string; Name: string }>;
}>();

const model = defineModel<ProcessEditFormModel>({ required: true });
const formRef = ref<FormInstance>();

const rules: FormRules = {
  processName: [{ required: true, message: '流程名称不能为空', trigger: 'blur' }],
  processCode: [
    { required: true, message: '流程标识不能为空', trigger: 'blur' },
    {
      validator: (_r, value: string, cb) => {
        if (!value) return cb();
        if (!/^[a-zA-Z_][\-_.0-9_a-zA-Z$]*$/.test(value)) {
          return cb(new Error('须以字母或下划线开头，可含字母数字._-$'));
        }
        cb();
      },
      trigger: 'blur'
    }
  ],
  category: [{ required: true, message: '流程分类不能为空', trigger: 'change' }],
  visible: [{ required: true, message: '请选择是否可见', trigger: 'change' }]
};

function onStartTypeChange(v: number) {
  if (v === 0) {
    model.value.startUserIds = [];
    model.value.startDeptIds = [];
  } else if (v === 1) {
    model.value.startDeptIds = [];
  } else {
    model.value.startUserIds = [];
  }
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
.sf-edit-form__row {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
}
.sf-edit-form__tip {
  display: inline-flex;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 1px solid #c0c4cc;
  color: #909399;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  flex-shrink: 0;
  cursor: help;
}
.sf-edit-form__hint {
  margin-top: 4px;
  color: #909399;
  font-size: 12px;
  line-height: 1.4;
}
</style>
