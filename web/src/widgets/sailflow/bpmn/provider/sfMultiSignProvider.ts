import { is } from 'bpmn-js/lib/util/ModelUtil';
import { makeSelectOnExtension, makeTextOnExtension } from '../utils/panelEntries';

const complexEntry = makeSelectOnExtension({
  id: 'sf-ms-complex',
  label: '会签类型',
  extensionType: 'sf:MultiSignDetail',
  prop: 'complexType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '会签', value: '1', name: 'SignTogether' },
    { label: '加签', value: '2', name: 'SignForward' }
  ]
});

const mergeEntry = makeSelectOnExtension({
  id: 'sf-ms-merge',
  label: '串并行',
  extensionType: 'sf:MultiSignDetail',
  prop: 'mergeType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '串行', value: '1', name: 'Sequence' },
    { label: '并行', value: '2', name: 'Parallel' }
  ]
});

const compareEntry = makeSelectOnExtension({
  id: 'sf-ms-compare',
  label: '通过率类型',
  extensionType: 'sf:MultiSignDetail',
  prop: 'compareType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '按人数', value: '1', name: 'Count' },
    { label: '按百分比', value: '2', name: 'Percentage' }
  ]
});

const orderEntry = makeTextOnExtension({
  id: 'sf-ms-order',
  label: '通过值',
  extensionType: 'sf:MultiSignDetail',
  prop: 'completeOrder'
});

const LOW_PRIORITY = 500;

function SfMultiSignPropertiesProvider(this: { getGroups: Function }, propertiesPanel: { registerProvider: Function }) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask')) {
        groups.push({
          id: 'sf-multisign',
          label: '会签',
          entries: [complexEntry(element), mergeEntry(element), compareEntry(element), orderEntry(element)]
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfMultiSignPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfMultiSignPropertiesProviderModule = {
  __init__: ['sfMultiSignPropertiesProvider'],
  sfMultiSignPropertiesProvider: ['type', SfMultiSignPropertiesProvider]
};
