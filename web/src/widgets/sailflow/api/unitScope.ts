import request from '@/utils/requestAPI6.js';

export function getUnitScopeContext() {
  return request({ url: '/api/WfUnitScope/Context', method: 'get' });
}

export function copyProcessToUnit(data: {
  SourceProcessId: string;
  SourceVersion?: string;
  TargetUnitId: string;
  ProcessName?: string;
  ProcessCode?: string;
}) {
  return request({ url: '/api/WfUnitScope/CopyToUnit', method: 'post', data });
}

export function getAssigneeConfig(data: { ProcessId: string; Version?: string }) {
  return request({ url: '/api/WfUnitScope/GetAssigneeConfig', method: 'post', data });
}

export function saveAssigneeConfig(data: {
  ProcessId: string;
  Version?: string;
  Nodes: Array<{ ActivityId: string; Strategy: string; ConfigJson?: string }>;
}) {
  return request({ url: '/api/WfUnitScope/SaveAssigneeConfig', method: 'post', data });
}

export function resolveStartDefinition(data: { BusinessType: string; UnitId?: string }) {
  return request({ url: '/api/WfBizStart/Resolve', method: 'post', data });
}

export function startByBusiness(data: {
  BusinessType: string;
  UnitId?: string;
  ProcessId?: string;
  Version?: string;
  AppInstanceCode?: string;
  Conditions?: Record<string, string>;
}) {
  return request({ url: '/api/WfBizStart/Start', method: 'post', data });
}
