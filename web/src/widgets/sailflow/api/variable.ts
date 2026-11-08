import request from '@/utils/requestAPI6.js';

export type VariableRow = {
  Id?: string;
  ProcessId?: string;
  Version?: string;
  ActivityId?: string;
  Name?: string;
  Type?: string;
  Direction?: string;
  DefaultValue?: string;
  IsRequired?: number | boolean;
  IsReferenced?: number | boolean;
  SourceRef?: string;
  SourceVariableName?: string;
  SortOrder?: number | null;
  Description?: string;
};

export function getVariableList(processId: string, version: string, activityId: string) {
  return request({
    url: '/api/WfVariable/GetList',
    method: 'get',
    params: { processId, version, activityId }
  });
}

export function saveVariableList(
  processId: string,
  version: string,
  activityId: string,
  list: VariableRow[]
) {
  return request({
    url: '/api/WfVariable/SaveList',
    method: 'post',
    params: { processId, version, activityId },
    data: list
  });
}
