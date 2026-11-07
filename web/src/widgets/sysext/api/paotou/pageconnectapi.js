import request from '@/utils/portal/requestAPI6.js';

export function fetchSysPageConnectPage(params) {
  return request({ url: '/api/sysPageConnect/page', method: 'get', params });
}

export function saveSysPageConnect(rows) {
  return request({ url: '/api/sysPageConnect', method: 'post', data: rows });
}
