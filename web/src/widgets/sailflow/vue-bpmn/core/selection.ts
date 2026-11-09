import type { Bounds, DiagramElement, DiagramSnapshot } from './types.js';
import { isConnection } from './xml/elementKind.js';

export function normalizeRect(from: { x: number; y: number }, to: { x: number; y: number }): Bounds {
  const x = Math.min(from.x, to.x);
  const y = Math.min(from.y, to.y);
  return {
    x,
    y,
    width: Math.abs(to.x - from.x),
    height: Math.abs(to.y - from.y),
  };
}

function boundsOf(el: DiagramElement): Bounds {
  return {
    x: el.di.x,
    y: el.di.y,
    width: el.di.width || 1,
    height: el.di.height || 1,
  };
}

function fullyEnclosed(inner: Bounds, outer: Bounds): boolean {
  return (
    inner.x >= outer.x &&
    inner.y >= outer.y &&
    inner.x + inner.width <= outer.x + outer.width &&
    inner.y + inner.height <= outer.y + outer.height
  );
}

/** Lasso: shapes fully inside rect (C04-003). */
export function lassoSelectIds(state: DiagramSnapshot, rect: Bounds): string[] {
  return Object.values(state.elements)
    .filter((el) => !isConnection(el) && el.id !== state.rootId)
    .filter((el) => fullyEnclosed(boundsOf(el), rect))
    .map((el) => el.id);
}

/** Select all non-root elements (C04-004). */
export function selectAllIds(state: DiagramSnapshot): string[] {
  return Object.values(state.elements)
    .filter((el) => el.id !== state.rootId)
    .map((el) => el.id);
}
