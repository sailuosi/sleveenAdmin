<template>
  <el-dialog
    v-model="visible"
    :title="title"
    width="640px"
    top="5vh"
    append-to-body
    destroy-on-close
    @closed="onClosed"
  >
    <el-form v-if="kind && model" label-position="top" class="sf-el-config">
      <el-card shadow="never" class="sf-el-config__card">
        <template #header><span class="sf-el-config__h">基本信息</span></template>
        <el-form-item label="名称">
          <el-input v-model="(model as any).name" clearable />
        </el-form-item>
        <el-form-item label="Id">
          <el-input :model-value="(model as any).id" disabled />
        </el-form-item>
        <el-form-item label="文档说明">
          <el-input v-model="(model as any).documentation" type="textarea" :rows="2" clearable />
        </el-form-item>
      </el-card>

      <template v-if="kind === 'gateway' && gateway">
        <el-card shadow="never" class="sf-el-config__card">
          <template #header><span class="sf-el-config__h">网关扩展</span></template>
          <el-form-item v-if="gateway.showSplit" label="扩展分支类型">
            <el-select v-model="gateway.extraSplitType" style="width: 100%">
              <el-option v-for="o in SPLIT_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item v-if="gateway.showJoin" label="扩展合并类型">
            <el-select v-model="gateway.extraJoinType" style="width: 100%">
              <el-option v-for="o in JOIN_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item v-if="gateway.showJoin" label="合并通过类型">
            <el-select v-model="gateway.joinPassType" style="width: 100%">
              <el-option v-for="o in PASS_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
        </el-card>
      </template>

      <template v-else-if="kind === 'service' && service">
        <el-card shadow="never" class="sf-el-config__card">
          <template #header><span class="sf-el-config__h">服务调用</span></template>
          <el-form-item label="调用类型">
            <el-select v-model="service.methodType" style="width: 100%">
              <el-option v-for="o in METHOD_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="HTTP 方法">
            <el-select v-model="service.subMethodType" style="width: 100%">
              <el-option v-for="o in HTTP_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="参数"><el-input v-model="service.argus" clearable /></el-form-item>
          <el-form-item label="表达式 / URL">
            <el-input v-model="service.expression" type="textarea" :rows="2" />
          </el-form-item>
        </el-card>
        <el-card shadow="never" class="sf-el-config__card">
          <template #header><span class="sf-el-config__h">AI 服务（可选）</span></template>
          <el-form-item label="AI 类型">
            <el-select v-model="service.aiType" clearable style="width: 100%">
              <el-option v-for="o in AI_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item v-if="service.aiType" label="Config UUID">
            <el-input v-model="service.configUUID" clearable />
          </el-form-item>
          <el-form-item v-if="service.aiType === '3'" label="ToolSet">
            <el-input v-model="service.toolSetName" clearable />
          </el-form-item>
        </el-card>
      </template>

      <template v-else-if="kind === 'script' && script">
        <el-card shadow="never" class="sf-el-config__card">
          <template #header><span class="sf-el-config__h">脚本</span></template>
          <el-form-item label="脚本类型">
            <el-select v-model="script.scriptType" style="width: 100%">
              <el-option v-for="o in SCRIPT_OPTS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="参数"><el-input v-model="script.argus" clearable /></el-form-item>
          <el-form-item label="脚本内容">
            <el-input v-model="script.scriptText" type="textarea" :rows="8" />
          </el-form-item>
        </el-card>
      </template>

      <template v-else-if="kind === 'rule' && rule">
        <el-card shadow="never" class="sf-el-config__card">
          <template #header><span class="sf-el-config__h">规则集</span></template>
          <el-form-item label="RuleSet Code">
            <div class="sf-el-config__row">
              <el-input v-model="rule.ruleSetCode" clearable />
              <el-button @click="pickRule">选择</el-button>
            </div>
          </el-form-item>
        </el-card>
      </template>

      <template v-else-if="kind === 'subprocess' && sub">
        <el-card shadow="never" class="sf-el-config__card">
          <template #header><span class="sf-el-config__h">子流程绑定</span></template>
          <el-form-item label="子流程">
            <div class="sf-el-config__row">
              <el-input
                :model-value="sub.subProcessName || sub.subProcessId || '（未绑定）'"
                disabled
              />
              <el-button type="primary" @click="pickSub">选择</el-button>
            </div>
          </el-form-item>
        </el-card>
      </template>

      <template v-else-if="kind === 'event' && event">
        <el-card shadow="never" class="sf-el-config__card">
          <template #header>
            <span class="sf-el-config__h">触发 · {{ event.eventDefType || 'Event' }}</span>
          </template>
          <template v-if="event.eventDefType === 'TimerEventDefinition'">
            <el-form-item label="定时类型">
              <el-radio-group v-model="event.timerKind">
                <el-radio value="timeDate">时间点</el-radio>
                <el-radio value="timeDuration">持续</el-radio>
                <el-radio value="timeCycle">循环</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="表达式">
              <el-input v-model="event.timerBody" clearable placeholder="PT10M / ISO 日期 / R3/PT10M" />
            </el-form-item>
          </template>
          <el-form-item v-else-if="event.eventDefType === 'ConditionalEventDefinition'" label="条件">
            <el-input v-model="event.condition" type="textarea" :rows="3" />
          </el-form-item>
          <el-form-item v-else-if="event.eventDefType === 'MessageEventDefinition'" label="消息名">
            <el-input v-model="event.messageName" clearable />
          </el-form-item>
          <el-form-item v-else-if="event.eventDefType === 'SignalEventDefinition'" label="信号名">
            <el-input v-model="event.signalName" clearable />
          </el-form-item>
          <el-form-item v-else-if="event.eventDefType === 'ErrorEventDefinition'" label="错误引用">
            <el-input v-model="event.errorName" clearable />
          </el-form-item>
          <el-form-item v-else-if="event.eventDefType === 'EscalationEventDefinition'" label="升级引用">
            <el-input v-model="event.escalationName" clearable />
          </el-form-item>
          <el-form-item v-else-if="event.eventDefType === 'LinkEventDefinition'" label="链接名">
            <el-input v-model="event.linkName" clearable />
          </el-form-item>
          <el-alert
            v-else
            type="info"
            :closable="false"
            :title="event.eventDefType ? `事件定义：${event.eventDefType}` : '当前事件定义无额外可编辑字段'"
          />
        </el-card>
      </template>
    </el-form>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import {
  loadEvent,
  loadGateway,
  loadRule,
  loadScript,
  loadService,
  loadSubProcess,
  saveEvent,
  saveGateway,
  saveRule,
  saveScript,
  saveService,
  saveSubProcess
} from '@sail/bpmn/elementConfig/loadSave';
import {
  AI_OPTS,
  HTTP_OPTS,
  JOIN_OPTS,
  METHOD_OPTS,
  PASS_OPTS,
  SCRIPT_OPTS,
  SPLIT_OPTS,
  type ElementConfigKind,
  type SailEventModel,
  type SailGatewayModel,
  type SailRuleModel,
  type SailScriptModel,
  type SailServiceModel,
  type SailSubProcessModel
} from '@sail/bpmn/elementConfig/types';

