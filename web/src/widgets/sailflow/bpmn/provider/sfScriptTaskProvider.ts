import { is } from 'bpmn-js/lib/util/ModelUtil';
import { makeSelectOnNested, makeTextOnNested } from '../utils/panelEntries';

const LOW_PRIORITY = 500;

const scriptTypeEntry = makeSelectOnNested({
  id: 'sf-script-type',
  label: '脚本类型',
  collectionType: 'sf:Scripts',
  listProp: 'scripts',
  itemType: 'sf:Script',
  prop: 'scriptType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: 'SQL', value: '1', name: 'SQL' },
    { label: 'Javascript', value: '2', name: 'Javascript' },
    { label: 'Python', value: '3', name: 'Python' }
  ]
});

const argusEntry = makeTextOnNested({
  id: 'sf-script-argus',
  label: '参数',
  collectionType: 'sf:Scripts',
  listProp: 'scripts',
  itemType: 'sf:Script',
  prop: 'argus'
});

const scriptTextEntry = makeTextOnNested({
  id: 'sf-script-text',
  label: '脚本内容',
  collectionType: 'sf:Scripts',
  listProp: 'scripts',
  itemType: 'sf:Script',
  prop: 'scriptText',
  rows: 6
});

function SfScriptTaskPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!is(element, 'bpmn:ScriptTask')) return groups;
      groups.push({
        id: 'sf-scripttask',
        label: '脚本任务',
        entries: [scriptTypeEntry(element), argusEntry(element), scriptTextEntry(element)]
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfScriptTaskPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfScriptTaskPropertiesProviderModule = {
  __init__: ['sfScriptTaskPropertiesProvider'],
  sfScriptTaskPropertiesProvider: ['type', SfScriptTaskPropertiesProvider]
};
