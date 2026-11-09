import type { DiagramElement, DiagramSnapshot } from '../types.js';

export type ConnectionType = 'bpmn:SequenceFlow' | 'bpmn:MessageFlow' | 'bpmn:Association';

export interface ConnectDecision {
  allowed: boolean;
  type?: ConnectionType;
  reason?: string;
}

function processScopeId(state: DiagramSnapshot, el: DiagramElement): string | null {
  let cur: DiagramElement | undefined = el;
  const seen = new Set<string>();
  while (cur) {
    if (seen.has(cur.id)) break;
    seen.add(cur.id);
    if (cur.type === 'bpmn:Process') return cur.id;
    if (cur.parentId && state.elements[cur.parentId]) {
      cur = state.elements[cur.parentId];
      continue;
    }
    // element parented directly under collaboration root via process membership
    if (cur.parentId) return cur.parentId;
    break;
  }
  // collaboration: map via participant processRef ownership
  for (const p of Object.values(state.elements)) {
    if (p.type !== 'bpmn:Participant') continue;
    const processRef = p.businessObject.processRef as string | undefined;
    if (!processRef) continue;
    // if el's parent chain equals processRef
    let walk: DiagramElement | undefined = el;
    while (walk) {
      if (walk.id === processRef || walk.parentId === processRef) return processRef;
      walk = walk.parentId ? state.elements[walk.parentId] : undefined;
    }
  }
  return el.parentId;
}

function participantOf(state: DiagramSnapshot, el: DiagramElement): string | null {
  const scope = processScopeId(state, el);
  if (!scope) return null;
  for (const p of Object.values(state.elements)) {
    if (p.type === 'bpmn:Participant' && p.businessObject.processRef === scope) return p.id;
  }
  return null;
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

function hasCompensateDef(el: DiagramElement): boolean {
  const defs = (el.businessObject.eventDefinitions as string[] | undefined) ?? [];
  return defs.some((d) => d.includes('Compensate'));
}

export interface ConnectOptions {
  /** Hint for association kind (C11-007). */
  association?: 'compensation' | 'association';
}

/**
 * Decide whether source→target can connect and which BPMN connection type.
 */
export function canConnect(
  state: DiagramSnapshot,
  sourceId: string,
  targetId: string,
  options: ConnectOptions = {},
): ConnectDecision {
  if (sourceId === targetId) {
    return { allowed: false, reason: 'same-element' };
  }
  const source = state.elements[sourceId];
  const target = state.elements[targetId];
  if (!source || !target) return { allowed: false, reason: 'missing' };

  // Association involving text annotation
  if (source.type === 'bpmn:TextAnnotation' || target.type === 'bpmn:TextAnnotation') {
    return { allowed: true, type: 'bpmn:Association' };
  }

  const targetCompensation = target.businessObject.isForCompensation === true;
  const sourceCompensate = hasCompensateDef(source);

  if (options.association === 'compensation' || (sourceCompensate && targetCompensation)) {
    if (sourceCompensate && targetCompensation) {
      return { allowed: true, type: 'bpmn:Association' };
    }
    return { allowed: false, reason: 'compensation-association-invalid' };
  }

  if (targetCompensation && !sourceCompensate) {
    return { allowed: false, reason: 'compensation-target' };
  }

  const sourcePool = participantOf(state, source);
  const targetPool = participantOf(state, target);
  if (sourcePool && targetPool && sourcePool !== targetPool) {
    if (isFlowNode(source) && isFlowNode(target)) {
      return { allowed: true, type: 'bpmn:MessageFlow' };
    }
    return { allowed: false, reason: 'cross-pool-invalid' };
  }

  // Sequence flow within same process/scope
  if (source.type === 'bpmn:EndEvent') {
    return { allowed: false, reason: 'end-event-source' };
  }
  if (target.type === 'bpmn:StartEvent' && !target.hostId) {
    // attaching to none-start inside same process is typically rejected for sequence from mid-flow;
    // allow from nothing→start only via create; reject connecting INTO start
    return { allowed: false, reason: 'start-event-target' };
  }
  if (!isFlowNode(source) || !isFlowNode(target)) {
    return { allowed: false, reason: 'not-flow-nodes' };
  }

  const sourceScope = processScopeId(state, source);
  const targetScope = processScopeId(state, target);
  if (sourceScope && targetScope && sourceScope !== targetScope) {
    return { allowed: false, reason: 'different-process' };
  }

  return { allowed: true, type: 'bpmn:SequenceFlow' };
}
