import type { Command, CommandContext, DiagramSnapshot, Point } from '../types.js';

export interface UpdateWaypointsPayload {
  connectionId: string;
  waypoints: Point[];
}

export function createUpdateWaypointsCommand(payload: UpdateWaypointsPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'connection.updateWaypoints',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const el = before.elements[payload.connectionId];
        if (!el) return;
        after = {
          ...before,
          elements: {
            ...before.elements,
            [payload.connectionId]: {
              ...el,
              waypoints: payload.waypoints.map((p) => ({ ...p })),
              di: {
                ...el.di,
                x: payload.waypoints[0]?.x ?? el.di.x,
                y: payload.waypoints[0]?.y ?? el.di.y,
              },
            },
          },
        };
      }
      ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}

export function addBendpoint(
  waypoints: Point[],
  point: Point,
): { waypoints: Point[]; index: number } {
  if (waypoints.length < 2) {
    return { waypoints: [...waypoints, point], index: waypoints.length };
  }
  // insert after first segment mid if near, else before last
  let bestIdx = 1;
  let bestDist = Infinity;
  for (let i = 0; i < waypoints.length - 1; i++) {
    const a = waypoints[i]!;
    const b = waypoints[i + 1]!;
    const mx = (a.x + b.x) / 2;
    const my = (a.y + b.y) / 2;
    const d = (mx - point.x) ** 2 + (my - point.y) ** 2;
    if (d < bestDist) {
      bestDist = d;
      bestIdx = i + 1;
    }
  }
  const next = [...waypoints.slice(0, bestIdx), point, ...waypoints.slice(bestIdx)];
  return { waypoints: next, index: bestIdx };
}

export function removeBendpoint(waypoints: Point[], index: number): Point[] {
  if (waypoints.length <= 2) return waypoints.map((p) => ({ ...p }));
  if (index <= 0 || index >= waypoints.length - 1) return waypoints.map((p) => ({ ...p }));
  return waypoints.filter((_, i) => i !== index).map((p) => ({ ...p }));
}
