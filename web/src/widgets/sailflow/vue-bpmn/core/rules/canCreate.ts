import type { DiagramElement, DiagramSnapshot } from '../types.js';

export interface CreateRuleContext {
  type: string;
  parent?: string;
}

function isContainer(el: DiagramElement): boolean {
  return (
    el.type === 'bpmn:Process' ||
    el.type === 'bpmn:SubProcess' ||
    el.type === 'bpmn:Transaction' ||
    el.type === 'bpmn:AdHocSubProcess' ||
    el.type === 'bpmn:Participant' ||
    el.type === 'bpmn:Lane'
  );
}

/** Whether a shape can be created under the given parent (C11-001). */
export function canCreate(state: DiagramSnapshot, ctx: CreateRuleContext): boolean {
  const parentId = ctx.parent ?? state.rootId;
  if (!parentId) return false;
  const parent =
    state.elements[parentId] ??
    (parentId === state.rootId
      ? ({
          id: parentId,
          type: 'bpmn:Process',
          parentId: null,
          businessObject: {},
          di: { x: 0, y: 0, width: 0, height: 0, isExpanded: null },
          incoming: [],
          outgoing: [],
        } as DiagramElement)
      : null);
  if (!parent) return false;
  if (!isContainer(parent) && parent.type !== 'bpmn:Collaboration') return false;
  if (ctx.type === 'bpmn:Participant' && parent.type === 'bpmn:SubProcess') return false;
  if (ctx.type === 'bpmn:Lane' && parent.type !== 'bpmn:Participant' && parent.type !== 'bpmn:Process') {
    return false;
  }
  return true;
}
