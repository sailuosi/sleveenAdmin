import type { Bounds, DiagramElement, DiagramSnapshot, Point } from './types.js';
import { isConnection } from './xml/elementKind.js';

function centerOf(b: Bounds): Point {
  return { x: b.x + b.width / 2, y: b.y + b.height / 2 };
}

function dist2(a: Point, b: Point): number {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  return dx * dx + dy * dy;
}

function clamp(n: number, lo: number, hi: number): number {
  return Math.max(lo, Math.min(hi, n));
}

function projectOntoSegment(p: Point, a: Point, b: Point): Point {
  const abx = b.x - a.x;
  const aby = b.y - a.y;
  const len2 = abx * abx + aby * aby;
  if (len2 < 1e-9) return { ...a };
  const t = clamp(((p.x - a.x) * abx + (p.y - a.y) * aby) / len2, 0, 1);
  return { x: a.x + abx * t, y: a.y + aby * t };
}

/** 轴对齐矩形：投影到边界（内部点推到最近边） */
export function projectOntoRect(b: Bounds, p: Point): Point {
  const left = b.x;
  const right = b.x + b.width;
  const top = b.y;
  const bottom = b.y + b.height;
  const inside =
    p.x > left && p.x < right && p.y > top && p.y < bottom;
  if (!inside) {
    return {
      x: clamp(p.x, left, right),
      y: clamp(p.y, top, bottom),
    };
  }
  const dl = p.x - left;
  const dr = right - p.x;
  const dt = p.y - top;
  const db = bottom - p.y;
  const m = Math.min(dl, dr, dt, db);
  if (m === dl) return { x: left, y: p.y };
  if (m === dr) return { x: right, y: p.y };
  if (m === dt) return { x: p.x, y: top };
  return { x: p.x, y: bottom };
}

/** 圆（开始/结束/中间事件）：投影到圆周 */
export function projectOntoCircle(b: Bounds, p: Point): Point {
  const c = centerOf(b);
  const r = Math.max(1, Math.min(b.width, b.height) / 2);
  let dx = p.x - c.x;
  let dy = p.y - c.y;
  const len = Math.hypot(dx, dy);
  if (len < 1e-6) {
    // 与圆心重合时默认靠右
    return { x: c.x + r, y: c.y };
  }
  return { x: c.x + (dx / len) * r, y: c.y + (dy / len) * r };
}

/** 菱形网关：投影到四条边 */
export function projectOntoDiamond(b: Bounds, p: Point): Point {
  const midX = b.x + b.width / 2;
  const midY = b.y + b.height / 2;
  const top = { x: midX, y: b.y };
  const right = { x: b.x + b.width, y: midY };
  const bottom = { x: midX, y: b.y + b.height };
  const left = { x: b.x, y: midY };
  const candidates = [
    projectOntoSegment(p, top, right),
    projectOntoSegment(p, right, bottom),
    projectOntoSegment(p, bottom, left),
    projectOntoSegment(p, left, top),
  ];
  let best = candidates[0]!;
  let bestD = dist2(p, best);
  for (let i = 1; i < candidates.length; i++) {
    const d = dist2(p, candidates[i]!);
    if (d < bestD) {
      bestD = d;
      best = candidates[i]!;
    }
  }
  return best;
}

function dockingKind(el: DiagramElement): 'circle' | 'diamond' | 'rect' {
  if (el.type.includes('Event')) return 'circle';
  if (el.type.includes('Gateway')) return 'diamond';
  return 'rect';
}

/** 将点投影到形状轮廓（任务矩形 / 事件圆 / 网关菱形） */
export function projectPointOntoShape(el: DiagramElement, point: Point): Point {
  const kind = dockingKind(el);
  if (kind === 'circle') return projectOntoCircle(el.di, point);
  if (kind === 'diamond') return projectOntoDiamond(el.di, point);
  return projectOntoRect(el.di, point);
}

/**
 * 拖连线端点时：首/尾贴到 source/target 边缘；中间折点自由。
 */
export function resolveDockedWaypoint(
  state: DiagramSnapshot,
  connectionId: string,
  index: number,
  point: Point,
): Point {
  const conn = state.elements[connectionId];
  if (!conn?.waypoints?.length) return point;
  const last = conn.waypoints.length - 1;
  if (index !== 0 && index !== last) return point;

  const hostId =
    index === 0
      ? String(conn.businessObject.sourceRef ?? '')
      : String(conn.businessObject.targetRef ?? '');
  if (!hostId) return point;
  const host = state.elements[hostId];
  if (!host || isConnection(host)) return point;
  return projectPointOntoShape(host, point);
}

/** 两节点之间连线的贴边锚点（朝向对方） */
export function dockAnchorsBetween(
  source: DiagramElement,
  target: DiagramElement,
): [Point, Point] {
  const sc = centerOf(source.di);
  const tc = centerOf(target.di);
  return [projectPointOntoShape(source, tc), projectPointOntoShape(target, sc)];
}
