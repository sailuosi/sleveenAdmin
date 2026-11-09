import type { Bounds, Command, CommandContext, DiagramSnapshot } from '../types.js';

export interface ResizeElementPayload {
  id: string;
  bounds: Partial<Bounds>;
}

const MIN_SIZE: Record<string, { width: number; height: number }> = {
  'bpmn:SubProcess': { width: 100, height: 80 },
  'bpmn:Transaction': { width: 100, height: 80 },
  'bpmn:AdHocSubProcess': { width: 100, height: 80 },
  'bpmn:Participant': { width: 300, height: 150 },
  'bpmn:Task': { width: 50, height: 40 },
  default: { width: 20, height: 20 },
};

export function clampBounds(type: string, bounds: Bounds): Bounds {
  const min = MIN_SIZE[type] ?? MIN_SIZE.default!;
  return {
    x: bounds.x,
    y: bounds.y,
    width: Math.max(min.width, bounds.width),
    height: Math.max(min.height, bounds.height),
  };
}

export function createResizeElementCommand(payload: ResizeElementPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'shape.resize',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const el = before.elements[payload.id];
        if (!el) return;
        const nextBounds = clampBounds(el.type, {
          x: payload.bounds.x ?? el.di.x,
          y: payload.bounds.y ?? el.di.y,
          width: payload.bounds.width ?? el.di.width,
          height: payload.bounds.height ?? el.di.height,
        });
        after = {
          ...before,
          elements: {
            ...before.elements,
            [payload.id]: {
              ...el,
              di: { ...el.di, ...nextBounds },
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
