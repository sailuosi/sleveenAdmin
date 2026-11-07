import { is } from 'bpmn-js/lib/util/ModelUtil';
import {
  makeCheckboxOnExtension,
  makeConditionExpressionEntry,
  makeSelectOnExtension,
  makeTextOnExtension
} from '../utils/panelEntries';
import { isApprovalOrSplit, isEOrJoin, isOrSplit, isXOrSplit } from '../utils/sfModelUtility';

const conditionEntry = makeConditionExpressionEntry();

const receiverEntry = makeSelectOnExtension({
  id: 'sf-receiver',
  label: '接收者类型',
  extensionType: 'sf:GroupBehaviours',
  prop: 'recieverType',
  options: [
    { label: '默认', value: '0', name: 'Optiondefault' },
    { label: '上级', value: '1', name: 'Superior' },
    { label: '同级', value: '2', name: 'Compeer' },
    { label: '下级', value: '3', name: 'Subordinate' }
  ]
});

const approvalEntry = makeSelectOnExtension({
  id: 'sf-approval',
  label: '审批选项',
  extensionType: 'sf:GroupBehaviours',
  prop: 'approval',
  options: [
    { label: '同意', value: '1', name: 'Agreed' },
    { label: '拒绝', value: '2', name: 'Refused' }
  ]
});

const priorityEntry = makeTextOnExtension({
  id: 'sf-priority',
  label: '优先级',
  description: '数字越小优先级越高',
  extensionType: 'sf:GroupBehaviours',
  prop: 'priority'
});

const defaultBranchEntry = makeCheckboxOnExtension({
  id: 'sf-default-branch',
  label: '默认分支',
  extensionType: 'sf:GroupBehaviours',
  prop: 'defaultBranch'
});

const forcedMergeEntry = makeCheckboxOnExtension({
  id: 'sf-forced-merge',
  label: '强制合并分支',
  description: '或合并时强制通过该分支',
  extensionType: 'sf:GroupBehaviours',
  prop: 'forcedMerge'
});

const LOW_PRIORITY = 500;

function SfTransitionPropertiesProvider(this: { getGroups: Function }, propertiesPanel: { registerProvider: Function }) {
  this.getGroups = function (element: { source?: any; target?: any }) {
    return function (groups: object[]) {
      if (!is(element, 'bpmn:SequenceFlow')) return groups;

      const entries: object[] = [conditionEntry(element), receiverEntry(element)];
      const source = element.source;
      const target = element.target;

      if (source && isApprovalOrSplit(source)) entries.push(approvalEntry(element));
      else if (source && isXOrSplit(source)) entries.push(priorityEntry(element));
      else if (source && isOrSplit(source)) entries.push(defaultBranchEntry(element));
      else if (target && isEOrJoin(target)) entries.push(forcedMergeEntry(element));

      groups.push({ id: 'sf-transition', label: '转移', entries });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfTransitionPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfTransitionPropertiesProviderModule = {
  __init__: ['sfTransitionPropertiesProvider'],
  sfTransitionPropertiesProvider: ['type', SfTransitionPropertiesProvider]
};
