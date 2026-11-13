import type { DiagramSnapshot, Point } from './types.js';

const DEFAULT_VP = { width: 1000, height: 600 };

/**
 * viewbox 约定：
 * - width/height：画布参考尺寸（≈ SVG 像素），缩放时保持不变
 * - scale：缩放倍数；SVG viewBox 可见世界 = (width/scale) × (height/scale)
 * - scale=1 → 约 1 世界单位 ≈ 1 CSS 像素
 */
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
  if (nextScale === v.scale) return state;

  const worldW = v.width / v.scale;
  const worldH = v.height / v.scale;
  const cx = center?.x ?? v.x + worldW / 2;
  const cy = center?.y ?? v.y + worldH / 2;
  // 保持中心点不动：可见宽高按 scale 反比变化
  const ratio = v.scale / nextScale;
  return {
    ...state,
    viewbox: {
      ...v,
      scale: nextScale,
      x: cx - (cx - v.x) * ratio,
      y: cy - (cy - v.y) * ratio,
    },
  };
}

export type FitViewportOptions = {
  /** 画布 CSS 像素尺寸；缺省用当前 viewbox.width/height 或 1000×600 */
  viewport?: { width: number; height: number };
  padding?: number;
  /**
   * 适应时最大缩放。默认 1：内容很少时不放大（避免单节点占满屏）。
   * 内容超出视口时仍会缩小到 ≤1。
   */
  maxScale?: number;
};

/**
 * 把图元放进视口中央。
 * - 小图：scale≤maxScale（默认 1），四周留白，节点保持正常像素大小
 * - 大图：缩小直至装下
 */
export function fitViewport(
  state: DiagramSnapshot,
  paddingOrOpts: number | FitViewportOptions = 40,
): DiagramSnapshot {
  const opts: FitViewportOptions =
    typeof paddingOrOpts === 'number' ? { padding: paddingOrOpts } : paddingOrOpts ?? {};
  const padding = opts.padding ?? 40;
  const maxScale = opts.maxScale ?? 1;
  const vpW = Math.max(
    120,
    opts.viewport?.width || state.viewbox.width || DEFAULT_VP.width,
  );
  const vpH = Math.max(
    120,
    opts.viewport?.height || state.viewbox.height || DEFAULT_VP.height,
  );

  const shapes = Object.values(state.elements).filter(
    (e) => e.di.width > 0 && e.di.height > 0 && !e.type.includes('Flow'),
  );
  if (!shapes.length) {
    return {
      ...state,
      viewbox: { x: 0, y: 0, width: vpW, height: vpH, scale: 1 },
    };
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

  const paddedW = Math.max(1, maxX - minX + padding * 2);
  const paddedH = Math.max(1, maxY - minY + padding * 2);
  const fitScale = Math.min(vpW / paddedW, vpH / paddedH);
  const scale = Math.min(maxScale, Math.max(0.2, fitScale));

  const worldW = vpW / scale;
  const worldH = vpH / scale;
  const cx = (minX + maxX) / 2;
  const cy = (minY + maxY) / 2;

  return {
    ...state,
    viewbox: {
      x: cx - worldW / 2,
      y: cy - worldH / 2,
      width: vpW,
      height: vpH,
      scale,
    },
  };
}
