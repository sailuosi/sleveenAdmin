import type { Bounds } from '../types.js';

export function defaultBoundsFor(type: string, position: { x: number; y: number }): Bounds {
  const size = defaultSize(type);
  return {
    x: position.x - size.width / 2,
    y: position.y - size.height / 2,
    width: size.width,
    height: size.height,
  };
}

export function defaultSize(type: string): { width: number; height: number } {
  if (type.includes('Event')) return { width: 36, height: 36 };
  if (type.includes('Gateway')) return { width: 50, height: 50 };
  if (type === 'bpmn:SubProcess' || type === 'bpmn:Transaction' || type === 'bpmn:AdHocSubProcess') {
    return { width: 350, height: 200 };
  }
  if (type === 'bpmn:Participant') return { width: 600, height: 250 };
  if (type === 'bpmn:Group') return { width: 300, height: 200 };
  if (type === 'bpmn:DataObjectReference') return { width: 36, height: 50 };
  if (type === 'bpmn:DataStoreReference') return { width: 50, height: 50 };
  return { width: 100, height: 80 };
}

export function idPrefixFor(type: string): string {
  const map: Record<string, string> = {
    'bpmn:StartEvent': 'StartEvent',
    'bpmn:EndEvent': 'EndEvent',
    'bpmn:IntermediateThrowEvent': 'IntermediateThrowEvent',
    'bpmn:IntermediateCatchEvent': 'IntermediateCatchEvent',
    'bpmn:ExclusiveGateway': 'Gateway',
    'bpmn:Task': 'Task',
    'bpmn:SubProcess': 'SubProcess',
    'bpmn:DataObjectReference': 'DataObjectReference',
    'bpmn:DataStoreReference': 'DataStoreReference',
    'bpmn:Participant': 'Participant',
    'bpmn:Group': 'Group',
    'bpmn:Process': 'Process',
    'bpmn:Collaboration': 'Collaboration',
  };
  return map[type] ?? (type.replace(/^bpmn:/, '') || 'Element');
}
