import { is } from 'bpmn-js/lib/util/ModelUtil';

type ModelerLike = {
  get: (name: string) => unknown;
};

type ShapeLike = {
  businessObject?: { name?: string };
  labelTarget?: unknown;
  type?: string;
  x?: number;
  y?: number;
  width?: number;
  height?: number;
};

const START_ALIASES = new Set(['', 'start', 'startevent']);
const END_ALIASES = new Set(['', 'end', 'endevent']);

function normalizeName(raw: unknown): string {
  return String(raw ?? '').trim();
}

function shouldApplyDefault(current: string, aliases: Set<string>): boolean {
  return aliases.has(current.toLowerCase());
}

function resolveDefaultName(element: ShapeLike): string | null {
  if (!element || element.labelTarget) return null;
  const name = normalizeName(element.businessObject?.name);
  if (is(element, 'bpmn:StartEvent') && shouldApplyDefault(name, START_ALIASES)) return '开始';
  if (is(element, 'bpmn:EndEvent') && shouldApplyDefault(name, END_ALIASES)) return '结束';
  return null;
}

function applyDefaultName(
  modeling: { updateLabel: (el: unknown, text: string | null) => void },
  element: ShapeLike | undefined
) {
  if (!element) return;
  const next = resolveDefaultName(element);
  if (!next) return;
  // 等图形落位后再写外部标签，避免标签 DI 落在错误坐标（保存重开后错位）
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      if (!resolveDefaultName(element)) return;
      modeling.updateLabel(element, next);
    });
  });
}

/**
 * 画布新增开始/结束事件时补默认中文名。
 * bpmn-js 18 拖拽走 elements.create / create.end。
 */
export function installEventNameDefaults(modeler: ModelerLike) {
  const eventBus = modeler.get('eventBus') as {
    on: (e: string, priority: number | ((ev: any) => void), fn?: (ev: any) => void) => void;
  };
  const modeling = modeler.get('modeling') as {
    updateLabel: (el: unknown, text: string | null) => void;
  };

  const onShapes = (shapes: Array<ShapeLike | undefined> | undefined) => {
    (shapes || []).forEach(shape => applyDefaultName(modeling, shape));
  };

  eventBus.on('commandStack.elements.create.postExecuted', 1500, (event: { context?: { elements?: ShapeLike[] } }) => {
    onShapes(event?.context?.elements);
  });

  eventBus.on('commandStack.shape.create.postExecuted', 1500, (event: { context?: { shape?: ShapeLike } }) => {
    applyDefaultName(modeling, event?.context?.shape);
  });

  eventBus.on('commandStack.shape.replace.postExecuted', 1500, (event: { context?: { newShape?: ShapeLike } }) => {
    applyDefaultName(modeling, event?.context?.newShape);
  });

  // 拖拽落盘完成（含坐标）后再补一次，覆盖仅 postExecuted 时图形未就绪的情况
  eventBus.on('create.end', 1500, (event: { context?: { shape?: ShapeLike; elements?: ShapeLike[] } }) => {
    applyDefaultName(modeling, event?.context?.shape);
    onShapes(event?.context?.elements);
  });
}
