<template>
  <div class="sf-model-edit">
    <header class="sf-model-edit__bar">
      <div class="sf-model-edit__left">
        <el-button text @click="handleBack">← 返回</el-button>
        <span class="sf-model-edit__title" :title="formData.processName || pageTitle">
          {{ formData.processName || pageTitle }}
        </span>
      </div>
      <div class="sf-model-edit__steps">
        <div
          v-for="(step, index) in steps"
          :key="step.title"
          class="sf-model-edit__step"
          :class="{ 'is-active': currentStep === index }"
          @click="handleStepClick(index)"
        >
          <span class="sf-model-edit__step-num">{{ index + 1 }}</span>
          <span class="sf-model-edit__step-label">{{ step.title }}</span>
        </div>
      </div>
      <div class="sf-model-edit__actions">
        <el-button v-if="actionType === 'update' && formData.processId" type="success" :loading="saving" @click="handleDeploy">
          发布
        </el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存</el-button>
      </div>
    </header>

    <main v-loading="loading" class="sf-model-edit__body">
      <div v-show="currentStep === 0" class="sf-model-edit__pane sf-model-edit__pane--narrow">
        <BasicInfo ref="basicInfoRef" v-model="formData" :category-list="categoryList" />
      </div>
      <div v-show="currentStep === 1" class="sf-model-edit__pane sf-model-edit__pane--narrow">
        <FormDesign ref="formDesignRef" v-model="formData" :form-list="formList" />
      </div>
      <div v-show="currentStep === 2" class="sf-model-edit__pane sf-model-edit__pane--wide">
        <ProcessDesign ref="processDesignRef" v-model="formData" />
      </div>
      <div v-show="currentStep === 3" class="sf-model-edit__pane sf-model-edit__pane--medium">
        <ExtraSettings ref="extraSettingsRef" v-model="formData" />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { getProcessEdit, saveProcessEdit } from '@sail/api/processEdit';
import { updateProcessUsingState } from '@sail/api/process';
import { getFormList } from '@sail/api/form';
import { listCategories } from '@sail/api/category';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import BasicInfo from './BasicInfo.vue';
import FormDesign from './FormDesign.vue';
import ProcessDesign from './ProcessDesign.vue';
import ExtraSettings from './ExtraSettings.vue';
import {
  applyExtraJson,
  createEmptyProcessEditForm,
  packExtraJson,
  parseIdList,
  type ProcessEditFormModel
} from './types';

defineOptions({ name: 'SailProcessEditPage' });

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const saving = ref(false);
const currentStep = ref(0);
const actionType = ref<'create' | 'update' | 'copy'>('create');
const pageTitle = ref('创建流程');
const formData = ref<ProcessEditFormModel>(createEmptyProcessEditForm());
const categoryList = ref<Array<{ Code: string; Name: string }>>([]);
const formList = ref<Array<{ id: string; name: string }>>([]);

const basicInfoRef = ref<InstanceType<typeof BasicInfo> | null>(null);
const formDesignRef = ref<InstanceType<typeof FormDesign> | null>(null);
const processDesignRef = ref<InstanceType<typeof ProcessDesign> | null>(null);
const extraSettingsRef = ref<InstanceType<typeof ExtraSettings> | null>(null);

const steps = [
  { title: '基本信息' },
  { title: '表单设计' },
  { title: '流程设计' },
  { title: '更多设置' }
];

async function validateBasic() {
  await basicInfoRef.value?.validate();
}
async function validateForm() {
  await formDesignRef.value?.validate();
}
async function validateProcess() {
  await processDesignRef.value?.validate();
}

async function loadLookups() {
  const [catRes, formRes] = await Promise.all([listCategories(), getFormList()]);
  const cats = unpackSailResult<any[]>(catRes).entity || [];
  categoryList.value = (Array.isArray(cats) ? cats : []).map(c => ({
    Code: String(pickField(c, 'Code', 'code') || ''),
    Name: String(pickField(c, 'Name', 'name') || '')
  })).filter(c => c.Code);

  const forms = unpackSailResult<any[]>(formRes).entity || [];
  formList.value = (Array.isArray(forms) ? forms : []).map(f => ({
    id: String(pickField(f, 'FormId', 'formId', 'Id', 'id') || ''),
    name: String(pickField(f, 'FormName', 'formName', 'Name', 'name') || '')
  })).filter(f => f.id);
}

