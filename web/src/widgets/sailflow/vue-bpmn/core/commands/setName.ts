import type { Command, CommandContext } from '../types.js';

export interface SetNamePayload {
  id: string;
  name: string;
}

export function createSetNameCommand(payload: SetNamePayload): Command {
  let previous = '';

  return {
    name: 'element.setName',
    execute(ctx: CommandContext) {
      const el = ctx.store.getState().elements[payload.id];
      if (!el) return;
      previous = el.name ?? '';
      ctx.store.patchState({
        elements: {
          ...ctx.store.getState().elements,
          [payload.id]: {
            ...el,
            name: payload.name,
            businessObject: { ...el.businessObject, name: payload.name },
          },
        },
      });
    },
    revert(ctx: CommandContext) {
      const el = ctx.store.getState().elements[payload.id];
      if (!el) return;
      ctx.store.patchState({
        elements: {
          ...ctx.store.getState().elements,
          [payload.id]: {
            ...el,
            name: previous,
            businessObject: { ...el.businessObject, name: previous },
          },
        },
      });
    },
  };
}
