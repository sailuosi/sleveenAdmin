<template>
  <el-form ref="formRef" :model="model" label-width="130px" class="sf-edit-form">
    <el-form-item>
      <template #label><b>提交人权限</b></template>
      <el-checkbox v-model="model.allowCancelRunningProcess">允许撤销审批中的申请</el-checkbox>
    </el-form-item>

    <el-form-item>
      <template #label><b>审批人权限</b></template>
      <div>
        <el-checkbox v-model="model.allowWithdrawTask">允许审批人撤回任务</el-checkbox>
        <div class="sf-edit-form__hint">审批人可撤回正在审批节点的前一节点</div>
      </div>
    </el-form-item>

    <el-form-item>
      <template #label><b>流程编码</b></template>
      <div>
        <div class="sf-extra-rule">
          <el-input
            v-model="model.processIdRule.prefix"
            class="sf-extra-rule__prefix"
            placeholder="前缀"
            :disabled="!model.processIdRule.enable"
          >
            <template #prepend>
              <el-checkbox v-model="model.processIdRule.enable" />
            </template>
          </el-input>
          <el-select
            v-model="model.processIdRule.infix"
            class="sf-extra-rule__infix"
            placeholder="中缀"
            :disabled="!model.processIdRule.enable"
          >
            <el-option label="无" value="" />
            <el-option label="精确到日" value="DAY" />
            <el-option label="精确到时" value="HOUR" />
            <el-option label="精确到分" value="MINUTE" />
            <el-option label="精确到秒" value="SECOND" />
          </el-select>
          <el-input
            v-model="model.processIdRule.postfix"
            class="sf-extra-rule__postfix"
            placeholder="后缀"
            :disabled="!model.processIdRule.enable"
          />
          <el-input-number
            v-model="model.processIdRule.length"
            :min="5"
            :disabled="!model.processIdRule.enable"
          />
        </div>
        <div v-if="model.processIdRule.enable" class="sf-edit-form__hint">
          编码示例：{{ numberExample }}
        </div>
      </div>
    </el-form-item>

    <el-form-item>
      <template #label><b>自动去重</b></template>
      <div>
        <div class="sf-edit-form__hint" style="margin-bottom: 6px">同一审批人在流程中重复出现时：</div>
        <el-radio-group v-model="model.autoApprovalType">
          <div class="sf-extra-radios">
            <el-radio :value="0">不自动通过</el-radio>
            <el-radio :value="1">仅审批一次，后续重复节点自动通过</el-radio>
            <el-radio :value="2">仅针对连续审批节点自动通过</el-radio>
          </div>
        </el-radio-group>
      </div>
    </el-form-item>

    <el-form-item>
      <template #label><b>标题设置</b></template>
      <div>
        <el-radio-group v-model="model.titleSetting.enable">
          <div class="sf-extra-radios">
            <el-radio :value="false">系统默认（展示流程名称）</el-radio>
            <el-radio :value="true">自定义标题</el-radio>
          </div>
        </el-radio-group>
        <el-input
          v-if="model.titleSetting.enable"
          v-model="model.titleSetting.title"
          type="textarea"
          :rows="2"
          placeholder="可用 {字段名} 插入表单字段"
          style="margin-top: 8px; width: 100%"
        />
      </div>
    </el-form-item>

    <el-form-item v-if="model.formType === FORM_TYPE_NORMAL">
      <template #label><b>摘要设置</b></template>
      <div>
        <el-radio-group v-model="model.summarySetting.enable">
          <div class="sf-extra-radios">
            <el-radio :value="false">系统默认（前 3 个字段）</el-radio>
            <el-radio :value="true">自定义摘要</el-radio>
          </div>
        </el-radio-group>
        <el-select
          v-if="model.summarySetting.enable"
          v-model="model.summarySetting.summary"
          multiple
          filterable
          allow-create
          placeholder="选择或输入表单字段 field"
          style="margin-top: 8px; width: 100%"
        >
          <el-option
            v-for="f in formFieldOptions"
            :key="f.value"
            :label="f.label"
            :value="f.value"
          />
        </el-select>
      </div>
    </el-form-item>

    <el-form-item>
      <template #label><b>流程前置通知</b></template>
      <div class="sf-extra-trigger">
        <div class="sf-extra-trigger__row">
          <el-switch v-model="processBeforeEnable" @change="onProcessBefore" />
          <span>流程启动后通知</span>
        </div>
        <el-input
          v-if="processBeforeEnable && model.processBeforeTriggerSetting"
          v-model="model.processBeforeTriggerSetting.url"
          placeholder="回调 URL"
          style="margin-top: 8px"
        />
      </div>
    </el-form-item>
    <el-form-item>
      <template #label><b>流程后置通知</b></template>
      <div class="sf-extra-trigger">
        <div class="sf-extra-trigger__row">
          <el-switch v-model="processAfterEnable" @change="onProcessAfter" />
          <span>流程结束后通知</span>
        </div>
        <el-input
          v-if="processAfterEnable && model.processAfterTriggerSetting"
          v-model="model.processAfterTriggerSetting.url"
          placeholder="回调 URL"
          style="margin-top: 8px"
        />
      </div>
    </el-form-item>
    <el-form-item>
      <template #label><b>任务前置通知</b></template>
      <div class="sf-extra-trigger">
        <div class="sf-extra-trigger__row">
          <el-switch v-model="taskBeforeEnable" @change="onTaskBefore" />
          <span>任务执行时通知</span>
        </div>
        <el-input
          v-if="taskBeforeEnable && model.taskBeforeTriggerSetting"
          v-model="model.taskBeforeTriggerSetting.url"
          placeholder="回调 URL"
          style="margin-top: 8px"
        />
      </div>
    </el-form-item>
    <el-form-item>
      <template #label><b>任务后置通知</b></template>
      <div class="sf-extra-trigger">
        <div class="sf-extra-trigger__row">
          <el-switch v-model="taskAfterEnable" @change="onTaskAfter" />
          <span>任务结束后通知</span>
        </div>
        <el-input
          v-if="taskAfterEnable && model.taskAfterTriggerSetting"
          v-model="model.taskAfterTriggerSetting.url"
          placeholder="回调 URL"
          style="margin-top: 8px"
        />
      </div>
    </el-form-item>

    <el-form-item>
      <template #label><b>自定义打印模板</b></template>
      <div class="sf-extra-trigger">
        <div class="sf-extra-trigger__row">
          <el-switch v-model="model.printTemplateSetting.enable" @change="onPrintEnable" />
          <el-button
            v-if="model.printTemplateSetting.enable"
            link
            type="primary"
            @click="showPrintEditor = !showPrintEditor"
          >
            {{ showPrintEditor ? '收起模板' : '编辑模板' }}
          </el-button>
        </div>
        <el-input
          v-if="model.printTemplateSetting.enable && showPrintEditor"
          v-model="model.printTemplateSetting.template"
          type="textarea"
          :rows="10"
          placeholder="HTML 打印模板"
          style="margin-top: 8px"
        />
      </div>
    </el-form-item>
  </el-form>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import dayjs from 'dayjs';
