export type ElementConfigKind =
  | 'gateway'
  | 'service'
  | 'script'
  | 'rule'
  | 'subprocess'
  | 'event'
  | 'basic'
  | 'process';

export type SailGatewayModel = {
  id: string;
  name: string;
  documentation: string;
  extraSplitType: string;
  extraJoinType: string;
  joinPassType: string;
  showSplit: boolean;
  showJoin: boolean;
};

export type SailServiceModel = {
  id: string;
  name: string;
  documentation: string;
  methodType: string;
  subMethodType: string;
  argus: string;
  expression: string;
  aiType: string;
  configUUID: string;
  toolSetName: string;
};

export type SailScriptModel = {
  id: string;
  name: string;
  documentation: string;
  scriptType: string;
  argus: string;
  scriptText: string;
};

export type SailRuleModel = {
  id: string;
  name: string;
  documentation: string;
  ruleSetCode: string;
};

export type SailSubProcessModel = {
  id: string;
  name: string;
  documentation: string;
  subProcessId: string;
  subProcessName: string;
};

export type SailEventModel = {
  id: string;
  name: string;
  documentation: string;
  eventDefType: string;
  timerKind: 'timeDate' | 'timeDuration' | 'timeCycle';
  timerBody: string;
  condition: string;
  messageName: string;
  signalName: string;
  errorName: string;
  escalationName: string;
  linkName: string;
};

export type SailBasicPropsModel = {
  id: string;
  name: string;
  documentation: string;
  type: string;
  /** Process only */
  isExecutable?: boolean;
  /** Participant only */
  processRef?: string;
  processName?: string;
};

export const METHOD_OPTS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: 'WebApi', value: '1', name: 'WebApi' },
  { label: 'LocalService', value: '2', name: 'LocalService' },
  { label: 'CSharpLibrary', value: '3', name: 'CSharpLibrary' },
  { label: 'SQLStoreProcedure', value: '4', name: 'SQLStoreProcedure' }
];

export const HTTP_OPTS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: 'HttpGet', value: '1', name: 'HttpGet' },
  { label: 'HttpPost', value: '2', name: 'HttpPost' },
  { label: 'HttpPut', value: '3', name: 'HttpPut' },
  { label: 'HttpDelete', value: '4', name: 'HttpDelete' }
];

export const SCRIPT_OPTS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: 'SQL', value: '1', name: 'SQL' },
  { label: 'Javascript', value: '2', name: 'Javascript' },
  { label: 'Python', value: '3', name: 'Python' }
];

export const SPLIT_OPTS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: '审批或分支', value: '1', name: 'ApprovalOrSplit' },
  { label: '多实例与分支', value: '2', name: 'AndSplitMI' }
];

export const JOIN_OPTS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: '或合并', value: '1', name: 'EOrJoin' },
  { label: '多实例与合并', value: '2', name: 'AndJoinMI' }
];

export const PASS_OPTS = [
  { label: '默认', value: '0', name: 'Optiondefault' },
  { label: '按分支数', value: '1', name: 'Count' },
  { label: '强制分支', value: '2', name: 'Forced' }
];

export const AI_OPTS = [
  { label: '（无）', value: '', name: '' },
  { label: 'LLM', value: '1', name: 'LLM' },
  { label: 'RAG', value: '2', name: 'RAG' },
  { label: 'Agent', value: '3', name: 'Agent' }
];
