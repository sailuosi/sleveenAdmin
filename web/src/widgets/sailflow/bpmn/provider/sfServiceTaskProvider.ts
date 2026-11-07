import { is } from 'bpmn-js/lib/util/ModelUtil';
import { makeSelectOnNested, makeTextOnNested } from '../utils/panelEntries';

const LOW_PRIORITY = 500;

const methodTypeEntry = makeSelectOnNested({
  id: 'sf-svc-method',
  label: '调用类型',
  collectionType: 'sf:Services',
  listProp: 'services',
  itemType: 'sf:Service',
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
  id: 'sf-svc-submethod',
  label: 'HTTP 方法',
  collectionType: 'sf:Services',
  listProp: 'services',
  itemType: 'sf:Service',
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
  id: 'sf-svc-argus',
  label: '参数',
  collectionType: 'sf:Services',
  listProp: 'services',
  itemType: 'sf:Service',
  prop: 'argus'
});

const expressionEntry = makeTextOnNested({
  id: 'sf-svc-expression',
  label: '表达式 / URL',
  collectionType: 'sf:Services',
  listProp: 'services',
  itemType: 'sf:Service',
  prop: 'expression',
  rows: 3
});

function SfServiceTaskPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!is(element, 'bpmn:ServiceTask')) return groups;
      groups.push({
        id: 'sf-servicetask',
        label: '服务任务',
        entries: [
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

SfServiceTaskPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfServiceTaskPropertiesProviderModule = {
  __init__: ['sfServiceTaskPropertiesProvider'],
  sfServiceTaskPropertiesProvider: ['type', SfServiceTaskPropertiesProvider]
};
