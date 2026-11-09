import { ElMessage } from 'element-plus';
import type { DiagramStore } from '@vue-bpmn/core';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import {
  createVueModdleBridge,
  createVueModelingBridge,
  wrapDiagramElement,
} from './vueModdle';

export type SailConfigKind =
  | 'node'
  | 'transition'
  | 'gateway'
  | 'service'
  | 'script'
  | 'rule'
  | 'subprocess'
  | 'event'
  | 'basic';

/** Route by bpmn type string — no bpmn-js `is()`. */
export function resolveSailConfigKind(
  type: string,
  eventDefinitions?: unknown,
): SailConfigKind {
  const t = String(type || '');
  if (t === 'bpmn:SequenceFlow') return 'transition';
  if (t === 'bpmn:UserTask' || t === 'bpmn:Task') return 'node';
  if (t.includes('Gateway')) return 'gateway';
  if (t === 'bpmn:ScriptTask') return 'script';
  if (t === 'bpmn:BusinessRuleTask') return 'rule';
  if (t === 'bpmn:ServiceTask') return 'service';
  if (t === 'bpmn:SubProcess' || t === 'bpmn:CallActivity') return 'subprocess';
  if (
    t === 'bpmn:StartEvent' ||
    t === 'bpmn:EndEvent' ||
    t === 'bpmn:IntermediateCatchEvent' ||
    t === 'bpmn:IntermediateThrowEvent' ||
    t === 'bpmn:BoundaryEvent'
  ) {
    const defs = Array.isArray(eventDefinitions) ? eventDefinitions : [];
    if (defs.length > 0) return 'event';
    return 'basic';
  }
  return 'basic';
}

const EVENT_BY_KIND: Record<
  Exclude<SailConfigKind, 'basic'>,
  | 'nodeconfig:open'
  | 'transitionconfig:open'
  | 'gatewayconfig:open'
  | 'servicetaskconfig:open'
  | 'scripttaskconfig:open'
  | 'ruletaskconfig:open'
  | 'subprocessconfig:open'
  | 'eventconfig:open'
> = {
  node: 'nodeconfig:open',
  transition: 'transitionconfig:open',
  gateway: 'gatewayconfig:open',
  service: 'servicetaskconfig:open',
  script: 'scripttaskconfig:open',
  rule: 'ruletaskconfig:open',
  subprocess: 'subprocessconfig:open',
  event: 'eventconfig:open',
};

/**
 * Open Sail config dialog for a diagram element (Vue engine path).
 */
export function openSailConfig(store: DiagramStore, elementId: string): boolean {
  const de = store.getElement(elementId);
  if (!de) {
    ElMessage.warning('未找到选中元素');
    return false;
  }
  const view = wrapDiagramElement(store, elementId, { withFlowEnds: true });
  if (!view) return false;
  const modeling = createVueModelingBridge(store);
  const moddle = createVueModdleBridge();
  const kind = resolveSailConfigKind(de.type, de.businessObject.eventDefinitions);
  if (kind === 'basic') {
    designerEvents.emit('basicconfig:open', { element: view, modeling, moddle });
    return true;
  }
  designerEvents.emit(EVENT_BY_KIND[kind], { element: view, modeling, moddle });
  return true;
}