defineOptions({ name: 'SailElementConfigDialog' });

const visible = ref(false);
const saving = ref(false);
const kind = ref<ElementConfigKind | null>(null);
const model = ref<
  | SailGatewayModel
  | SailServiceModel
  | SailScriptModel
  | SailRuleModel
  | SailSubProcessModel
  | SailEventModel
  | null
>(null);

const gateway = computed(() => (kind.value === 'gateway' ? (model.value as SailGatewayModel) : null));
const service = computed(() => (kind.value === 'service' ? (model.value as SailServiceModel) : null));
const script = computed(() => (kind.value === 'script' ? (model.value as SailScriptModel) : null));
const rule = computed(() => (kind.value === 'rule' ? (model.value as SailRuleModel) : null));
const sub = computed(() => (kind.value === 'subprocess' ? (model.value as SailSubProcessModel) : null));
const event = computed(() => (kind.value === 'event' ? (model.value as SailEventModel) : null));

let element: unknown = null;
let modeling: {
  updateProperties: (el: unknown, props: object) => void;
  updateModdleProperties?: (el: unknown, m: unknown, props: object) => void;
} | null = null;
let moddle: { create: (type: string, props?: object) => any } | null = null;

const title = computed(() => {
  const map: Record<string, string> = {
    gateway: '网关配置',
    service: '服务任务配置',
    script: '脚本任务配置',
    rule: '规则任务配置',
    subprocess: '子流程配置',
    event: '事件配置'
  };
  const n = (model.value as any)?.name || (model.value as any)?.id || '';
  const base = kind.value ? map[kind.value] : '图元配置';
  return n ? `${base} · ${n}` : base;
});

