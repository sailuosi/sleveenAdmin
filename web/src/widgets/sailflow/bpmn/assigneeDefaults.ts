import { is } from 'bpmn-js/lib/util/ModelUtil';
import { defaultButtonsSetting } from './buttonsCatalog';
import { ensureChildExtension, getBo, getExtension } from './utils/sfExtension';

/** R6 产品出厂设计默认：发送人自选 + 协办 */
export const DEFAULT_ASSIGNEE_RESOLVE_TYPE = 'ApproverPick';
export const DEFAULT_LEAVE_JUDGE_TYPE = 'Collaborate';
export const DEFAULT_LEAVE_JUDGE_PARAMS = JSON.stringify({ completeRate: 1 });

type ModelerLike = {
  get: (name: string) => unknown;
};

type ShapeLike = {
  businessObject?: unknown;
  id?: string;
};

/**
 * 画布新增 UserTask/Task 时预埋 assignee / leave / 办理按钮设置（已有则不覆盖）。
 * 必须在 commandStack 之外调用 updateProperties，否则会 illegal invocation。
 */
export function installAssigneeDesignDefaults(modeler: ModelerLike) {
  const eventBus = modeler.get('eventBus') as {
    on: (e: string, priority: number | ((ev: any) => void), fn?: (ev: any) => void) => void;
  };
  const modeling = modeler.get('modeling') as {
    updateProperties: (el: unknown, props: object) => void;
  };
  const moddle = modeler.get('moddle') as { create: (type: string, props?: object) => any };

  function applyDefaults(element: ShapeLike | undefined | null) {
    if (!element || (!is(element, 'bpmn:UserTask') && !is(element, 'bpmn:Task'))) return;
    const bo = getBo(element);
    if (!bo) return;

    const hasResolve = !!getExtension(bo, 'sf:AssigneeResolve');
    const hasLeave = !!getExtension(bo, 'sf:LeaveJudge');
    const hasButtons = !!getExtension(bo, 'sf:ButtonsSetting');
    if (hasResolve && hasLeave && hasButtons) return;

    if (!hasResolve) {
      const ext = ensureChildExtension(bo, 'sf:AssigneeResolve', moddle);
      ext.type = DEFAULT_ASSIGNEE_RESOLVE_TYPE;
      if (ext.params == null) ext.params = '';
    }
    if (!hasLeave) {
      const ext = ensureChildExtension(bo, 'sf:LeaveJudge', moddle);
      ext.type = DEFAULT_LEAVE_JUDGE_TYPE;
      ext.params = DEFAULT_LEAVE_JUDGE_PARAMS;
    }
    if (!hasButtons) {
      const ext = ensureChildExtension(bo, 'sf:ButtonsSetting', moddle);
      ext.json = JSON.stringify(defaultButtonsSetting());
    }
    modeling.updateProperties(element, { extensionElements: bo.extensionElements });
  }

  const onShapes = (shapes: Array<ShapeLike | undefined> | undefined) => {
    (shapes || []).forEach(shape => applyDefaults(shape));
  };

  eventBus.on('commandStack.elements.create.postExecuted', 1500, (event: { context?: { elements?: ShapeLike[] } }) => {
    onShapes(event?.context?.elements);
  });

  eventBus.on('commandStack.shape.create.postExecuted', 1500, (event: { context?: { shape?: ShapeLike } }) => {
    applyDefaults(event?.context?.shape);
  });

  eventBus.on('commandStack.shape.replace.postExecuted', 1500, (event: { context?: { newShape?: ShapeLike } }) => {
    applyDefaults(event?.context?.newShape);
  });

  eventBus.on('create.end', 1500, (event: { context?: { shape?: ShapeLike; elements?: ShapeLike[] } }) => {
    applyDefaults(event?.context?.shape);
    onShapes(event?.context?.elements);
  });
}