import { getForm } from '@sail/api/form';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import { decodeFields } from '@sail/utils/formCreate';
import {
  FORM_TYPE_NORMAL,
  type HttpTriggerSetting,
  type ProcessEditFormModel
} from './types';

defineOptions({ name: 'SailProcessExtraSettings' });

const model = defineModel<ProcessEditFormModel>({ required: true });
const formRef = ref();
const showPrintEditor = ref(false);
const formFieldOptions = ref<Array<{ label: string; value: string }>>([]);

const emptyTrigger = (): HttpTriggerSetting => ({
  url: '',
  header: [],
  body: [],
  response: []
});

const processBeforeEnable = ref(false);
const processAfterEnable = ref(false);
const taskBeforeEnable = ref(false);
const taskAfterEnable = ref(false);

function onProcessBefore(v: boolean) {
  model.value.processBeforeTriggerSetting = v ? emptyTrigger() : null;
}
function onProcessAfter(v: boolean) {
  model.value.processAfterTriggerSetting = v ? emptyTrigger() : null;
}
function onTaskBefore(v: boolean) {
  model.value.taskBeforeTriggerSetting = v ? emptyTrigger() : null;
}
function onTaskAfter(v: boolean) {
  model.value.taskAfterTriggerSetting = v ? emptyTrigger() : null;
}

