import type { DiagramSnapshot } from './types.js';

/** Find element ids whose name/id matches query (C07-006). */
export function findElements(state: DiagramSnapshot, query: string): string[] {
  const q = query.trim().toLowerCase();
  if (!q) return [];
  return Object.values(state.elements)
    .filter((e) => {
      const name = (e.name ?? '').toLowerCase();
      const id = e.id.toLowerCase();
      return name.includes(q) || id.includes(q);
    })
    .map((e) => e.id);
}
