import request from '@/utils/portal/requestAPI6.js';

export function fetchSysPageFieldPage(params) {
  return request({ url: '/api/sysPageField/page', method: 'get', params });
}

export function fetchSysPageField(id) {
  return request({ url: `/api/sysPageField/${id}`, method: 'get' });
}

export function saveSysPageField(rows) {
  return request({ url: '/api/sysPageField', method: 'post', data: rows });
}

export function deleteSysPageField(id) {
  return request({ url: `/api/sysPageField/${id}`, method: 'delete' });
}
