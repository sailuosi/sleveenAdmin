/** 我的委托（委托代批 · sf_surrogate） */
import request from '@/utils/requestAPI6.js';

export type SailSurrogateRow = {
  id?: string;
  Id?: string;
  grantorId?: string;
  GrantorId?: string;
  grantorName?: string;
  GrantorName?: string;
  agentId?: string;
  AgentId?: string;
  agentName?: string;
  AgentName?: string;
  processId?: string | null;
  ProcessId?: string | null;
  enabled?: number;
  Enabled?: number;
  startAt?: string | null;
  StartAt?: string | null;
  endAt?: string | null;
  EndAt?: string | null;
  weight?: number;
  Weight?: number;
  updatedAt?: string | null;
  UpdatedAt?: string | null;
  createdAt?: string | null;
  CreatedAt?: string | null;
};

export type SurrogateQuery = {
  pageIndex?: number;
  pageSize?: number;
  processId?: string | null;
  enabled?: number | null;
};

export function queryMySurrogatePage(data?: SurrogateQuery) {
  return request({
    url: '/api/WfSurrogate/MyPage',
    method: 'post',
    data: data || {}
  });
}

export function saveSurrogate(row: Partial<SailSurrogateRow>) {
  return request({
    url: '/api/WfSurrogate/Save',
    method: 'post',
    data: row
  });
}

export function setSurrogateEnabled(id: string, enabled: boolean) {
  return request({
    url: `/api/WfSurrogate/SetEnabled/${encodeURIComponent(id)}`,
    method: 'post',
    params: { enabled: enabled ? 1 : 0 }
  });
}

export function deleteSurrogate(id: string) {
  return request({
    url: `/api/WfSurrogate/Delete/${encodeURIComponent(id)}`,
    method: 'post'
  });
}
