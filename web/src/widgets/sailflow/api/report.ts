import request from '@/utils/requestAPI6.js';

export type ProcessInstanceRankItem = {
  ProcessId?: string;
  ProcessName?: string;
  Version?: string;
  InstanceCount?: number;
};

export type WorkflowReportOverview = {
  ProcessDefinitionTotal?: number;
  ProcessDefinitionEnabled?: number;
  ProcessDefinitionDisabled?: number;
  InstanceTotal?: number;
  InstanceRunning?: number;
  InstanceCompleted?: number;
  InstanceSuspended?: number;
  InstanceOther?: number;
  TaskReady?: number;
  TaskCompletedRecent?: number;
  OutboxDead?: number;
  LeaveRunning?: number;
  TopProcesses?: ProcessInstanceRankItem[];
};

export function getReportOverview(top = 10) {
  return request({
    url: '/api/WfReport/Overview',
    method: 'get',
    params: { top }
  });
}
