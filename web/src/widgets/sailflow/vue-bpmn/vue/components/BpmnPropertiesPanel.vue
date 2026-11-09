<script setup lang="ts">
import { computed } from 'vue';
import {
  getGeneralEntries,
  getPropertyGroups,
  type DiagramSnapshot,
} from '@vue-bpmn/core';

const props = defineProps<{
  state: DiagramSnapshot;
}>();

const emit = defineEmits<{
  updateName: [id: string, name: string];
  updateId: [id: string, nextId: string];
  updateProperty: [id: string, key: string, value: unknown];
}>();

const selected = computed(() => {
  if (props.state.selection.length !== 1) return null;
  const id = props.state.selection[0]!;
  return props.state.elements[id] ?? null;
});

const placeholder = computed(() => {
  if (props.state.selection.length === 0) return 'empty';
  if (props.state.selection.length > 1) return 'multiple';
  return null;
});

const groups = computed(() =>
  selected.value ? getPropertyGroups(props.state, selected.value.id) : [],
);

const generalEntries = computed(() =>
  selected.value ? getGeneralEntries(selected.value) : [],
);

const processRef = computed(() => {
  if (!selected.value || selected.value.type !== 'bpmn:Participant') return null;
  const ref = selected.value.businessObject.processRef as string | undefined;
  return ref ? props.state.elements[ref] ?? null : null;
});

const TYPE_LABELS: Record<string, string> = {
  'bpmn:StartEvent': '开始事件',
  'bpmn:EndEvent': '结束事件',
  'bpmn:IntermediateThrowEvent': '中间抛出事件',
  'bpmn:IntermediateCatchEvent': '中间捕获事件',
  'bpmn:BoundaryEvent': '边界事件',
  'bpmn:Task': '任务',
  'bpmn:UserTask': '用户任务',
  'bpmn:ServiceTask': '服务任务',
  'bpmn:ScriptTask': '脚本任务',
  'bpmn:SendTask': '发送任务',
  'bpmn:ReceiveTask': '接收任务',
  'bpmn:ManualTask': '手工任务',
  'bpmn:BusinessRuleTask': '业务规则任务',
  'bpmn:ExclusiveGateway': '排他网关',
  'bpmn:ParallelGateway': '并行网关',
  'bpmn:InclusiveGateway': '包容网关',
  'bpmn:EventBasedGateway': '事件网关',
  'bpmn:ComplexGateway': '复杂网关',
  'bpmn:SubProcess': '子流程',
  'bpmn:Transaction': '事务',
  'bpmn:AdHocSubProcess': '即席子流程',
  'bpmn:CallActivity': '调用活动',
  'bpmn:Participant': '参与者',
  'bpmn:Lane': '泳道',
  'bpmn:SequenceFlow': '顺序流',
  'bpmn:MessageFlow': '消息流',
  'bpmn:DataObjectReference': '数据对象',
  'bpmn:DataStoreReference': '数据存储',
  'bpmn:Group': '分组',
  'bpmn:TextAnnotation': '文本注释',
  'bpmn:Process': '流程',
  'bpmn:Collaboration': '协作',
};

const typeLabel = computed(() => {
  if (!selected.value) return '';
  return TYPE_LABELS[selected.value.type] ?? selected.value.type.replace(/^bpmn:/, '');
});

function onDoc(value: string) {
  if (!selected.value) return;
  emit('updateProperty', selected.value.id, 'documentation', value);
}

function onExecutable(checked: boolean) {
  if (!selected.value) return;
  emit('updateProperty', selected.value.id, 'isExecutable', checked);
}

function onTimer(value: string) {
  if (!selected.value) return;
  emit('updateProperty', selected.value.id, 'timeDuration', value);
}

function onMultiInstance(value: string) {
  if (!selected.value) return;
  emit('updateProperty', selected.value.id, 'loopCharacteristics', value);
  emit(
    'updateProperty',
    selected.value.id,
    'isSequential',
    value === 'sequential',
  );
}

function onLoopCardinality(value: string) {
  if (!selected.value) return;
  emit('updateProperty', selected.value.id, 'loopCardinality', value);
}

function onCompletion(value: string) {
  if (!selected.value) return;
  emit('updateProperty', selected.value.id, 'completionCondition', value);
}
</script>

