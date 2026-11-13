import type { Command, CommandContext, DiagramElement, DiagramSnapshot } from '../types.js';
import { nextId } from '../ids.js';
import { canConnect } from '../rules/canConnect.js';
import { dockAnchorsBetween } from '../docking.js';

export interface ConnectElementsPayload {
  sourceId: string;
  targetId: string;
}

function findExisting(
  state: DiagramSnapshot,
  sourceId: string,
  targetId: string,
): DiagramElement | undefined {
  return Object.values(state.elements).find(
    (el) =>
      (el.type.includes('Flow') || el.type.includes('Association')) &&
      el.businessObject.sourceRef === sourceId &&
      el.businessObject.targetRef === targetId,
  );
}

export function buildConnection(
  state: DiagramSnapshot,
  payload: ConnectElementsPayload,
): { ok: false; reason: string } | { ok: true; state: DiagramSnapshot; connectionId: string } {
  const decision = canConnect(state, payload.sourceId, payload.targetId);
  if (!decision.allowed || !decision.type) {
    return { ok: false, reason: decision.reason ?? 'rejected' };
  }

  const existing = findExisting(state, payload.sourceId, payload.targetId);
  if (existing) {
    return { ok: true, state, connectionId: existing.id };
  }

  const source = state.elements[payload.sourceId]!;
  const target = state.elements[payload.targetId]!;
  const parentId =
    decision.type === 'bpmn:MessageFlow'
      ? state.rootId
      : source.parentId ?? state.rootId;

  const prefix =
    decision.type === 'bpmn:MessageFlow'
      ? 'MessageFlow'
      : decision.type === 'bpmn:Association'
        ? 'Association'
        : 'Flow';
  const id = nextId(state, prefix);
  const waypoints = [...dockAnchorsBetween(source, target)];

  const connection: DiagramElement = {
    id,
    type: decision.type,
    parentId,
    name: undefined,
    businessObject: {
      sourceRef: payload.sourceId,
      targetRef: payload.targetId,
    },
    di: { x: waypoints[0]!.x, y: waypoints[0]!.y, width: 0, height: 0, isExpanded: null },
    hostId: null,
    incoming: [],
    outgoing: [],
    waypoints,
    label: { kind: 'none' },
    children: [],
  };

  const elements = { ...state.elements, [id]: connection };
  elements[payload.sourceId] = {
    ...source,
    outgoing: [...new Set([...source.outgoing, id])],
  };
  elements[payload.targetId] = {
    ...target,
    incoming: [...new Set([...target.incoming, id])],
  };

  return {
    ok: true,
    connectionId: id,
    state: {
      ...state,
      elements,
      selection: [id],
    },
  };
}

export function createConnectElementsCommand(payload: ConnectElementsPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;
  let rejected = false;

  return {
    name: 'connection.create',
    execute(ctx: CommandContext) {
      if (rejected) return;
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const built = buildConnection(before, payload);
        if (!built.ok) {
          rejected = true;
          return;
        }
        after = structuredClone(built.state);
      }
      ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (rejected || !before) return;
      ctx.store.replaceState(structuredClone(before));
    },
  };
}

/** Non-command helper used by tests / UI preview commit. */
export function tryConnect(
  state: DiagramSnapshot,
  sourceId: string,
  targetId: string,
): ReturnType<typeof buildConnection> {
  return buildConnection(state, { sourceId, targetId });
}
