import type { SailButtonSetting } from '../buttonsCatalog';

/** 对照 n_flowable 产品策略，存 sf:NodePolicy.json（引擎不解释，Host/办理台消费） */
export type SailNodePolicy = {
  /** 1人工 2自动通过 3自动拒绝 */
  approveType: number;
  /** 拒绝：1结束流程 2驳回到指定节点 */
  rejectHandlerType: number;
  returnNodeId: string;
  /** 审批人为空：1自动通过 2自动拒绝（产品默认） 3指定人 4转管理员 */
  assignEmptyHandlerType: number;
  assignEmptyUserIds: string[];
  /** 与发起人相同：1自己审 2跳过 3按 assignStartResolveType 换人 */
  assignStartUserHandlerType: number;
  /** 同人=3：主管道 Resolve Type（如 deptBoss） */
  assignStartResolveType: string;
  /** 同人=3：Resolve Params JSON */
  assignStartResolveParams: string;
  signEnable: boolean;
  reasonRequire: boolean;
  skipExpression: string;
  timeoutHandlerEnable: boolean;
  /** 1提醒 2自动同意 3自动拒绝 */
  timeoutHandlerType: number;
  timeDuration: number;
  /** 1分 2时 3天 */
  timeUnit: number;
  maxRemindCount: number;
  /**
   * §J 维① 退回目标：Prev | Starter | History
   * 默认 Starter（对齐 AntFlow 常见默认）
   */
  sendBackTarget: 'Prev' | 'Starter' | 'History';
  /**
   * §J 维② 再发送：ForwardNormal | ReturnToSender
   * 默认 ReturnToSender
   */
  resubmitMode: 'ForwardNormal' | 'ReturnToSender';
  /**
   * §C 字段编辑性：field / title / permission
   * permission = editable | readonly | hidden | masked（旧 READ/WRITE/NONE 加载时归一）
   */
  fieldsPermission: Array<{ field: string; title: string; permission: string }>;
};

export type SailNodeFormRef = {
  name: string;
  outerId: string;
  outerCode: string;
};

export type SailNodePerformerRef = {
  name: string;
  outerId: string;
  outerCode: string;
  outerType: string;
};

export type SailNodeNotifyRef = SailNodePerformerRef;

export type SailNodeActionRef = {
  fireType: string;
  methodType: string;
  subMethodType: string;
  argus: string;
  expression: string;
};

export type SailNodeConfigModel = {
  id: string;
  name: string;
  code: string;
  url: string;
  /** 待办人定范围 */
  assigneeResolveType: string;
  assigneeResolveParams: string;
  /** 办理模式 */
  leaveJudgeType: string;
  leaveJudgeParams: string;
  /** 会签 */
  multiSign: {
    complexType: string;
    mergeType: string;
    compareType: string;
    completeOrder: string;
  };
  performers: SailNodePerformerRef[];
  forms: SailNodeFormRef[];
  notifications: SailNodeNotifyRef[];
  actions: SailNodeActionRef[];
  buttons: SailButtonSetting[];
  policy: SailNodePolicy;
};

export function defaultNodePolicy(): SailNodePolicy {
  return {
    approveType: 1,
    rejectHandlerType: 1,
    returnNodeId: '',
    assignEmptyHandlerType: 2,
    assignEmptyUserIds: [],
    assignStartUserHandlerType: 1,
    assignStartResolveType: 'deptBoss',
    assignStartResolveParams: '',
    signEnable: false,
    reasonRequire: false,
    skipExpression: '',
    timeoutHandlerEnable: false,
    timeoutHandlerType: 1,
    timeDuration: 6,
    timeUnit: 2,
    maxRemindCount: 1,
    sendBackTarget: 'Starter',
    resubmitMode: 'ReturnToSender',
    fieldsPermission: []
  };
}
