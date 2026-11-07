import { is } from 'bpmn-js/lib/util/ModelUtil';
import { makeSelectOnExtension } from '../utils/panelEntries';
import { isJoinTarget, isSplitSource } from '../utils/sfModelUtility';

const splitEntry = makeSelectOnExtension({
  id: 'sf-split-type',
  label: '扩展分支类型',
  extensionType: 'sf:GatewayDetail',
  prop: 'extraSplitType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '审批或分支', value: '1', name: 'ApprovalOrSplit' },
    { label: '多实例与分支', value: '2', name: 'AndSplitMI' }
  ]
});

const joinEntry = makeSelectOnExtension({
  id: 'sf-join-type',
  label: '扩展合并类型',
  extensionType: 'sf:GatewayDetail',
  prop: 'extraJoinType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '或合并', value: '1', name: 'EOrJoin' },
    { label: '多实例与合并', value: '2', name: 'AndJoinMI' }
  ]
});

const passEntry = makeSelectOnExtension({
  id: 'sf-pass-type',
  label: '合并通过类型',
  extensionType: 'sf:GatewayDetail',
  prop: 'joinPassType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '按分支数', value: '1', name: 'Count' },
    { label: '强制分支', value: '2', name: 'Forced' }
  ]
});

const LOW_PRIORITY = 500;

function SfGatewayPropertiesProvider(this: { getGroups: Function }, propertiesPanel: { registerProvider: Function }) {
  this.getGroups = function (element: { type?: string; incoming?: unknown[]; outgoing?: unknown[] }) {
    return function (groups: object[]) {
      if (
        is(element, 'bpmn:InclusiveGateway') ||
        is(element, 'bpmn:ExclusiveGateway') ||
        is(element, 'bpmn:ParallelGateway')
      ) {
        const entries: object[] = [];
        if (isSplitSource(element)) entries.push(splitEntry(element));
        if (isJoinTarget(element)) {
          entries.push(joinEntry(element));
          entries.push(passEntry(element));
        }
        groups.push({ id: 'sf-gateway', label: '网关', entries });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfGatewayPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfGatewayPropertiesProviderModule = {
  __init__: ['sfGatewayPropertiesProvider'],
  sfGatewayPropertiesProvider: ['type', SfGatewayPropertiesProvider]
};
