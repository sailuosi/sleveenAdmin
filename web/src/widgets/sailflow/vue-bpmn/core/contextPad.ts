import type { DiagramElement, DiagramSnapshot } from './types.js';
import { isConnection } from './xml/elementKind.js';

export interface ContextPadEntry {
  key: string;
  label: string;
  action: 'append' | 'connect' | 'replace' | 'delete' | 'lane';
  type?: string;
}

function isFlowNode(el: DiagramElement): boolean {
  return (
    el.type.includes('Event') ||
    el.type.includes('Task') ||
    el.type.includes('Gateway') ||
    el.type.includes('SubProcess') ||
    el.type.includes('Activity') ||
    el.type === 'bpmn:Transaction'
  );
}

/**
 * Context pad entries for the current selection (C06-001..003).
 */
export function getContextPadEntries(
  state: DiagramSnapshot,
  elementId?: string,
): ContextPadEntry[] {
  const id = elementId ?? state.selection[0];
  if (!id) return [];
  const el = state.elements[id];
  if (!el) return [];

  if (el.type === 'bpmn:Lane') {
    return [
      { key: 'lane-insert-above', label: '上方插入', action: 'lane' },
      { key: 'lane-divide-two', label: '分成两道', action: 'lane' },
      { key: 'lane-divide-three', label: '分成三道', action: 'lane' },
      { key: 'lane-insert-below', label: '下方插入', action: 'lane' },
      { key: 'delete', label: '删除', action: 'delete' },
    ];
  }

  if (el.type === 'bpmn:Participant') {
    return [
      { key: 'lane-insert-above', label: '上方插入', action: 'lane' },
      { key: 'lane-divide-two', label: '分成两道', action: 'lane' },
      { key: 'lane-divide-three', label: '分成三道', action: 'lane' },
      { key: 'lane-insert-below', label: '下方插入', action: 'lane' },
      { key: 'connect', label: '连线', action: 'connect' },
      { key: 'replace', label: '替换', action: 'replace' },
      { key: 'delete', label: '删除', action: 'delete' },
    ];
  }

  if (isConnection(el)) {
    return [
      { key: 'replace', label: '替换', action: 'replace' },
      { key: 'delete', label: '删除', action: 'delete' },
    ];
  }

  if (el.type === 'bpmn:EventBasedGateway') {
    return [
      {
        key: 'append.receive-task',
        label: '接收任务',
        action: 'append',
        type: 'bpmn:ReceiveTask',
      },
      {
        key: 'append.message-intermediate-event',
        label: '消息',
        action: 'append',
        type: 'bpmn:IntermediateCatchEvent',
      },
      {
        key: 'append.timer-intermediate-event',
        label: '定时',
        action: 'append',
        type: 'bpmn:IntermediateCatchEvent',
      },
      {
        key: 'append.condition-intermediate-event',
        label: '条件',
        action: 'append',
        type: 'bpmn:IntermediateCatchEvent',
      },
      {
        key: 'append.signal-intermediate-event',
        label: '信号',
        action: 'append',
        type: 'bpmn:IntermediateCatchEvent',
      },
      { key: 'connect', label: '连线', action: 'connect' },
      { key: 'replace', label: '替换', action: 'replace' },
      { key: 'delete', label: '删除', action: 'delete' },
    ];
  }

  if (isFlowNode(el) && el.type !== 'bpmn:EndEvent') {
    return [
      { key: 'append.end-event', label: '结束', action: 'append', type: 'bpmn:EndEvent' },
      {
        key: 'append.gateway',
        label: '网关',
        action: 'append',
        type: 'bpmn:ExclusiveGateway',
      },
      { key: 'append.append-task', label: '任务', action: 'append', type: 'bpmn:Task' },
      {
        key: 'append.intermediate-event',
        label: '中间事件',
        action: 'append',
        type: 'bpmn:IntermediateThrowEvent',
      },
      { key: 'connect', label: '连线', action: 'connect' },
      { key: 'replace', label: '替换', action: 'replace' },
      { key: 'delete', label: '删除', action: 'delete' },
    ];
  }

  if (el.type === 'bpmn:EndEvent') {
    return [
      { key: 'replace', label: '替换', action: 'replace' },
      { key: 'delete', label: '删除', action: 'delete' },
    ];
  }

  return [
    { key: 'replace', label: '替换', action: 'replace' },
    { key: 'delete', label: '删除', action: 'delete' },
  ];
}

