import type { DiagramSnapshot } from '../types.js';
import { getReplaceOptions } from '../contextPad.js';

export interface ReplaceRuleContext {
  element: string;
  newType: string;
}

function isEventSubProcessStart(state: DiagramSnapshot, elementId: string): boolean {
  const el = state.elements[elementId];
  if (!el || el.type !== 'bpmn:StartEvent') return false;
  const parent = el.parentId ? state.elements[el.parentId] : undefined;
  return Boolean(parent?.businessObject.triggeredByEvent);
}

/**
 * Whether element may be replaced with newType (C11-005/006).
 * Event subprocess starts must keep a non-none event definition.
 */
export function canReplace(state: DiagramSnapshot, ctx: ReplaceRuleContext): boolean {
  const el = state.elements[ctx.element];
  if (!el) return false;

  // Connections cannot become flow nodes / shapes of unrelated kinds
  if (ctx.newType === 'bpmn:SequenceFlow' && !el.type.includes('SequenceFlow') && !el.type.includes('Flow')) {
    if (el.type !== 'bpmn:SequenceFlow' && el.type !== 'bpmn:MessageFlow' && el.type !== 'bpmn:Association') {
      return false;
    }
  }
  if (ctx.newType === 'bpmn:SequenceFlow' && el.type.includes('Task')) return false;
  if (ctx.newType === 'bpmn:SequenceFlow' && el.type.includes('Event')) return false;
  if (ctx.newType === 'bpmn:SequenceFlow' && el.type.includes('Gateway')) return false;

  if (isEventSubProcessStart(state, ctx.element)) {
    if (ctx.newType === 'bpmn:StartEvent' || ctx.newType === 'bpmn:StartEvent.none') {
      return false;
    }
    if (ctx.newType.startsWith('bpmn:StartEvent.')) return true;
  }

  const options = getReplaceOptions(el);
  if (options.some((o) => o.target === ctx.newType || o.key === ctx.newType)) return true;

  // Allow same-family event swaps (C11-005 StartEvent → EndEvent)
  const isEvent = (t: string) =>
    t.includes('Event') && !t.includes('EventDefinition') && !t.includes('EventBased');
  if (isEvent(el.type) && isEvent(ctx.newType.split('.')[0]!)) return true;

  if (el.type.includes('Task') && ctx.newType.includes('Task')) return true;
  if (el.type.includes('Gateway') && ctx.newType.includes('Gateway')) return true;

  return false;
}