const numberExample = computed(() => {
  const rule = model.value.processIdRule;
  if (!rule.enable) return '';
  let infix = '';
  switch (rule.infix) {
    case 'DAY':
      infix = dayjs().format('YYYYMMDD');
      break;
    case 'HOUR':
      infix = dayjs().format('YYYYMMDDHH');
      break;
    case 'MINUTE':
      infix = dayjs().format('YYYYMMDDHHmm');
      break;
    case 'SECOND':
      infix = dayjs().format('YYYYMMDDHHmmss');
      break;
  }
  return `${rule.prefix}${infix}${rule.postfix}${'1'.padStart(Math.max(rule.length - 1, 1), '0')}`;
});

const defaultPrintTemplate =
  '<h2 style="text-align:center">{{processName}}</h2><p>发起人：{{startUser}}　发起时间：{{startTime}}</p><hr/><div>{{formBody}}</div><p>流程记录见办理页打印</p>';

function onPrintEnable(v: boolean) {
  if (v && !model.value.printTemplateSetting.template) {
    model.value.printTemplateSetting.template = defaultPrintTemplate;
  }
}

function initData() {
  processBeforeEnable.value = !!model.value.processBeforeTriggerSetting;
  processAfterEnable.value = !!model.value.processAfterTriggerSetting;
  taskBeforeEnable.value = !!model.value.taskBeforeTriggerSetting;
  taskAfterEnable.value = !!model.value.taskAfterTriggerSetting;
  if (!model.value.processIdRule) {
    model.value.processIdRule = { enable: false, prefix: '', infix: '', postfix: '', length: 5 };
  }
  if (!model.value.titleSetting) model.value.titleSetting = { enable: false, title: '' };
  if (!model.value.summarySetting) model.value.summarySetting = { enable: false, summary: [] };
  if (!model.value.printTemplateSetting) {
    model.value.printTemplateSetting = { enable: false, template: '' };
  }
}

watch(
  () => model.value.formId,
  async formId => {
    formFieldOptions.value = [];
    if (!formId || model.value.formType !== FORM_TYPE_NORMAL) return;
    try {
      const data = await getForm(formId);
      const r = unpackSailResult<Record<string, unknown>>(data);
      const fieldsRaw = pickField(r.entity, 'FieldsJson', 'fieldsJson') || '[]';
      const rules = decodeFields(fieldsRaw as any) as Array<{ field?: string; title?: string; props?: any }>;
      formFieldOptions.value = rules
        .map(item => ({
          value: String(item.field || item.props?.field || ''),
          label: String(item.title || item.props?.title || item.field || '')
        }))
        .filter(x => x.value);
    } catch {
      formFieldOptions.value = [];
    }
  },
  { immediate: true }
);

defineExpose({ initData });
</script>

<style scoped>
.sf-edit-form {
  margin-top: 12px;
}
.sf-edit-form__hint {
  margin-top: 4px;
  color: #909399;
  font-size: 12px;
  line-height: 1.4;
}
.sf-extra-rule {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
}
.sf-extra-rule__prefix {
  width: 160px;
}
.sf-extra-rule__infix {
  width: 130px;
}
.sf-extra-rule__postfix {
  width: 90px;
}
.sf-extra-radios {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
}
.sf-extra-trigger {
  width: 100%;
}
.sf-extra-trigger__row {
  display: flex;
  align-items: center;
  gap: 16px;
}
</style>