export interface ReplaceOption {
  key: string;
  label: string;
  target: string;
}

/** Replace menu options for an element (C06-004..008). */
export function getReplaceOptions(el: DiagramElement): ReplaceOption[] {
  if (el.type.includes('Task') || el.type === 'bpmn:CallActivity') {
    return [
      { key: 'bpmn:Task', label: '任务', target: 'bpmn:Task' },
      { key: 'bpmn:UserTask', label: '用户任务', target: 'bpmn:UserTask' },
      { key: 'bpmn:ServiceTask', label: '服务任务', target: 'bpmn:ServiceTask' },
      { key: 'bpmn:ScriptTask', label: '脚本任务', target: 'bpmn:ScriptTask' },
      { key: 'bpmn:SendTask', label: '发送任务', target: 'bpmn:SendTask' },
      { key: 'bpmn:ReceiveTask', label: '接收任务', target: 'bpmn:ReceiveTask' },
      { key: 'bpmn:ManualTask', label: '手工任务', target: 'bpmn:ManualTask' },
      { key: 'bpmn:BusinessRuleTask', label: '业务规则', target: 'bpmn:BusinessRuleTask' },
    ];
  }
  if (el.type.includes('Gateway')) {
    return [
      { key: 'bpmn:ExclusiveGateway', label: '排他', target: 'bpmn:ExclusiveGateway' },
      { key: 'bpmn:ParallelGateway', label: '并行', target: 'bpmn:ParallelGateway' },
      { key: 'bpmn:InclusiveGateway', label: '包容', target: 'bpmn:InclusiveGateway' },
      { key: 'bpmn:EventBasedGateway', label: '事件网关', target: 'bpmn:EventBasedGateway' },
      { key: 'bpmn:ComplexGateway', label: '复杂', target: 'bpmn:ComplexGateway' },
    ];
  }
  if (el.type === 'bpmn:StartEvent') {
    return [
      { key: 'bpmn:StartEvent', label: '无', target: 'bpmn:StartEvent' },
      { key: 'bpmn:StartEvent.message', label: '消息', target: 'bpmn:StartEvent.message' },
      { key: 'bpmn:StartEvent.timer', label: '定时', target: 'bpmn:StartEvent.timer' },
      { key: 'bpmn:StartEvent.signal', label: '信号', target: 'bpmn:StartEvent.signal' },
      {
        key: 'bpmn:StartEvent.conditional',
        label: '条件',
        target: 'bpmn:StartEvent.conditional',
      },
    ];
  }
  if (el.type === 'bpmn:EndEvent') {
    return [
      { key: 'bpmn:EndEvent', label: '无', target: 'bpmn:EndEvent' },
      { key: 'bpmn:EndEvent.message', label: '消息', target: 'bpmn:EndEvent.message' },
      { key: 'bpmn:EndEvent.error', label: '错误', target: 'bpmn:EndEvent.error' },
      { key: 'bpmn:EndEvent.terminate', label: '终止', target: 'bpmn:EndEvent.terminate' },
    ];
  }
  if (
    el.type === 'bpmn:SubProcess' ||
    el.type === 'bpmn:Transaction' ||
    el.type === 'bpmn:AdHocSubProcess'
  ) {
    return [
      { key: 'bpmn:SubProcess', label: '子流程', target: 'bpmn:SubProcess' },
      { key: 'bpmn:Transaction', label: '事务', target: 'bpmn:Transaction' },
      { key: 'bpmn:AdHocSubProcess', label: '即席', target: 'bpmn:AdHocSubProcess' },
    ];
  }
  if (el.type === 'bpmn:SequenceFlow') {
    return [
      { key: 'bpmn:SequenceFlow', label: '顺序流', target: 'bpmn:SequenceFlow' },
      {
        key: 'bpmn:SequenceFlow.default',
        label: '默认流',
        target: 'bpmn:SequenceFlow.default',
      },
      {
        key: 'bpmn:SequenceFlow.conditional',
        label: '条件流',
        target: 'bpmn:SequenceFlow.conditional',
      },
    ];
  }
  return [];
}