function applyBundle(bundle: Record<string, unknown>) {
  const process = (pickField(bundle, 'Process', 'process') || {}) as Record<string, unknown>;
  const meta = (pickField(bundle, 'Meta', 'meta') || {}) as Record<string, unknown>;
  const model = createEmptyProcessEditForm();
  model.processId = String(pickField(process, 'ProcessId', 'processId') || '');
  model.version = String(pickField(process, 'Version', 'version') || '1');
  model.processName = String(pickField(process, 'ProcessName', 'processName') || '');
  model.processCode = String(pickField(process, 'ProcessCode', 'processCode') || '');
  model.category = String(pickField(process, 'AppType', 'appType') || '') || undefined;
  model.icon = String(pickField(process, 'Icon', 'icon') || '');
  model.description = String(pickField(process, 'Description', 'description') || '');
  model.hasBpmn = Boolean(pickField(bundle, 'HasBpmn', 'hasBpmn'));

  model.formType = Number(pickField(meta, 'FormType', 'formType') ?? 10);
  model.formId = String(pickField(meta, 'FormId', 'formId') || '');
  model.formCustomCreatePath = String(pickField(meta, 'FormCustomCreatePath', 'formCustomCreatePath') || '');
  model.formCustomViewPath = String(pickField(meta, 'FormCustomViewPath', 'formCustomViewPath') || '');
  const visible = pickField(meta, 'Visible', 'visible');
  model.visible = visible === 0 || visible === '0' || visible === false ? false : true;
  model.startUserType = Number(pickField(meta, 'StartUserType', 'startUserType') ?? 0);
  model.startUserIds = parseIdList(pickField(meta, 'StartUserIds', 'startUserIds'));
  model.startDeptIds = parseIdList(pickField(meta, 'StartDeptIds', 'startDeptIds'));
  model.managerUserIds = parseIdList(pickField(meta, 'ManagerUserIds', 'managerUserIds'));
  applyExtraJson(model, pickField(meta, 'ExtraJson', 'extraJson'));
  formData.value = model;
}

async function init() {
  loading.value = true;
  try {
    await loadLookups();
    const type = String(route.query.type || 'create') as 'create' | 'update' | 'copy';
    actionType.value = type;
    const processId = route.query.processId ? String(route.query.processId) : '';
    const version = route.query.version ? String(route.query.version) : '1';
    const category = route.query.category ? String(route.query.category) : '';

    if (type === 'create' || !processId) {
      pageTitle.value = '创建流程';
      formData.value = createEmptyProcessEditForm();
      if (category) formData.value.category = category;
      actionType.value = 'create';
    } else {
      pageTitle.value = type === 'copy' ? '复制流程' : '修改流程';
      const data = await getProcessEdit({ ProcessId: processId, Version: version });
      const r = unpackSailResult<Record<string, unknown>>(data);
      if (!r.ok || !r.entity) {
        ElMessage.error(r.message || '加载流程失败');
        return;
      }
      applyBundle(r.entity);
      if (type === 'copy') {
        formData.value.processId = undefined;
        formData.value.processName = `${formData.value.processName}副本`;
        formData.value.processCode = `${formData.value.processCode}_copy`;
        formData.value.hasBpmn = false;
        actionType.value = 'copy';
      } else {
        actionType.value = 'update';
      }
    }
    await nextTick();
    extraSettingsRef.value?.initData?.();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    loading.value = false;
  }
}

function buildSavePayload() {
  const m = formData.value;
  return {
    ProcessId: m.processId,
    Version: m.version || '1',
    ProcessName: m.processName,
    ProcessCode: m.processCode,
    Description: m.description,
    AppType: m.category || '',
    Icon: m.icon,
    Status: undefined as number | undefined,
    FormType: m.formType,
    FormId: m.formId,
    FormCustomCreatePath: m.formCustomCreatePath,
    FormCustomViewPath: m.formCustomViewPath,
    Visible: m.visible ? 1 : 0,
    StartUserType: m.startUserType,
    StartUserIds: JSON.stringify(m.startUserIds || []),
    StartDeptIds: JSON.stringify(m.startDeptIds || []),
    ManagerUserIds: JSON.stringify(m.managerUserIds || []),
    ExtraJson: packExtraJson(m)
  };
}

async function validateCurrentStep() {
  if (currentStep.value === 0) {
    try {
      await validateBasic();
    } catch {
      throw new Error('请完善基本信息');
    }
    return;
  }
  if (currentStep.value === 1) {
    try {
      await validateForm();
    } catch {
      throw new Error('请完善表单设计信息');
    }
    return;
  }
  if (currentStep.value === 2 && formData.value.processId) {
    try {
      await validateProcess();
    } catch (e: any) {
      throw new Error(e?.message || '请设计流程');
    }
  }
}

async function validateAllSteps() {
  try {
    await validateBasic();
  } catch {
    currentStep.value = 0;
    throw new Error('请完善基本信息');
  }
  try {
    await validateForm();
  } catch {
    currentStep.value = 1;
    throw new Error('请完善表单设计信息');
  }
  if (formData.value.processId) {
    try {
      await validateProcess();
    } catch (e: any) {
      currentStep.value = 2;
      throw new Error(e?.message || '请设计流程');
    }
  }
}

