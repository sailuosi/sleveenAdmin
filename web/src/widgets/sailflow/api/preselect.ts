import request from '@/utils/request';

/** 与后端 PreselectPerformerDto 对齐 */
export type PreselectPerformerDto = {
  UserId: string;
  UserName?: string;
  GroupId?: string;
  GroupName?: string;
  GroupRole?: string;
  UnitId?: string;
  UnitCode?: string;
  UnitName?: string;
};

export type PreselectTargetDto = {
  ActivityId: string;
  ActivityName: string;
  Required?: boolean;
  AllowMulti?: boolean;
  ParamsJson?: string | null;
};

export type PreselectNextProbeDto = {
  ActivityId: string;
  IsPreselectTarget: boolean;
  HasPerformers: boolean;
  SkipManualPick: boolean;
  Performers: PreselectPerformerDto[];
};

/** GET api/WfPreselect/ListTargets */
export function listPreselectTargets(processId: string, version = '1') {
  return request({
    url: '/api/WfPreselect/ListTargets',
    method: 'get',
    params: { processId, version }
  });
}

/** GET api/WfPreselect/ProbeNext */
export function probePreselectNext(params: {
  processId?: string;
  version?: string;
  processInstanceId?: string;
  activityId: string;
}) {
  return request({
    url: '/api/WfPreselect/ProbeNext',
    method: 'get',
    params
  });
}

/** GET api/WfPreselect/Get */
export function getPreselectAssignees(processInstanceId: string, activityId: string) {
  return request({
    url: '/api/WfPreselect/Get',
    method: 'get',
    params: { processInstanceId, activityId }
  });
}

/** POST api/WfPreselect/Replace */
export function replacePreselectAssignees(data: {
  ProcessInstanceId: string;
  Performers: Record<string, PreselectPerformerDto[]>;
}) {
  return request({
    url: '/api/WfPreselect/Replace',
    method: 'post',
    data
  });
}

/** POST api/WfPreselect/Clear */
export function clearPreselectAssignees(data: {
  ProcessInstanceId: string;
  ActivityId?: string;
}) {
  return request({
    url: '/api/WfPreselect/Clear',
    method: 'post',
    data
  });
}
