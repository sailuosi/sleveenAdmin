import type { DiagramElement } from './types.js';

export type EventMarkerKind =
  | 'none'
  | 'message'
  | 'timer'
  | 'signal'
  | 'error'
  | 'escalation'
  | 'link'
  | 'conditional'
  | 'compensate'
  | 'terminate'
  | 'cancel';

/** Derive visual event marker kind from imported eventDefinitions (C02-001/002). */
export function eventMarker(el: DiagramElement): EventMarkerKind {
  const defs = (el.businessObject.eventDefinitions as string[] | undefined) ?? [];
  if (!defs.length) return 'none';
  const d = defs[0]!;
  if (d.includes('Message')) return 'message';
  if (d.includes('Timer')) return 'timer';
  if (d.includes('Signal')) return 'signal';
  if (d.includes('Error')) return 'error';
  if (d.includes('Escalation')) return 'escalation';
  if (d.includes('Link')) return 'link';
  if (d.includes('Conditional')) return 'conditional';
  if (d.includes('Compensate')) return 'compensate';
  if (d.includes('Terminate')) return 'terminate';
  if (d.includes('Cancel')) return 'cancel';
  return 'none';
}

export function isNonInterrupting(el: DiagramElement): boolean {
  return el.businessObject.cancelActivity === 'false' || el.businessObject.cancelActivity === false;
}
