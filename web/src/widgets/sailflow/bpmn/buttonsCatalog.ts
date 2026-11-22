/**
 * Sail 办理按钮码表（与引擎命令 / Host API 对齐）。
 * 设计器 sf:ButtonsSetting.json 与办理条共用。
 */

export type SailButtonContext = 'handle' | 'done' | 'admin';

export type SailButtonDef = {
  id: string;
  displayName: string;
  /** 引擎或 Host 命令 */
  engine: string;
  contexts: SailButtonContext[];
  defaultEnable: boolean;
  /** 暂未产品化完整 UX 时仍可配置启用 */
  note?: string;
};

export type SailButtonSetting = {
  id: string;
  enable: boolean;
  displayName: string;
};

/** 全量 OA 办理按钮（对齐 IWorkflowService + Host 产品能力） */
export const SAIL_BUTTON_CATALOG: SailButtonDef[] = [
  {
    id: 'Approve',
    displayName: '通过',
    engine: 'RunProcess',
    contexts: ['handle'],
    defaultEnable: true
  },
  {
    id: 'Reject',
    displayName: '驳回',
    engine: 'RejectProcess',
    contexts: ['handle'],
    defaultEnable: true
  },
  {
    id: 'SendBack',
    displayName: '退回',
    engine: 'SendBackProcess',
    contexts: ['handle'],
    defaultEnable: true
  },
  {
    id: 'SignForward',
    displayName: '加签',
    engine: 'SignForwardProcess',
    contexts: ['handle'],
    defaultEnable: true
  },
  {
    id: 'RemoveAssignee',
    displayName: '减签',
    engine: 'RemoveAssigneeProcess',
    contexts: ['handle'],
    defaultEnable: true,
    note: '从当前多人节点移除未办结办理人；不可减最后一人'
  },
  {
    id: 'ChangeAssignee',
    displayName: '改派',
    engine: 'ChangeAssigneeProcess',
    contexts: ['handle', 'admin'],
    defaultEnable: false,
    note: '管理向：NextActivityPerformers=新办理人全集（差量增删）'
  },
  {
    id: 'AddAssignee',
    displayName: '加人',
    engine: 'AddAssigneeProcess',
    contexts: ['handle', 'admin'],
    defaultEnable: false,
    note: '扁节点追加普通办理人（增量）；也可经加签对话框「普通加人」路由；不压加签栈'
  },
  {
    id: 'Urge',
    displayName: '催办',
    engine: 'UrgeProcess',
    contexts: ['handle', 'admin'],
    defaultEnable: false,
    note: '不改待办状态；空目标=催本活动全部打开待办'
  },
  {
    id: 'Transfer',
    displayName: '转办',
    engine: 'TransferProcess',
    contexts: ['handle'],
    defaultEnable: true,
    note: '个人办结+本节点交人，不流转；办完不还回'
  },
  {
    id: 'Delegate',
    displayName: '委派',
    engine: 'DelegateProcess',
    contexts: ['handle'],
    defaultEnable: false,
    note: '请他人代办；受托人办结后自动回转原人；≠ 转办'
  },
  {
    id: 'Consult',
    displayName: '沟通',
    engine: 'ConsultProcess',
    contexts: ['handle'],
    defaultEnable: true,
    note: '建参与者待办；锁定发起人审批直至全员阅办或收回'
  },
  {
    id: 'Acknowledge',
    displayName: '阅办',
    engine: 'AcknowledgeProcess',
    contexts: ['handle'],
    defaultEnable: true,
    note: '参与者关闭自己的沟通待办；运行态按 AssigneeRole 显隐'
  },
  {
    id: 'RecallConsult',
    displayName: '收回沟通',
    engine: 'RecallConsultProcess',
    contexts: ['handle'],
    defaultEnable: true,
    note: '发起人取消未反馈参与者；仅 ConsultPending 时露出'
  },
  {
    id: 'Copyto',
    displayName: '抄送',
    engine: 'WfCopyto/Create',
    contexts: ['handle'],
    defaultEnable: true,
    note: 'Host 产品表，非引擎内置；抄送=copyto，禁止裸 Copy'
  },
  {
    id: 'Withdraw',
    displayName: '撤回',
    engine: 'WithdrawProcess',
    contexts: ['done'],
    defaultEnable: true
  },
  {
    id: 'Resend',
    displayName: '重发',
    engine: 'ResendProcess',
    contexts: ['handle', 'done'],
    defaultEnable: true,
    note: '有 BackSrc 时作「返送退回人」（§G）；文案由办理台覆盖'
  },
  {
    id: 'Jump',
    displayName: '跳转',
    engine: 'JumpProcess',
    contexts: ['admin'],
    defaultEnable: true
  },
  {
    id: 'Close',
    displayName: '关闭',
    engine: 'CloseProcess',
    contexts: ['admin'],
    defaultEnable: true
  },
  {
    id: 'Terminate',
    displayName: '终止',
    engine: 'TerminateProcess',
    contexts: ['admin'],
    defaultEnable: false,
    note: '§B 强行结束运行实例；默认关，旧图不启用'
  },
  {
    id: 'Discard',
    displayName: '作废',
    engine: 'DiscardProcess',
    contexts: ['admin'],
    defaultEnable: false,
    note: '§B 作废实例；默认关；≠ Close/Terminate'
  },
  {
    id: 'Revise',
    displayName: '修订',
    engine: 'ReviseProcess',
    contexts: ['handle', 'admin'],
    defaultEnable: false,
    note: '默认关闭；节点按需开启'
  },
  {
    id: 'Reverse',
    displayName: '反转',
    engine: 'ReverseProcess',
    contexts: ['admin'],
    defaultEnable: false,
    note: '与退回语义不同；默认关闭'
  }
];

