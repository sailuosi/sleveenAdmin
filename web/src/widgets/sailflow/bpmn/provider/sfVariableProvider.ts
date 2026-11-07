import { ListGroup } from '@bpmn-io/properties-panel';
import { designerEvents } from '../ui/designerEvents';
import { getCachedVariables, isTaskLike, setCachedVariables } from '../variable/wfVariableCache';
import { getDesignerProcessMeta } from '../ui/designerContext';
import { saveVariableList, type VariableRow } from '@sail/api/variable';

const LOW_PRIORITY = 500;

function labelOf(row: VariableRow, index: number) {
  const dir = row.Direction === 'Output' ? '[OUT]' : '[IN]';
  const name = row.Name || `变量 ${index + 1}`;
  const req = row.IsRequired === 1 || row.IsRequired === true ? ' • 必填' : '';
  return `${dir} ${name}${req}`;
}

function variablesProps(element: { id: string }, injector: { get: (n: string) => any }) {
  const eventBus = injector.get('eventBus');
  const rows = getCachedVariables(element.id) || [];

  const items = rows.map((row, index) => ({
    id: `${element.id}-var-${index}`,
    label: labelOf(row, index),
    remove: (event: Event) => {
      event.stopPropagation();
      const next = rows.filter((_, i) => i !== index);
      const { processId, version } = getDesignerProcessMeta();
      if (!processId) return;
      saveVariableList(processId, version || '1', element.id, next)
        .then(data => {
          if (data?.Status === -1 || data?.Status === '-1') {
            throw new Error(data?.Message || '删除变量失败');
          }
          setCachedVariables(element.id, next);
          eventBus.fire('elements.changed', { elements: [element] });
        })
        .catch(() => {
          /* ignore toast here; dialog path surfaces errors */
        });
    }
  }));

  return {
    items,
    add: (event: Event) => {
      event.stopPropagation();
      designerEvents.emit('variable:open', {
        activityId: element.id,
        rows: [...rows],
        onSaved: next => {
          setCachedVariables(element.id, next);
          eventBus.fire('elements.changed', { elements: [element] });
        }
      });
    }
  };
}

function SfVariablePropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: { id?: string }) {
    return function (groups: object[]) {
      if (isTaskLike(element) && element.id) {
        groups.push({
          id: 'sf-variables',
          label: '变量',
          component: ListGroup,
          ...variablesProps(element as { id: string }, injector)
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfVariablePropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfVariablePropertiesProviderModule = {
  __init__: ['sfVariablePropertiesProvider'],
  sfVariablePropertiesProvider: ['type', SfVariablePropertiesProvider]
};
