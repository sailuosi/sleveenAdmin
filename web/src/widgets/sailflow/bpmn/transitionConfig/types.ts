/** 连接线（SequenceFlow）配置模型，对齐侧栏「转移」+ 芋道流转条件 */

export type SailFlowType = 'normal' | 'default' | 'condition';

export type SailTransitionContext = {
  showApproval: boolean;
  showPriority: boolean;
  showDefaultBranch: boolean;
  showForcedMerge: boolean;
  sourceLabel: string;
  targetLabel: string;
};

export type SailTransitionConfigModel = {
  id: string;
  name: string;
  /** 普通 / 默认 / 条件 */
  flowType: SailFlowType;
  conditionExpression: string;
  /** sf:GroupBehaviours */
  recieverType: string;
  approval: string;
  priority: string;
  defaultBranch: boolean;
  forcedMerge: boolean;
  context: SailTransitionContext;
};

export const RECEIVER_OPTIONS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: '上级', value: '1', name: 'Superior' },
  { label: '同级', value: '2', name: 'Compeer' },
  { label: '下级', value: '3', name: 'Subordinate' }
] as const;

export const APPROVAL_OPTIONS = [
  { label: '同意', value: '1', name: 'Agreed' },
  { label: '拒绝', value: '2', name: 'Refused' }
] as const;

export function defaultTransitionContext(): SailTransitionContext {
  return {
    showApproval: false,
    showPriority: false,
    showDefaultBranch: false,
    showForcedMerge: false,
    sourceLabel: '',
    targetLabel: ''
  };
}

export function defaultTransitionConfig(): SailTransitionConfigModel {
  return {
    id: '',
    name: '',
    flowType: 'normal',
    conditionExpression: '',
    recieverType: '0',
    approval: '1',
    priority: '',
    defaultBranch: false,
    forcedMerge: false,
    context: defaultTransitionContext()
  };
}
