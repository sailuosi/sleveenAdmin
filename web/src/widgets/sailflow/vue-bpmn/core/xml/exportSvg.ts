import type { DiagramSnapshot } from '../types.js';
import { isConnection } from './elementKind.js';

/**
 * Structural SVG export for C01-008 — element ids present as data-element-id.
 * Not pixel-parity with bpmn-js.
 */
export function exportSvg(state: DiagramSnapshot): { svg: string } {
  const shapes = Object.values(state.elements).filter((el) => !isConnection(el));
  const edges = Object.values(state.elements).filter((el) => isConnection(el));

  let minX = Infinity;
  let minY = Infinity;
  let maxX = 0;
  let maxY = 0;
  for (const el of shapes) {
    minX = Math.min(minX, el.di.x);
    minY = Math.min(minY, el.di.y);
    maxX = Math.max(maxX, el.di.x + el.di.width);
    maxY = Math.max(maxY, el.di.y + el.di.height);
  }
  if (!Number.isFinite(minX)) {
    minX = 0;
    minY = 0;
    maxX = 100;
    maxY = 100;
  }
  const pad = 40;
  const width = Math.max(100, maxX - minX + pad * 2);
  const height = Math.max(100, maxY - minY + pad * 2);

  const shapeNodes = shapes
    .map((el) => {
      const x = el.di.x;
      const y = el.di.y;
      const w = el.di.width || 100;
      const h = el.di.height || 80;
      if (el.type.includes('Event')) {
        const cx = x + w / 2;
        const cy = y + h / 2;
        const r = Math.min(w, h) / 2;
        return `  <circle data-element-id="${el.id}" cx="${cx}" cy="${cy}" r="${r}" fill="#fff" stroke="#000" />`;
      }
      if (el.type.includes('Gateway')) {
        const points = [
          `${x + w / 2},${y}`,
          `${x + w},${y + h / 2}`,
          `${x + w / 2},${y + h}`,
          `${x},${y + h / 2}`,
        ].join(' ');
        return `  <polygon data-element-id="${el.id}" points="${points}" fill="#fff" stroke="#000" />`;
      }
      return `  <rect data-element-id="${el.id}" x="${x}" y="${y}" width="${w}" height="${h}" rx="8" fill="#fff" stroke="#000" />`;
    })
    .join('\n');

  const edgeNodes = edges
    .map((el) => {
      const pts = el.waypoints ?? [];
      if (pts.length < 2) return '';
      const d = pts.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ');
      return `  <path data-element-id="${el.id}" d="${d}" fill="none" stroke="#000" />`;
    })
    .filter(Boolean)
    .join('\n');

  const svg = `<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="${minX - pad} ${minY - pad} ${width} ${height}">
${edgeNodes}
${shapeNodes}
</svg>
`;

  return { svg };
}
