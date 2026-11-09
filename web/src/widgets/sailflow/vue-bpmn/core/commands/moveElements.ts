import type { Command, CommandContext, DiagramElement, Point } from '../types.js';
import { isConnection } from '../xml/elementKind.js';

export interface MoveElementsPayload {
  ids: string[];
  delta: Point;
}

function moveElement(el: DiagramElement, delta: Point): DiagramElement {
  const next: DiagramElement = {
    ...el,
    di: {
      ...el.di,
      x: el.di.x + delta.x,
      y: el.di.y + delta.y,
    },
  };
  if (el.label?.kind === 'external' && el.label.bounds) {
    next.label = {
      ...el.label,
      bounds: {
        ...el.label.bounds,
        x: el.label.bounds.x + delta.x,
        y: el.label.bounds.y + delta.y,
      },
    };
  }
  if (el.waypoints?.length) {
    next.waypoints = el.waypoints.map((p) => ({ x: p.x + delta.x, y: p.y + delta.y }));
  }
  return next;
}

export function createMoveElementsCommand(payload: MoveElementsPayload): Command {
  return {
    name: 'elements.move',
    execute(ctx: CommandContext) {
      const state = ctx.store.getState();
      const elements = { ...state.elements };
      const moved = new Set(payload.ids);

      for (const id of payload.ids) {
        const el = elements[id];
        if (!el) continue;
        elements[id] = moveElement(el, payload.delta);
      }

      // Keep attached edges in sync when endpoints move
      for (const el of Object.values(elements)) {
        if (!isConnection(el) || !el.waypoints?.length) continue;
        if (moved.has(el.id)) continue;
        const sourceRef = el.businessObject.sourceRef as string | undefined;
        const targetRef = el.businessObject.targetRef as string | undefined;
        const moveSource = sourceRef && moved.has(sourceRef);
        const moveTarget = targetRef && moved.has(targetRef);
        if (!moveSource && !moveTarget) continue;
        if (moveSource && moveTarget) {
          elements[el.id] = moveElement(el, payload.delta);
          continue;
        }
        const wps = [...(el.waypoints ?? [])];
        if (moveSource && wps[0]) {
          wps[0] = { x: wps[0].x + payload.delta.x, y: wps[0].y + payload.delta.y };
        }
        if (moveTarget && wps.length) {
          const last = wps.length - 1;
          wps[last] = {
            x: wps[last]!.x + payload.delta.x,
            y: wps[last]!.y + payload.delta.y,
          };
        }
        elements[el.id] = { ...el, waypoints: wps };
      }

      ctx.store.patchState({ elements });
    },
    revert(ctx: CommandContext) {
      const inverse = {
        ids: payload.ids,
        delta: { x: -payload.delta.x, y: -payload.delta.y },
      };
      createMoveElementsCommand(inverse).execute(ctx);
    },
  };
}
