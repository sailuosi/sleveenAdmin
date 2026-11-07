import request from '@/utils/portal/requestAPI6.js';

export function loadOrgTree() {
  return request({ url: '/api/sysOrg/tree', method: 'get' });
}

export function loadHROrgTree() {
  return loadOrgTree();
}

export function loadManSearch(params) {
  return request({ url: '/api/sysUser/page', method: 'get', params });
}
