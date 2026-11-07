import request from '@/utils/portal/requestAPI6.js';

export function fetchSysPageSelectPage(params) {
  return request({ url: '/api/sysPageSelect/page', method: 'get', params });
}

export function fetchSysPageSelect(id) {
  return request({ url: `/api/sysPageSelect/${id}`, method: 'get' });
}

export function saveSysPageSelect(rows) {
  return request({ url: '/api/sysPageSelect', method: 'post', data: rows });
}

export function deleteSysPageSelect(id) {
  return request({ url: `/api/sysPageSelect/${id}`, method: 'delete' });
}
