import type { DiagramSnapshot } from './types.js';

export type AlignMode = 'left' | 'right' | 'top' | 'bottom' | 'center' | 'middle';

export function alignElements(
  state: DiagramSnapshot,
  ids: string[],
  mode: AlignMode,
): DiagramSnapshot {
  const targets = ids.map((id) => state.elements[id]).filter(Boolean);
  if (targets.length < 2) return state;

  const elements = { ...state.elements };
  let anchor = 0;
  if (mode === 'left') anchor = Math.min(...targets.map((e) => e!.di.x));
  if (mode === 'right') anchor = Math.max(...targets.map((e) => e!.di.x + e!.di.width));
  if (mode === 'top') anchor = Math.min(...targets.map((e) => e!.di.y));
  if (mode === 'bottom') anchor = Math.max(...targets.map((e) => e!.di.y + e!.di.height));
  if (mode === 'center') {
    const min = Math.min(...targets.map((e) => e!.di.x));
    const max = Math.max(...targets.map((e) => e!.di.x + e!.di.width));
    anchor = (min + max) / 2;
  }
  if (mode === 'middle') {
    const min = Math.min(...targets.map((e) => e!.di.y));
    const max = Math.max(...targets.map((e) => e!.di.y + e!.di.height));
    anchor = (min + max) / 2;
  }

  for (const el of targets) {
    if (!el) continue;
    const next = { ...el, di: { ...el.di } };
    if (mode === 'left') next.di.x = anchor;
    if (mode === 'right') next.di.x = anchor - el.di.width;
    if (mode === 'top') next.di.y = anchor;
    if (mode === 'bottom') next.di.y = anchor - el.di.height;
    if (mode === 'center') next.di.x = anchor - el.di.width / 2;
    if (mode === 'middle') next.di.y = anchor - el.di.height / 2;
    elements[el.id] = next;
  }

  return { ...state, elements };
}

export function distributeElements(
  state: DiagramSnapshot,
  ids: string[],
  axis: 'horizontal' | 'vertical',
): DiagramSnapshot {
  const targets = ids
    .map((id) => state.elements[id])
    .filter(Boolean)
    .sort((a, b) => (axis === 'horizontal' ? a!.di.x - b!.di.x : a!.di.y - b!.di.y));
  if (targets.length < 3) return state;

  const first = targets[0]!;
  const last = targets[targets.length - 1]!;
  const span =
    axis === 'horizontal'
      ? last.di.x + last.di.width - first.di.x
      : last.di.y + last.di.height - first.di.y;
  const totalSize = targets.reduce(
    (sum, e) => sum + (axis === 'horizontal' ? e!.di.width : e!.di.height),
    0,
  );
  const gap = (span - totalSize) / (targets.length - 1);
  const elements = { ...state.elements };
  let cursor = axis === 'horizontal' ? first.di.x : first.di.y;
  for (const el of targets) {
    if (!el) continue;
    const next = { ...el, di: { ...el.di } };
    if (axis === 'horizontal') next.di.x = cursor;
    else next.di.y = cursor;
    elements[el.id] = next;
    cursor += (axis === 'horizontal' ? el.di.width : el.di.height) + gap;
  }
  return { ...state, elements };
}