async function handleSave() {
  saving.value = true;
  try {
    // 保存只校验当前步骤，便于分步填写；发布时再全量校验
    await validateCurrentStep();
    // 流程设计步：先让 iframe 内设计器落库 BPMN，再写元数据
    if (currentStep.value === 2 && formData.value.processId) {
      const ok = await processDesignRef.value?.saveDesigner?.();
      if (ok === false) {
        ElMessage.warning('流程图未保存成功，已取消元数据保存');
        return;
      }
    }
    const data = await saveProcessEdit(buildSavePayload());
    const r = unpackSailResult<Record<string, unknown>>(data);
    if (!r.ok) {
      ElMessage.error(r.message || '保存失败');
      return;
    }
    if (r.entity) applyBundle(r.entity);
    actionType.value = 'update';
    pageTitle.value = '修改流程';
    ElMessage.success('保存成功');
    // 同步 URL，便于刷新
    if (formData.value.processId) {
      router.replace({
        path: '/sail/process-edit',
        query: {
          type: 'update',
          processId: formData.value.processId,
          version: formData.value.version || '1'
        }
      });
    }
  } catch (e: unknown) {
    ElMessage.warning(e instanceof Error ? e.message : '请完善必填信息');
  } finally {
    saving.value = false;
  }
}

async function handleDeploy() {
  if (!formData.value.processId) {
    ElMessage.warning('请先保存流程');
    return;
  }
  saving.value = true;
  try {
    await validateAllSteps();
    const data = await saveProcessEdit(buildSavePayload());
    const r = unpackSailResult(data);
    if (!r.ok) {
      ElMessage.error(r.message || '保存失败');
      return;
    }
    const pub = await updateProcessUsingState({
      ProcessId: formData.value.processId!,
      Version: formData.value.version || '1',
      UsingState: 1
    });
    const pr = unpackSailResult(pub);
    if (!pr.ok && (pub?.Status ?? pub?.status) === -1) {
      ElMessage.error(pr.message || '发布失败');
      return;
    }
    ElMessage.success('发布成功');
    router.push('/sail/processes');
  } catch (e: unknown) {
    ElMessage.warning(e instanceof Error ? e.message : '发布失败');
  } finally {
    saving.value = false;
  }
}

async function handleStepClick(index: number) {
  try {
    if (index > 0) await validateBasic();
    if (index > 1) await validateForm();
    if (index > 2 && formData.value.processId) {
      // 流程设计软校验：未保存可不阻断切到更多设置
    }
    currentStep.value = index;
    if (index === 2) {
      await nextTick();
      processDesignRef.value?.refresh?.();
    }
    if (index === 3) {
      await nextTick();
      extraSettingsRef.value?.initData?.();
    }
  } catch {
    ElMessage.warning('请先完善当前步骤必填信息');
  }
}

function handleBack() {
  router.push('/sail/processes');
}

onMounted(init);
</script>

<style scoped>
.sf-model-edit {
  min-height: 100vh;
  background: #f5f7fa;
  display: flex;
  flex-direction: column;
}
.sf-model-edit__bar {
  height: 56px;
  background: #fff;
  border-bottom: 1px solid #dcdfe6;
  display: flex;
  align-items: center;
  padding: 0 16px;
  gap: 12px;
  flex-shrink: 0;
}
.sf-model-edit__left {
  width: 220px;
  display: flex;
  align-items: center;
  gap: 4px;
  overflow: hidden;
}
.sf-model-edit__title {
  font-size: 16px;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.sf-model-edit__steps {
  flex: 1;
  display: flex;
  justify-content: center;
  gap: 8px;
  height: 100%;
}
.sf-model-edit__step {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
  cursor: pointer;
  color: #909399;
  border-bottom: 2px solid transparent;
  height: 100%;
}
.sf-model-edit__step.is-active {
  color: #3473ff;
  border-bottom-color: #3473ff;
}
.sf-model-edit__step-num {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 2px solid #dcdfe6;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}
.sf-model-edit__step.is-active .sf-model-edit__step-num {
  background: #3473ff;
  border-color: #3473ff;
  color: #fff;
}
.sf-model-edit__step-label {
  font-size: 15px;
  font-weight: 600;
  white-space: nowrap;
}
.sf-model-edit__actions {
  width: 200px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
.sf-model-edit__body {
  flex: 1;
  padding: 16px;
  overflow: auto;
}
.sf-model-edit__pane--narrow {
  max-width: 560px;
  margin: 0 auto;
  background: #fff;
  padding: 16px 24px 32px;
  border-radius: 8px;
}
.sf-model-edit__pane--medium {
  max-width: 720px;
  margin: 0 auto;
  background: #fff;
  padding: 16px 24px 32px;
  border-radius: 8px;
}
.sf-model-edit__pane--wide {
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
}
</style>
