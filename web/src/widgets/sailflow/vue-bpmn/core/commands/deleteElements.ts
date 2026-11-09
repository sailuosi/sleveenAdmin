import type { Command, CommandContext, DiagramSnapshot } from '../types.js';
import { isConnection, isContainerActivity } from '../xml/elementKind.js';

export interface DeleteElementsPayload {
  ids: string[];
}

function collectRemovals(state: DiagramSnapshot, seedIds: string[]): Set<string> {
  const remove = new Set<string>();
  const visit = (id: string) => {
    if (remove.has(id) || !state.elements[id]) return;
    if (id === state.rootId) return;
    remove.add(id);
    const el = state.elements[id]!;
    if (isContainerActivity(el)) {
      for (const child of Object.values(state.elements)) {
        if (child.parentId === id) visit(child.id);
      }
    }
  };
  for (const id of seedIds) visit(id);

  // attached connections
  for (const el of Object.values(state.elements)) {
    if (!isConnection(el)) continue;
    const source = el.businessObject.sourceRef as string | undefined;
    const target = el.businessObject.targetRef as string | undefined;
    if ((source && remove.has(source)) || (target && remove.has(target)) || remove.has(el.id)) {
      remove.add(el.id);
    }
  }
  return remove;
}

export function applyDelete(state: DiagramSnapshot, ids: string[]): DiagramSnapshot {
  const remove = collectRemovals(state, ids);
  const elements = { ...state.elements };
  for (const id of remove) delete elements[id];

  // scrub refs on survivors
  for (const el of Object.values(elements)) {
    el.incoming = el.incoming.filter((x) => !remove.has(x));
    el.outgoing = el.outgoing.filter((x) => !remove.has(x));
  }

  return {
    ...state,
    elements,
    selection: state.selection.filter((id) => !remove.has(id)),
  };
}

export function createDeleteElementsCommand(payload: DeleteElementsPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'elements.delete',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        after = applyDelete(before, payload.ids);
      }
      ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
