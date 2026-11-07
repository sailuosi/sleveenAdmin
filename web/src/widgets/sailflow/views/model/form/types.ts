/** 流程编辑表单模型（对齐 n_flowable model/form） */
export type ProcessEditFormModel = {
  processId?: string;
  version: string;
  processName: string;
  processCode: string;
  category?: string;
  icon: string;
  description: string;
  visible: boolean;
  startUserType: number;
  startUserIds: string[];
  startDeptIds: string[];
  managerUserIds: string[];
  /** 10 NORMAL · 20 CUSTOM */
  formType: number;
  formId: string;
  formCustomCreatePath: string;
  formCustomViewPath: string;
  /** 是否已有 BPMN */
  hasBpmn: boolean;
  allowCancelRunningProcess: boolean;
  allowWithdrawTask: boolean;
  processIdRule: {
    enable: boolean;
    prefix: string;
    infix: string;
    postfix: string;
    length: number;
  };
  autoApprovalType: number;
  titleSetting: { enable: boolean; title: string };
  summarySetting: { enable: boolean; summary: string[] };
  processBeforeTriggerSetting: HttpTriggerSetting | null;
  processAfterTriggerSetting: HttpTriggerSetting | null;
  taskBeforeTriggerSetting: HttpTriggerSetting | null;
  taskAfterTriggerSetting: HttpTriggerSetting | null;
  printTemplateSetting: { enable: boolean; template: string };
};

export type HttpTriggerSetting = {
  url: string;
  header: Array<{ key: string; value: string }>;
  body: Array<{ key: string; value: string }>;
  response: Array<{ key: string; value: string }>;
};

export const FORM_TYPE_NORMAL = 10;
export const FORM_TYPE_CUSTOM = 20;

export function createEmptyProcessEditForm(): ProcessEditFormModel {
  return {
    version: '1',
    processName: '',
    processCode: '',
    category: undefined,
    icon: '',
    description: '',
    visible: true,
    startUserType: 0,
    startUserIds: [],
    startDeptIds: [],
    managerUserIds: [],
    formType: FORM_TYPE_NORMAL,
    formId: '',
    formCustomCreatePath: '',
    formCustomViewPath: '',
    hasBpmn: false,
    allowCancelRunningProcess: true,
    allowWithdrawTask: false,
    processIdRule: { enable: false, prefix: '', infix: '', postfix: '', length: 5 },
    autoApprovalType: 0,
    titleSetting: { enable: false, title: '' },
    summarySetting: { enable: false, summary: [] },
    processBeforeTriggerSetting: null,
    processAfterTriggerSetting: null,
    taskBeforeTriggerSetting: null,
    taskAfterTriggerSetting: null,
    printTemplateSetting: { enable: false, template: '' }
  };
}

export function parseIdList(raw: unknown): string[] {
  if (Array.isArray(raw)) return raw.map(String);
  if (typeof raw === 'string' && raw.trim()) {
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) return parsed.map(String);
    } catch {
      return raw.split(/[,;\s]+/).filter(Boolean);
    }
  }
  return [];
}

export function packExtraJson(model: ProcessEditFormModel): string {
  return JSON.stringify({
    allowCancelRunningProcess: model.allowCancelRunningProcess,
    allowWithdrawTask: model.allowWithdrawTask,
    processIdRule: model.processIdRule,
    autoApprovalType: model.autoApprovalType,
    titleSetting: model.titleSetting,
    summarySetting: model.summarySetting,
    processBeforeTriggerSetting: model.processBeforeTriggerSetting,
    processAfterTriggerSetting: model.processAfterTriggerSetting,
    taskBeforeTriggerSetting: model.taskBeforeTriggerSetting,
    taskAfterTriggerSetting: model.taskAfterTriggerSetting,
    printTemplateSetting: model.printTemplateSetting
  });
}

export function applyExtraJson(model: ProcessEditFormModel, extraJson: unknown) {
  let extra: Record<string, unknown> = {};
  if (typeof extraJson === 'string' && extraJson.trim()) {
    try {
      extra = JSON.parse(extraJson);
    } catch {
      extra = {};
    }
  } else if (extraJson && typeof extraJson === 'object') {
    extra = extraJson as Record<string, unknown>;
  }
  if (typeof extra.allowCancelRunningProcess === 'boolean') {
    model.allowCancelRunningProcess = extra.allowCancelRunningProcess;
  }
  if (typeof extra.allowWithdrawTask === 'boolean') {
    model.allowWithdrawTask = extra.allowWithdrawTask;
  }
  if (extra.processIdRule && typeof extra.processIdRule === 'object') {
    model.processIdRule = { ...model.processIdRule, ...(extra.processIdRule as object) };
  }
  if (typeof extra.autoApprovalType === 'number') {
    model.autoApprovalType = extra.autoApprovalType;
  }
  if (extra.titleSetting && typeof extra.titleSetting === 'object') {
    model.titleSetting = { ...model.titleSetting, ...(extra.titleSetting as object) };
  }
  if (extra.summarySetting && typeof extra.summarySetting === 'object') {
    model.summarySetting = { ...model.summarySetting, ...(extra.summarySetting as object) };
  }
  model.processBeforeTriggerSetting = (extra.processBeforeTriggerSetting as HttpTriggerSetting) || null;
  model.processAfterTriggerSetting = (extra.processAfterTriggerSetting as HttpTriggerSetting) || null;
  model.taskBeforeTriggerSetting = (extra.taskBeforeTriggerSetting as HttpTriggerSetting) || null;
  model.taskAfterTriggerSetting = (extra.taskAfterTriggerSetting as HttpTriggerSetting) || null;
  if (extra.printTemplateSetting && typeof extra.printTemplateSetting === 'object') {
    model.printTemplateSetting = {
      ...model.printTemplateSetting,
      ...(extra.printTemplateSetting as object)
    };
  }
}