/** 设计器分组：待办办理 | 其他（实例/已办/管理） */
export type SailButtonDesignerGroup = 'handle' | 'other';

export function buttonDesignerGroup(def: SailButtonDef): SailButtonDesignerGroup {
  if (def.contexts.every(c => c === 'handle')) return 'handle';
  // Resend 主场景在办理/返送 → 待办组
  if (def.id === 'Resend') return 'handle';
  return 'other';
}

export function defaultButtonsSetting(): SailButtonSetting[] {
  return SAIL_BUTTON_CATALOG.map(b => ({
    id: b.id,
    enable: b.defaultEnable,
    displayName: b.displayName
  }));
}

export function mergeButtonsSetting(
  raw: SailButtonSetting[] | null | undefined
): SailButtonSetting[] {
  const map = new Map((raw || []).map(x => [x.id, x]));
  return SAIL_BUTTON_CATALOG.map(def => {
    const hit = map.get(def.id);
    return {
      id: def.id,
      enable: hit?.enable ?? def.defaultEnable,
      displayName: (hit?.displayName || def.displayName).trim() || def.displayName
    };
  });
}

export function parseButtonsJson(json: string | null | undefined): SailButtonSetting[] {
  if (!json?.trim()) return defaultButtonsSetting();
  try {
    const arr = JSON.parse(json) as SailButtonSetting[];
    if (!Array.isArray(arr)) return defaultButtonsSetting();
    return mergeButtonsSetting(arr);
  } catch {
    return defaultButtonsSetting();
  }
}

export function isButtonEnabled(
  settings: SailButtonSetting[] | null | undefined,
  id: string
): boolean {
  const list = mergeButtonsSetting(settings);
  return list.find(b => b.id === id)?.enable !== false;
}

export function buttonDisplayName(
  settings: SailButtonSetting[] | null | undefined,
  id: string
): string {
  const list = mergeButtonsSetting(settings);
  return list.find(b => b.id === id)?.displayName || id;
}
