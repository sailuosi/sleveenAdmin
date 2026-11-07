import request from '@/utils/requestAPI6.js';

export type CategoryRow = {
  Id?: string;
  Code?: string;
  Name?: string;
  SortOrder?: number;
  Remark?: string;
  CreatedAt?: string;
};

export function listCategories() {
  return request({ url: '/api/WfCategory/List', method: 'get' });
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
