import type { Command, CommandContext, DiagramSnapshot, Point } from '../types.js';
import { isConnection } from '../xml/elementKind.js';

export interface SpaceToolPayload {
  /** Axis of space insertion. */
  axis: 'x' | 'y';
  /** World coordinate of the cut line. */
  cut: number;
  /** Amount of space to insert (positive expands). */
  amount: number;
}

export function buildSpaceTool(state: DiagramSnapshot, payload: SpaceToolPayload): DiagramSnapshot {
  const elements = { ...state.elements };
  const { axis, cut, amount } = payload;

  for (const [id, el] of Object.entries(elements)) {
    if (isConnection(el) && el.waypoints?.length) {
      const wps = el.waypoints.map((p) => {
        const v = axis === 'x' ? p.x : p.y;
        if (v >= cut) {
          return axis === 'x' ? { x: p.x + amount, y: p.y } : { x: p.x, y: p.y + amount };
        }
        return p;
      });
      elements[id] = { ...el, waypoints: wps };
      continue;
    }
    const origin = axis === 'x' ? el.di.x : el.di.y;
    if (origin >= cut) {
      elements[id] = {
        ...el,
        di: {
          ...el.di,
          x: axis === 'x' ? el.di.x + amount : el.di.x,
          y: axis === 'y' ? el.di.y + amount : el.di.y,
        },
        label:
          el.label?.kind === 'external' && el.label.bounds
            ? {
                ...el.label,
                bounds: {
                  ...el.label.bounds,
                  x: axis === 'x' ? el.label.bounds.x + amount : el.label.bounds.x,
                  y: axis === 'y' ? el.label.bounds.y + amount : el.label.bounds.y,
                },
              }
            : el.label,
      };
    }
  }

  return { ...state, elements };
}

export function createSpaceToolCommand(payload: SpaceToolPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'tool.space',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        after = structuredClone(buildSpaceTool(before, payload));
      }
      if (after) ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}

/** Convenience: horizontal drag from→to becomes space insert. */
export function spaceFromDrag(from: Point, to: Point): SpaceToolPayload {
  const dx = to.x - from.x;
  const dy = to.y - from.y;
  if (Math.abs(dx) >= Math.abs(dy)) {
    return { axis: 'x', cut: from.x, amount: dx };
  }
  return { axis: 'y', cut: from.y, amount: dy };
}
