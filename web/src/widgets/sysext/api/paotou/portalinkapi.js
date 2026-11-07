import request from '@/utils/portal/requestAPI6.js';

export function fetchSysPortalLinkPage(params) {
  return request({ url: '/api/sysPortalLink/page', method: 'get', params });
}

export function saveSysPortalLink(rows) {
  return request({ url: '/api/sysPortalLink', method: 'post', data: rows });
}

export function deleteSysPortalLink(id) {
  return request({ url: `/api/sysPortalLink/${id}`, method: 'delete' });
}
