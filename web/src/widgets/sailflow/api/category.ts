import request from '@/utils/requestAPI6.js';

export type CategoryRow = {
  Id?: string;
  Code?: string;
  Name?: string;
  SortOrder?: number;
  Remark?: string;
  AuthType?: string;
  AuthCode?: string;
  AuthId?: string;
  AuthLevel?: number | null;
  Creator?: string;
  CreatorName?: string;
  CreatedAt?: string;
};

export function listCategories(unitId?: string | null) {
  return request({
    url: '/api/WfCategory/List',
    method: 'get',
    params: unitId ? { unitId } : undefined
  });
}

export function getNextSort(authType?: string, unitId?: string) {
  return request({
    url: '/api/WfCategory/NextSort',
    method: 'get',
    params: { authType, unitId }
  });
}

export function saveCategory(data: CategoryRow) {
  return request({ url: '/api/WfCategory/Save', method: 'post', data });
}

export function deleteCategory(idOrCode: string) {
  return request({
    url: `/api/WfCategory/Delete/${encodeURIComponent(idOrCode)}`,
    method: 'post'
  });
}

export function bindProcessCategory(data: {
  ProcessId: string;
  Version?: string;
  CategoryCode?: string;
}) {
  return request({
    url: '/api/WfCategory/BindProcess',
    method: 'post',
    data
  });
}

export function countByCategory() {
  return request({ url: '/api/WfCategory/CountByCategory', method: 'get' });
}

/** 按 Id 顺序批量写 SortOrder */
export function updateCategorySortBatch(ids: string[]) {
  return request({
    url: '/api/WfCategory/UpdateSortBatch',
    method: 'post',
    data: { Ids: ids }
  });
}
