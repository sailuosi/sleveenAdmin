import type { DiagramSnapshot } from '../types.js';

export interface AttachRuleContext {
  type: string;
  host: string;
}

/** Whether a boundary event may attach to the given host (C11-002/003). */
export function canAttach(state: DiagramSnapshot, ctx: AttachRuleContext): boolean {
  if (!ctx.type.includes('BoundaryEvent')) return false;
  const host = state.elements[ctx.host];
  if (!host) return false;
  if (host.type.includes('Gateway')) return false;
  if (host.type.includes('Event') && !host.type.includes('SubProcess')) return false;
  return (
    host.type.includes('Task') ||
    host.type.includes('SubProcess') ||
    host.type === 'bpmn:CallActivity' ||
    host.type === 'bpmn:Transaction' ||
    host.type === 'bpmn:AdHocSubProcess'
  );
}
