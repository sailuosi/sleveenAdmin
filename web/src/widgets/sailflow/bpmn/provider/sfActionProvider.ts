import { is } from 'bpmn-js/lib/util/ModelUtil';
import { makeSelectOnNested, makeTextOnNested } from '../utils/panelEntries';

const LOW_PRIORITY = 500;

const fireTypeEntry = makeSelectOnNested({
  id: 'sf-action-fire',
  label: '触发时机',
  collectionType: 'sf:Actions',
  listProp: 'actions',
  itemType: 'sf:Action',
  prop: 'fireType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '之前 (before)', value: '1', name: 'before' },
    { label: '之后 (after)', value: '2', name: 'after' }
  ]
});

const methodTypeEntry = makeSelectOnNested({
  id: 'sf-action-method',
  label: '方法类型',
  collectionType: 'sf:Actions',
  listProp: 'actions',
  itemType: 'sf:Action',
  prop: 'methodType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: 'WebApi', value: '1', name: 'WebApi' },
    { label: 'LocalService', value: '2', name: 'LocalService' },
    { label: 'CSharpLibrary', value: '3', name: 'CSharpLibrary' },
    { label: 'SQLStoreProcedure', value: '4', name: 'SQLStoreProcedure' }
  ]
});

const subMethodTypeEntry = makeSelectOnNested({
  id: 'sf-action-submethod',
  label: 'HTTP 方法',
  collectionType: 'sf:Actions',
  listProp: 'actions',
  itemType: 'sf:Action',
  prop: 'subMethodType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: 'HttpGet', value: '1', name: 'HttpGet' },
    { label: 'HttpPost', value: '2', name: 'HttpPost' },
    { label: 'HttpPut', value: '3', name: 'HttpPut' },
    { label: 'HttpDelete', value: '4', name: 'HttpDelete' }
  ]
});

const argusEntry = makeTextOnNested({
  id: 'sf-action-argus',
  label: '参数',
  collectionType: 'sf:Actions',
  listProp: 'actions',
  itemType: 'sf:Action',
  prop: 'argus'
});

const expressionEntry = makeTextOnNested({
  id: 'sf-action-expression',
  label: '表达式 / URL',
  collectionType: 'sf:Actions',
  listProp: 'actions',
  itemType: 'sf:Action',
  prop: 'expression',
  rows: 3
});

function isActionHost(element: unknown): boolean {
  // 对齐 ClientApp：非 Service/Script，且为 Task/UserTask/中间抛出事件
  if (is(element, 'bpmn:ServiceTask') || is(element, 'bpmn:ScriptTask')) return false;
  return (
    is(element, 'bpmn:Task') ||
    is(element, 'bpmn:UserTask') ||
    is(element, 'bpmn:IntermediateThrowEvent')
  );
}

function SfActionPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!isActionHost(element)) return groups;
      groups.push({
        id: 'sf-action',
        label: '动作',
        entries: [
          fireTypeEntry(element),
          methodTypeEntry(element),
          subMethodTypeEntry(element),
          argusEntry(element),
          expressionEntry(element)
        ]
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfActionPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfActionPropertiesProviderModule = {
  __init__: ['sfActionPropertiesProvider'],
  sfActionPropertiesProvider: ['type', SfActionPropertiesProvider]
};
