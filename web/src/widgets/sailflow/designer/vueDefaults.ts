import type { DiagramSnapshot, DiagramStore } from '@vue-bpmn/core';
import { defaultButtonsSetting } from '@sail/bpmn/buttonsCatalog';
import { ensureChildExtension, getExtension } from '@sail/bpmn/utils/sfExtension';
import {
  DEFAULT_ASSIGNEE_RESOLVE_TYPE,
  DEFAULT_LEAVE_JUDGE_TYPE,
  DEFAULT_LEAVE_JUDGE_PARAMS,
} from '@sail/bpmn/assigneeDefaults';
import {
  createVueModdleBridge,
  createVueModelingBridge,
  wrapDiagramElement,
} from './vueModdle';

const START_ALIASES = new Set(['', 'start', 'startevent', '开始事件']);
const END_ALIASES = new Set(['', 'end', 'endevent', '结束事件']);

function shouldApplyDefault(name: string | undefined, aliases: Set<string>): boolean {
  const n = (name || '').trim().toLowerCase();
  return !n || aliases.has(n);
}

/**
 * Apply Sail design defaults when elements appear in the vue-bpmn store.
 */
export function installVueDesignDefaults(store: DiagramStore): () => void {
  const modeling = createVueModelingBridge(store);
  const moddle = createVueModdleBridge();
  const seen = new Set<string>();

  function apply(id: string) {
    if (seen.has(id)) return;
    const de = store.getElement(id);
    if (!de) return;
    const type = String(de.type);

    if (type === 'bpmn:StartEvent' && shouldApplyDefault(de.name, START_ALIASES)) {
      store.updateElement(id, { name: '开始' });
      seen.add(id);
      return;
    }
    if (type === 'bpmn:EndEvent' && shouldApplyDefault(de.name, END_ALIASES)) {
      store.updateElement(id, { name: '结束' });
      seen.add(id);
      return;
    }

    if (type !== 'bpmn:UserTask' && type !== 'bpmn:Task') return;
    const view = wrapDiagramElement(store, id);
    if (!view) return;
    const bo = view.businessObject;
    const hasResolve = !!getExtension(bo, 'sf:AssigneeResolve');
    const hasLeave = !!getExtension(bo, 'sf:LeaveJudge');
    const hasButtons = !!getExtension(bo, 'sf:ButtonsSetting');
    if (hasResolve && hasLeave && hasButtons) {
      seen.add(id);
      return;
    }
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
    modeling.updateProperties(view, { extensionElements: bo.extensionElements });
    seen.add(id);
  }

  const unsub = store.subscribe((state: DiagramSnapshot) => {
    for (const id of Object.keys(state.elements)) {
      apply(id);
    }
  });

  return () => {
    unsub();
    seen.clear();
  };
}