<template>
  <aside class="vb-properties" aria-label="属性面板">
    <div v-if="placeholder === 'empty'" class="vb-properties-placeholder">
      未选中元素
    </div>
    <div v-else-if="placeholder === 'multiple'" class="vb-properties-placeholder">
      已选中多个元素
    </div>
    <template v-else-if="selected">
      <header class="vb-properties-header">
        <div class="vb-properties-type">{{ typeLabel }}</div>
        <div class="vb-properties-title">{{ selected.name || selected.id }}</div>
      </header>

      <section v-if="groups.includes('general')" class="vb-properties-group" data-group="general">
        <h3>常规</h3>
        <label v-if="generalEntries.includes('name')" class="vb-field">
          <span>名称</span>
          <input
            :value="selected.name ?? ''"
            @change="emit('updateName', selected.id, ($event.target as HTMLInputElement).value)"
          />
        </label>
        <label v-if="generalEntries.includes('id')" class="vb-field">
          <span>标识</span>
          <input
            :value="selected.id"
            @change="emit('updateId', selected.id, ($event.target as HTMLInputElement).value)"
          />
        </label>
        <label v-if="generalEntries.includes('isExecutable')" class="vb-field">
          <span>可执行</span>
          <input
            type="checkbox"
            :checked="selected.businessObject.isExecutable !== false"
            @change="onExecutable(($event.target as HTMLInputElement).checked)"
          />
        </label>
        <label v-if="generalEntries.includes('processId')" class="vb-field">
          <span>流程标识</span>
          <input :value="String(selected.businessObject.processRef ?? '')" disabled />
        </label>
        <label v-if="generalEntries.includes('processName')" class="vb-field">
          <span>流程名称</span>
          <input :value="processRef?.name ?? ''" disabled />
        </label>
      </section>

      <section
        v-if="groups.includes('documentation')"
        class="vb-properties-group"
        data-group="documentation"
      >
        <h3>文档</h3>
        <label class="vb-field">
          <span>文档说明</span>
          <textarea
            rows="4"
            :value="String(selected.businessObject.documentation ?? '')"
            @change="onDoc(($event.target as HTMLTextAreaElement).value)"
          />
        </label>
      </section>

      <section v-if="groups.includes('timer')" class="vb-properties-group" data-group="timer">
        <h3>定时器</h3>
        <label class="vb-field">
          <span>持续时间</span>
          <input
            :value="String(selected.businessObject.timeDuration ?? '')"
            @change="onTimer(($event.target as HTMLInputElement).value)"
          />
        </label>
      </section>

      <section
        v-if="groups.includes('multiInstance')"
        class="vb-properties-group"
        data-group="multiInstance"
      >
        <h3>多实例</h3>
        <label class="vb-field">
          <span>循环</span>
          <select
            :value="String(selected.businessObject.loopCharacteristics ?? '')"
            @change="onMultiInstance(($event.target as HTMLSelectElement).value)"
          >
            <option value="">无</option>
            <option value="parallel">并行</option>
            <option value="sequential">串行</option>
          </select>
        </label>
        <label class="vb-field">
          <span>基数</span>
          <input
            :value="String(selected.businessObject.loopCardinality ?? '')"
            @change="onLoopCardinality(($event.target as HTMLInputElement).value)"
          />
        </label>
      </section>

      <section
        v-if="groups.includes('adHocCompletion')"
        class="vb-properties-group"
        data-group="adHocCompletion"
      >
        <h3>即席</h3>
        <label class="vb-field">
          <span>完成条件</span>
          <input
            :value="String(selected.businessObject.completionCondition ?? '')"
            @change="onCompletion(($event.target as HTMLInputElement).value)"
          />
        </label>
      </section>

      <section
        v-if="groups.includes('message')"
        class="vb-properties-group"
        data-group="message"
      >
        <h3>消息</h3>
        <p class="vb-properties-type">消息事件定义</p>
      </section>
      <section v-if="groups.includes('error')" class="vb-properties-group" data-group="error">
        <h3>错误</h3>
        <p class="vb-properties-type">错误事件定义</p>
      </section>
      <section v-if="groups.includes('signal')" class="vb-properties-group" data-group="signal">
        <h3>信号</h3>
        <p class="vb-properties-type">信号事件定义</p>
      </section>
      <section
        v-if="groups.includes('escalation')"
        class="vb-properties-group"
        data-group="escalation"
      >
        <h3>升级</h3>
        <p class="vb-properties-type">升级事件定义</p>
      </section>
      <section v-if="groups.includes('link')" class="vb-properties-group" data-group="link">
        <h3>链接</h3>
        <p class="vb-properties-type">链接事件定义</p>
      </section>
      <section
        v-if="groups.includes('compensation')"
        class="vb-properties-group"
        data-group="compensation"
      >
        <h3>补偿</h3>
        <p class="vb-properties-type">补偿事件定义</p>
      </section>
    </template>
  </aside>
</template>
