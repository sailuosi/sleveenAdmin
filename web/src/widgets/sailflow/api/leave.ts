import request from '@/utils/requestAPI6.js';

export type LeaveRow = {
  Id?: string;
  UserId?: string;
  UserName?: string;
  Days?: number;
  Reason?: string;
  StartDate?: string;
  EndDate?: string;
  Status?: number;
  ProcessId?: string;
  ProcessVersion?: string;
  ProcessInstanceId?: string;
  CreatedAt?: string;
};

export function ensureLeaveTable() {
  return request({ url: '/api/WfLeave/EnsureTable', method: 'post' });
}

export function createLeave(data: {
  UserId?: string;
  UserName?: string;
  Days: number;
  Reason?: string;
  StartDate?: string;
  EndDate?: string;
  ProcessId: string;
  Version?: string;
}) {
  return request({
    url: '/api/WfLeave/Create',
    method: 'post',
    data
  });
}

export function queryLeave(data: { UserId?: string; Limit?: number }) {
  return request({
    url: '/api/WfLeave/Query',
    method: 'post',
    data
  });
}

export function getLeave(id: string) {
  return request({
    url: `/api/WfLeave/Get/${id}`,
    method: 'get'
  });
}

export function leaveStatusLabel(status?: number) {
  switch (Number(status)) {
    case 2:
      return '已通过';
    case 3:
      return '已驳回';
    case 4:
      return '已关闭';
    case 1:
    default:
      return '审批中';
  }
}