function openKind(k: ElementConfigKind, payload: {
  element: unknown;
  modeling: typeof modeling;
  moddle: typeof moddle;
}) {
  element = payload.element;
  modeling = payload.modeling;
  moddle = payload.moddle;
  kind.value = k;
  if (k === 'gateway') model.value = loadGateway(payload.element);
  else if (k === 'service') model.value = loadService(payload.element);
  else if (k === 'script') model.value = loadScript(payload.element);
  else if (k === 'rule') model.value = loadRule(payload.element);
  else if (k === 'subprocess') model.value = loadSubProcess(payload.element);
  else if (k === 'event') model.value = loadEvent(payload.element);
  visible.value = true;
}

function pickRule() {
  if (!rule.value) return;
  designerEvents.emit('ruleset:pick', {
    currentCode: rule.value.ruleSetCode,
    onConfirm: code => {
      if (rule.value) rule.value.ruleSetCode = code;
    }
  });
}

function pickSub() {
  if (!sub.value) return;
  designerEvents.emit('subprocess:pick', {
    onConfirm: draft => {
      if (!sub.value) return;
      sub.value.subProcessId = draft.subProcessId;
      sub.value.subProcessName = draft.subProcessName;
    }
  });
}

function onSave() {
  if (!kind.value || !model.value || !element || !modeling || !moddle) return;
  saving.value = true;
  try {
    if (kind.value === 'gateway') saveGateway(element, model.value as SailGatewayModel, modeling, moddle);
    else if (kind.value === 'service')
      saveService(element, model.value as SailServiceModel, modeling, moddle);
    else if (kind.value === 'script')
      saveScript(element, model.value as SailScriptModel, modeling, moddle);
    else if (kind.value === 'rule') saveRule(element, model.value as SailRuleModel, modeling, moddle);
    else if (kind.value === 'subprocess')
      saveSubProcess(element, model.value as SailSubProcessModel, modeling, moddle);
    else if (kind.value === 'event') saveEvent(element, model.value as SailEventModel, modeling, moddle);
    ElMessage.success('已写入图元配置');
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

function onClosed() {
  model.value = null;
  kind.value = null;
  element = null;
  modeling = null;
  moddle = null;
}

const openGateway = (p: any) => openKind('gateway', p);
const openService = (p: any) => openKind('service', p);
const openScript = (p: any) => openKind('script', p);
const openRule = (p: any) => openKind('rule', p);
const openSub = (p: any) => openKind('subprocess', p);
const openEvent = (p: any) => openKind('event', p);

onMounted(() => {
  designerEvents.on('gatewayconfig:open', openGateway);
  designerEvents.on('servicetaskconfig:open', openService);
  designerEvents.on('scripttaskconfig:open', openScript);
  designerEvents.on('ruletaskconfig:open', openRule);
  designerEvents.on('subprocessconfig:open', openSub);
  designerEvents.on('eventconfig:open', openEvent);
});
onBeforeUnmount(() => {
  designerEvents.off('gatewayconfig:open', openGateway);
  designerEvents.off('servicetaskconfig:open', openService);
  designerEvents.off('scripttaskconfig:open', openScript);
  designerEvents.off('ruletaskconfig:open', openRule);
  designerEvents.off('subprocessconfig:open', openSub);
  designerEvents.off('eventconfig:open', openEvent);
});
</script>

<style scoped>
.sf-el-config {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: min(70vh, 640px);
  overflow: auto;
}
.sf-el-config__card {
  border-radius: 8px;
}
.sf-el-config__h {
  font-weight: 600;
}
.sf-el-config__row {
  display: flex;
  gap: 8px;
  width: 100%;
}
</style>
