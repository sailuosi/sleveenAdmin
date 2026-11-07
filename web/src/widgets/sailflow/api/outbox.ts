import request from '@/utils/requestAPI6.js';

export function queryOutbox(data: {
  Status?: string;
  ProcessCode?: string;
  HandlerName?: string;
  Limit?: number;
}) {
  return request({
    url: '/api/WfOutbox/Query',
    method: 'post',
    data
  });
}

export function countOutboxByStatus(status = 'Dead') {
  return request({
    url: '/api/WfOutbox/CountByStatus',
    method: 'get',
    params: { status }
  });
}

export function resetOutboxDead(id: number | string) {
  return request({
    url: `/api/WfOutbox/ResetDead/${id}`,
    method: 'post'
  });
}

export function resetOutboxDeadBatch(ids: number[]) {
  return request({
    url: '/api/WfOutbox/ResetDeadBatch',
    method: 'post',
    data: { Ids: ids }
  });
}

export function resetAllOutboxDead(limit = 500) {
  return request({
    url: `/api/WfOutbox/ResetAllDead?limit=${limit}`,
    method: 'post'
  });
}
