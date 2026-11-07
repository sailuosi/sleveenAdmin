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

/**
 * 画布新增 UserTask/Task 时预埋 assignee / leave / 办理按钮设置（已有则不覆盖）。
 */
export function installAssigneeDesignDefaults(modeler: ModelerLike) {
  const eventBus = modeler.get('eventBus') as { on: (e: string, fn: (ev: any) => void) => void };
  const modeling = modeler.get('modeling') as {
    updateProperties: (el: unknown, props: object) => void;
  };
  const moddle = modeler.get('moddle') as { create: (type: string, props?: object) => any };

  eventBus.on('shape.added', (event: { element?: unknown; context?: { hints?: { create?: boolean } } }) => {
    const element = event?.element as { businessObject?: unknown } | undefined;
    if (!element || (!is(element, 'bpmn:UserTask') && !is(element, 'bpmn:Task'))) return;
    // 导入/粘贴时也可能触发；仅当扩展尚未写入时预埋
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
  });
}
