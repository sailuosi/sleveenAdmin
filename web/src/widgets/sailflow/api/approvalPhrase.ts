/** 我的审批常用语 · sf_approval_phrase */
import { request } from '@/utils/requestAPI6.js';

export type ApprovalPhraseRow = {
  Id?: string;
  id?: string;
  UserId?: string;
  userId?: string;
  Content?: string;
  content?: string;
  SortOrder?: number;
  sortOrder?: number;
  Enabled?: number;
  enabled?: number;
};

export function listMyApprovalPhrases(enabledOnly = true) {
  return request({
    url: '/api/WfApprovalPhrase/MyList',
    method: 'get',
    params: { enabledOnly: enabledOnly ? 1 : 0 }
  });
}

export function saveApprovalPhrase(row: Partial<ApprovalPhraseRow>) {
  return request({
    url: '/api/WfApprovalPhrase/Save',
    method: 'post',
    data: row
  });
}

export function deleteApprovalPhrase(id: string) {
  return request({
    url: `/api/WfApprovalPhrase/Delete/${encodeURIComponent(id)}`,
    method: 'post'
  });
}

export function updateApprovalPhraseSort(ids: string[]) {
  return request({
    url: '/api/WfApprovalPhrase/UpdateSortBatch',
    method: 'post',
    data: { Ids: ids }
  });
}
