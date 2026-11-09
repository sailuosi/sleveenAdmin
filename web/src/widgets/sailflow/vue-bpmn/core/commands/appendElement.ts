import type { Command, CommandContext, DiagramSnapshot, Point } from '../types.js';
import { buildCreateElements } from './createElements.js';
import { buildConnection } from './connectElements.js';

export interface AppendElementPayload {
  sourceId: string;
  type: string;
  /** Offset from source; default to the right. */
  offset?: Point;
}

/**
 * Context-pad style append: create shape + sequence/message/association connection (C05-007).
 */
export function createAppendElementCommand(payload: AppendElementPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'elements.append',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const source = before.elements[payload.sourceId];
        if (!source) return;
        const offset = payload.offset ?? { x: 150, y: 0 };
        const position = {
          x: source.di.x + source.di.width / 2 + offset.x,
          y: source.di.y + source.di.height / 2 + offset.y,
        };
        const created = buildCreateElements(before, {
          type: payload.type,
          position,
          parentId: source.parentId,
        });
        const mid: DiagramSnapshot = {
          ...before,
          rootId: created.rootId,
          elements: created.elements,
          selection: created.selection,
        };
        const newId = created.selection[0];
        if (!newId) {
          after = mid;
        } else {
          const linked = buildConnection(mid, { sourceId: payload.sourceId, targetId: newId });
          after = linked.ok
            ? { ...linked.state, selection: [newId] }
            : { ...mid, selection: [newId] };
        }
      }
      ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
