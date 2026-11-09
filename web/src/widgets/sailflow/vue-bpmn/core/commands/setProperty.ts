import type { Command, CommandContext, DiagramSnapshot } from '../types.js';

export interface SetPropertyPayload {
  id: string;
  key: string;
  value: unknown;
}

export function createSetPropertyCommand(payload: SetPropertyPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'element.setProperty',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const el = before.elements[payload.id];
        if (!el) return;
        const next = {
          ...el,
          businessObject: { ...el.businessObject, [payload.key]: payload.value },
        };
        if (payload.key === 'name') next.name = String(payload.value ?? '');
        if (payload.key === 'documentation') {
          next.businessObject.documentation = payload.value;
        }
        after = {
          ...before,
          elements: { ...before.elements, [payload.id]: next },
        };
      }
      if (after) ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
