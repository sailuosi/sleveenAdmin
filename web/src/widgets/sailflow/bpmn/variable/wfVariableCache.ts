import { is } from 'bpmn-js/lib/util/ModelUtil';
import { getVariableList, type VariableRow } from '@sail/api/variable';
import { getDesignerProcessMeta } from '../ui/designerContext';

const cache: Record<string, VariableRow[]> = {};

export function isTaskLike(el: unknown): boolean {
  if (!el) return false;
  return (
    is(el, 'bpmn:Task') ||
    is(el, 'bpmn:UserTask') ||
    is(el, 'bpmn:ServiceTask') ||
    is(el, 'bpmn:ScriptTask') ||
    is(el, 'bpmn:BusinessRuleTask') ||
    is(el, 'bpmn:SendTask') ||
    is(el, 'bpmn:ReceiveTask') ||
    is(el, 'bpmn:ManualTask')
  );
}

export function getCachedVariables(activityId: string): VariableRow[] | undefined {
  return Object.prototype.hasOwnProperty.call(cache, activityId) ? cache[activityId] : undefined;
}

export function setCachedVariables(activityId: string, rows: VariableRow[]) {
  cache[activityId] = rows;
}

export function clearVariableCache() {
  Object.keys(cache).forEach(k => delete cache[k]);
}

function unwrapList(data: any): VariableRow[] {
  if (Array.isArray(data)) return data;
  const entity = data?.Entity ?? data?.entity ?? data;
  return Array.isArray(entity) ? entity : [];
}

export async function loadVariablesForActivity(activityId: string): Promise<VariableRow[]> {
  const { processId, version } = getDesignerProcessMeta();
  if (!processId || !activityId) {
    setCachedVariables(activityId, []);
    return [];
  }
  const data = await getVariableList(processId, version || '1', activityId);
  if (data?.Status === -1 || data?.Status === '-1') {
    throw new Error(data?.Message || '加载变量失败');
  }
  const list = unwrapList(data);
  setCachedVariables(activityId, list);
  return list;
}

/**
 * 选中任务节点时拉取 wf_variable → 缓存，并触发属性面板刷新
 */
export function initWfVariableCacheLoader(eventBus: { on: Function; fire: Function }) {
  eventBus.on('selection.changed', (e: { newSelection?: unknown[] }) => {
    const el = e.newSelection?.[0] as { id?: string } | undefined;
    if (!isTaskLike(el) || !el?.id) return;
    loadVariablesForActivity(el.id)
      .then(() => {
        eventBus.fire('elements.changed', { elements: [el] });
      })
      .catch(() => {
        /* ignore */
      });
  });
}

/** 保存前去掉 BPMN 中的 sf:Variables（变量落库 wf_variable） */
export function stripSfVariablesFromDiagram(
  modeling: { updateModdleProperties: Function },
  elementRegistry: { getAll: () => any[] }
) {
  elementRegistry.getAll().forEach(el => {
    const bo = el.businessObject;
    if (!bo?.extensionElements) return;
    const extensionElements = bo.extensionElements;
    const values = extensionElements.get ? extensionElements.get('values') || [] : extensionElements.values || [];
    const newValues = values.filter((e: any) => e.$type !== 'sf:Variables');
    if (newValues.length !== values.length) {
      modeling.updateModdleProperties(el, extensionElements, { values: newValues });
    }
  });
}
