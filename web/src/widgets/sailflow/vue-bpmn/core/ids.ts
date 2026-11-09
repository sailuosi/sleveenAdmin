import type { DiagramSnapshot } from './types.js';

export function nextId(state: DiagramSnapshot, prefix: string): string {
  let i = 1;
  while (state.elements[`${prefix}_${i}`] || state.rootId === `${prefix}_${i}`) {
    i += 1;
  }
  return `${prefix}_${i}`;
}
