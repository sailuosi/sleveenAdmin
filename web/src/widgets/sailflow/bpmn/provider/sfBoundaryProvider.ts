import { is } from 'bpmn-js/lib/util/ModelUtil';
import { TextFieldEntry, isTextFieldEntryEdited } from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';
import { ensureNestedListItem, getBo, getExtension } from '../utils/sfExtension';

const LOW_PRIORITY = 500;

type Modeling = { updateProperties: (el: unknown, props: object) => void };
type Moddle = { create: (type: string, props?: object) => any };

function findTimerBoundary(boundariesEl: any): any | null {
  const list = boundariesEl?.get?.('boundaries') || boundariesEl?.boundaries || [];
  if (!Array.isArray(list)) return null;
  return list.find((b: any) => b.event === 'Timer') || null;
}

function makeOverdueEntry() {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const boundaries = getExtension(bo, 'sf:Boundaries');
      const timer = findTimerBoundary(boundaries);
      return String(timer?.expression ?? '');
    };

    const setValue = (value: string) => {
      const bo = getBo(element);
      const collection = ensureNestedListItem(
        bo,
        'sf:Boundaries',
        'boundaries',
        'sf:Boundary',
        moddle
      );
      // ensureNestedListItem 返回首项；强制 Timer 语义
      let timer = findTimerBoundary(getExtension(bo, 'sf:Boundaries'));
      if (!timer) {
        timer = collection;
        timer.event = 'Timer';
      } else {
        timer.event = 'Timer';
      }
      timer.expression = value;
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    return createElement(TextFieldEntry as never, {
      id,
      element,
      label: '逾期时长（Boundaries）',
      description:
        'ISO8601，例 PT10M / PT1H / P1D。仅写入 OverdueDateTime，不执行提醒/自动同意/拒绝。产品超时策略请用节点配置「审批人超时未处理时」。',
      getValue,
      setValue,
      debounce
    }) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: 'sf-overdue',
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}

const overdueEntry = makeOverdueEntry();

function SfBoundaryPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!(is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask'))) return groups;
      groups.push({
        id: 'sf-boundary',
        label: '逾期时长（Boundaries）',
        entries: [overdueEntry(element)]
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfBoundaryPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfBoundaryPropertiesProviderModule = {
  __init__: ['sfBoundaryPropertiesProvider'],
  sfBoundaryPropertiesProvider: ['type', SfBoundaryPropertiesProvider]
};
