/**
 * Sailflow 运行态 API（待办 / 发起 / 办理）
 */
import request from '@/utils/requestAPI6.js';

export type FlowCommandContextPayload = {
  UserId?: string;
  UserName?: string;
  AppName?: string;
  AppInstanceId?: string;
  AppInstanceCode?: string;
  ProcessId?: string;
  ProcessCode?: string;
  Version?: string;
  TaskId?: string | null;
  ActivityInstanceId?: string | null;
  ProcessInstanceId?: string | null;
  Conditions?: Record<string, string>;
  NextActivityPerformers?: Record<
    string,
    Array<{
      UserId: string;
      UserName: string;
      GroupId?: string;
      GroupRole?: string;
      GroupName?: string;
      UnitId?: string;
      UnitCode?: string;
      UnitName?: string;
    }>
  >;
  NextPerformerType?: number;
  /** 发起单位（写入流程实例 StartUnit*） */
  StartUnitId?: string;
  StartUnitCode?: string;
  StartUnitName?: string;
  [key: string]: unknown;
};

export type TaskQueryPayload = {
  UserId?: string;
  EndedUserId?: string;
  AppName?: string;
  ProcessId?: string;
  Version?: string;
  AppInstanceId?: string;
  PageIndex?: number;
  PageSize?: number;
};

export type ProcessInstanceQueryPayload = {
  CreatedUserId?: string;
  ProcessId?: string;
  AppName?: string;
  ProcessState?: number | null;
  PageIndex?: number;
  PageSize?: number;
};

/** POST api/Wf2Xml/QueryReadyTasks */
export function queryReadyTasks(data: TaskQueryPayload) {
  return request({
    url: '/api/Wf2Xml/QueryReadyTasks',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/QueryCompletedTasks */
export function queryCompletedTasks(data: TaskQueryPayload) {
  return request({
    url: '/api/Wf2Xml/QueryCompletedTasks',
    method: 'post',
    data
  });
}

/** GET api/Wf2Xml/GetTaskView/{id} */
export function getTaskView(id: string) {
  return request({
    url: `/api/Wf2Xml/GetTaskView/${id}`,
    method: 'get'
  });
}

/** POST api/Wf2Xml/GetApprovalWorkbench */
export function getApprovalWorkbench(data: {
  TaskId?: string | null;
  ProcessInstanceId?: string | null;
  FormId?: string;
  Conditions?: Record<string, string>;
}) {
  return request({
    url: '/api/Wf2Xml/GetApprovalWorkbench',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/GetNextActivityRoleUserTree */
export function getNextActivityRoleUserTree(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/GetNextActivityRoleUserTree',
    method: 'post',
    data
  });
}

export type QueryNextActivityCandidatesPayload = {
  ProcessId?: string;
  Version?: string;
  ActivityId?: string;
  TaskId?: string;
  Keyword?: string;
  Page?: number;
  PageSize?: number;
  RoleIds?: string[];
  CandidateMode?: string;
  CandidateSelectorType?: string;
};

/** POST api/Wf2Xml/QueryNextActivityCandidates · 选人分页（与下一节点树拆分） */
export function queryNextActivityCandidates(data: QueryNextActivityCandidatesPayload) {
  return request({
    url: '/api/Wf2Xml/QueryNextActivityCandidates',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/GetPreviousActivityTree */
export function getPreviousActivityTree(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/GetPreviousActivityTree',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/QueryProcessInstances */
export function queryProcessInstances(data: ProcessInstanceQueryPayload) {
  return request({
    url: '/api/Wf2Xml/QueryProcessInstances',
    method: 'post',
    data
  });
}

/** GET api/Wf2Xml/GetProcessInstance/{id} */
export function getProcessInstance(id: string) {
  return request({
    url: `/api/Wf2Xml/GetProcessInstance/${id}`,
    method: 'get'
  });
}

/** POST api/Wf2Xml/StartProcess */
export function startProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/StartProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/RunProcess */
export function runProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/RunProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/RejectProcess */
export function rejectProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/RejectProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/SendBackProcess */
export function sendBackProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/SendBackProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/WithdrawProcess */
export function withdrawProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/WithdrawProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/ResendProcess */
export function resendProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/ResendProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/SignForwardProcess */
export function signForwardProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/SignForwardProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/TransferProcess · 转办（本节点交人，不流转） */
export function transferProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/TransferProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/ButtonsOperation · 统一办理入口（码表 Id → Handler） */
export function buttonsOperation(data: {
  ButtonId: string;
  Runner?: FlowCommandContextPayload;
  [key: string]: unknown;
}) {
  return request({
    url: '/api/Wf2Xml/ButtonsOperation',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/CloseProcess */
export function closeProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/CloseProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/TerminateProcess · §B */
export function terminateProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/TerminateProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/DiscardProcess · §B */
export function discardProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/DiscardProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/JumpProcess */
export function jumpProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/JumpProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/ReviseProcess */
export function reviseProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/ReviseProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/ReverseProcess */
export function reverseProcess(data: FlowCommandContextPayload) {
  return request({
    url: '/api/Wf2Xml/ReverseProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/GetTaskActivityList · 跳转候选人工任务 */
export function getTaskActivityList(data: { ProcessId?: string; Version?: string }) {
  return request({
    url: '/api/Wf2Xml/GetTaskActivityList',
    method: 'post',
    data
  });
}

/** POST api/WfCopy/Create */
export function createProcessCopy(data: Record<string, unknown>) {
  return request({
    url: '/api/WfCopy/Create',
    method: 'post',
    data
  });
}

/** POST api/WfCopy/MyPage */
export function queryMyCopyPage(data: { UserId?: string; PageIndex?: number; PageSize?: number }) {
  return request({
    url: '/api/WfCopy/MyPage',
    method: 'post',
    data
  });
}
