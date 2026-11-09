import type { DiagramElement, DiagramSnapshot, Point } from './types.js';

const DEFAULT_TOLERANCE = 6;

export function snapDelta(
  state: DiagramSnapshot,
  movingIds: string[],
  delta: Point,
  tolerance = DEFAULT_TOLERANCE,
): Point {
  const moving = new Set(movingIds);
  const movers = movingIds.map((id) => state.elements[id]).filter(Boolean) as DiagramElement[];
  if (!movers.length) return delta;

  const others = Object.values(state.elements).filter(
    (e) => !moving.has(e.id) && e.di.width > 0 && !e.type.includes('Flow'),
  );

  let dx = delta.x;
  let dy = delta.y;
  let bestX = tolerance + 1;
  let bestY = tolerance + 1;

  for (const m of movers) {
    for (const o of others) {
      const snapXs = [
        o.di.x,
        o.di.x + o.di.width / 2 - m.di.width / 2,
        o.di.x + o.di.width - m.di.width,
      ];
      const snapYs = [
        o.di.y,
        o.di.y + o.di.height / 2 - m.di.height / 2,
        o.di.y + o.di.height - m.di.height,
      ];

      for (const sx of snapXs) {
        const adj = Math.abs(sx - m.di.x - delta.x);
        if (adj < bestX) {
          bestX = adj;
          dx = sx - m.di.x;
        }
      }
      for (const sy of snapYs) {
        const adj = Math.abs(sy - m.di.y - delta.y);
        if (adj < bestY) {
          bestY = adj;
          dy = sy - m.di.y;
        }
      }
    }
  }

  if (bestX > tolerance) dx = delta.x;
  if (bestY > tolerance) dy = delta.y;
  return { x: dx, y: dy };
}

/** Snap moving element to align with reference on one axis (C07-004). */
export function snapAlignAxis(
  state: DiagramSnapshot,
  movingId: string,
  alignWithId: string,
  axis: 'x' | 'y',
): Point {
  const moving = state.elements[movingId];
  const ref = state.elements[alignWithId];
  if (!moving || !ref) return { x: 0, y: 0 };
  if (axis === 'y') {
    return { x: 0, y: ref.di.y - moving.di.y };
  }
  return { x: ref.di.x - moving.di.x, y: 0 };
}
