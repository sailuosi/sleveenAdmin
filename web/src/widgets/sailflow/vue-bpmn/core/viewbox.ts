import type { DiagramSnapshot, Point } from './types.js';

export function panViewbox(state: DiagramSnapshot, delta: Point): DiagramSnapshot {
  const v = state.viewbox;
  return {
    ...state,
    viewbox: { ...v, x: v.x - delta.x, y: v.y - delta.y },
  };
}

export function zoomViewbox(
  state: DiagramSnapshot,
  factor: number,
  center?: Point,
): DiagramSnapshot {
  const v = state.viewbox;
  const nextScale = Math.min(4, Math.max(0.2, v.scale * (1 + factor)));
  const cx = center?.x ?? v.x + v.width / 2;
  const cy = center?.y ?? v.y + v.height / 2;
  const ratio = nextScale / v.scale;
  return {
    ...state,
    viewbox: {
      ...v,
      scale: nextScale,
      x: cx - (cx - v.x) * ratio,
      y: cy - (cy - v.y) * ratio,
      width: v.width / ratio,
      height: v.height / ratio,
    },
  };
}

export function fitViewport(state: DiagramSnapshot, padding = 40): DiagramSnapshot {
  const shapes = Object.values(state.elements).filter(
    (e) => e.di.width > 0 && e.di.height > 0 && !e.type.includes('Flow'),
  );
  if (!shapes.length) {
    return { ...state, viewbox: { ...state.viewbox, x: 0, y: 0, scale: 1, width: 1000, height: 600 } };
  }
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;
  for (const el of shapes) {
    minX = Math.min(minX, el.di.x);
    minY = Math.min(minY, el.di.y);
    maxX = Math.max(maxX, el.di.x + el.di.width);
    maxY = Math.max(maxY, el.di.y + el.di.height);
  }
  const width = Math.max(100, maxX - minX + padding * 2);
  const height = Math.max(100, maxY - minY + padding * 2);
  return {
    ...state,
    viewbox: {
      x: minX - padding,
      y: minY - padding,
      width,
      height,
      scale: 1,
    },
  };
}
