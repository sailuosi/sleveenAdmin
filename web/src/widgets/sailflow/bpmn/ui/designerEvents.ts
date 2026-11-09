import mitt from 'mitt';
import type { VariableRow } from '@sail/api/variable';
import type { ParsedValidateResult } from '@sail/api/processValidate';

export type PerformerDraft = {
  name: string;
  outerId: string;
  outerCode: string;
  outerType: string;
};

export type FormDraft = {
  name: string;
  outerId: string;
  outerCode: string;
};

export type ElementConfigPayload = {
  element: unknown;
  modeling: { updateProperties: (el: unknown, props: object) => void };
  moddle: { create: (type: string, props?: object) => any };
};

export type DesignerEvents = {
  'performer:open': {
    element: unknown;
    onConfirm: (draft: PerformerDraft) => void;
  };
  'form:open': {
    element: unknown;
    onConfirm: (draft: FormDraft) => void;
  };
  'variable:open': {
    activityId: string;
    rows: VariableRow[];
    onSaved: (rows: VariableRow[]) => void;
  };
  'expression:open': {
    value: string;
    onConfirm: (expression: string) => void;
  };
  /** 通知收件人：强制选用户 */
  'notification:open': {
    element: unknown;
    onConfirm: (draft: PerformerDraft) => void;
  };
  /** AI 活动配置（AxConfig） */
  'axconfig:open': {
    activityId: string;
    serviceType?: string;
  };
  'ruleset:pick': {
    currentCode?: string;
    onConfirm: (ruleSetCode: string) => void;
  };
  'subprocess:pick': {
    onConfirm: (draft: SubProcessDraft) => void;
  };
  /** 节点完整配置弹窗（UserTask/Task） */
  'nodeconfig:open': {
    element: unknown;
    modeling: { updateProperties: (el: unknown, props: object) => void };
    moddle: { create: (type: string, props?: object) => any };
  };
  /** 连接线完整配置弹窗（SequenceFlow） */
  'transitionconfig:open': {
    element: unknown;
    modeling: { updateProperties: (el: unknown, props: object) => void };
    moddle: { create: (type: string, props?: object) => any };
  };
  /** 侧栏：结构化编辑 Assignee/Leave Params */
  'assignee-params:open': {
    kind: 'resolve' | 'leave';
    type: string;
    params: string;
    onConfirm: (paramsJson: string) => void;
  };
  'gatewayconfig:open': ElementConfigPayload;
  'servicetaskconfig:open': ElementConfigPayload;
  'scripttaskconfig:open': ElementConfigPayload;
  'ruletaskconfig:open': ElementConfigPayload;
  'subprocessconfig:open': ElementConfigPayload;
  'eventconfig:open': ElementConfigPayload;
  /** 基础 BPMN 属性弹窗（Process / Participant / 无扩展事件等） */
  'basicconfig:open': ElementConfigPayload;
  /** 流程设计修复弹窗：导出当前 XML → 修复 → 回写画布 */
  'repair:open': {
    getXml: () => Promise<string>;
    applyXml: (xml: string) => Promise<void>;
  };
  /** 流程校验结果弹窗 */
  'validate:open': {
    result: ParsedValidateResult;
    onRepair?: () => void;
  };
};

export type SubProcessDraft = {
  subId: string;
  subProcessId: string;
  subProcessName: string;
};

export const designerEvents = mitt<DesignerEvents>();
