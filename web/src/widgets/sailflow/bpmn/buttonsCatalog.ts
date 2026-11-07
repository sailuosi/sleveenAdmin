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
    id: 'Transfer',
    displayName: '转办',
    engine: 'TransferProcess',
    contexts: ['handle'],
    defaultEnable: true,
    note: '个人办结+本节点交人，不流转'
  },
  {
    id: 'Copy',
    displayName: '抄送',
    engine: 'WfCopy/Create',
    contexts: ['handle'],
    defaultEnable: true,
    note: 'Host 产品表，非引擎内置'
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
