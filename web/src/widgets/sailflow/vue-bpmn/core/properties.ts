import type { DiagramElement, DiagramSnapshot } from './types.js';

export type PropertyGroupId =
  | 'general'
  | 'documentation'
  | 'message'
  | 'timer'
  | 'error'
  | 'signal'
  | 'escalation'
  | 'link'
  | 'compensation'
  | 'multiInstance'
  | 'adHocCompletion';

export function getPropertyGroups(
  state: DiagramSnapshot,
  elementId?: string,
): PropertyGroupId[] {
  const id = elementId ?? state.selection[0];
  if (!id) return [];
  const el = state.elements[id] ?? (id === state.rootId ? syntheticRoot(state) : null);
  if (!el) return [];

  const groups: PropertyGroupId[] = ['general', 'documentation'];
  const defs = (el.businessObject.eventDefinitions as string[] | undefined) ?? [];

  if (defs.some((d) => d.includes('Message'))) groups.push('message');
  if (defs.some((d) => d.includes('Timer'))) groups.push('timer');
  if (defs.some((d) => d.includes('Error'))) groups.push('error');
  if (defs.some((d) => d.includes('Signal'))) groups.push('signal');
  if (defs.some((d) => d.includes('Escalation'))) groups.push('escalation');
  if (defs.some((d) => d.includes('Link'))) groups.push('link');
  if (defs.some((d) => d.includes('Compensate'))) groups.push('compensation');
  if (el.type.includes('Task') || el.type.includes('SubProcess') || el.type === 'bpmn:CallActivity') {
    groups.push('multiInstance');
  }
  if (el.type === 'bpmn:AdHocSubProcess') groups.push('adHocCompletion');
  if (el.id.toLowerCase().includes('compensate') || el.type.includes('Compensation')) {
    if (!groups.includes('compensation')) groups.push('compensation');
  }
  return groups;
}

function syntheticRoot(state: DiagramSnapshot): DiagramElement | null {
  if (!state.rootId) return null;
  return {
    id: state.rootId,
    type: 'bpmn:Process',
    parentId: null,
    businessObject: { isExecutable: true },
    di: { x: 0, y: 0, width: 0, height: 0, isExpanded: null },
    incoming: [],
    outgoing: [],
  };
}

export function getGeneralEntries(el: DiagramElement): string[] {
  const entries = ['name', 'id'];
  if (el.type === 'bpmn:Process') entries.push('isExecutable');
  if (el.type === 'bpmn:Participant') entries.push('processId', 'processName');
  return entries;
}
