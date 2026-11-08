import request from '@/utils/requestAPI6.js';

const base = '/biz/systenantlist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysTenantlist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysTenantById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysTenantList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysTenantByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}

export function listBizSysTenant() {
  return request({ url: `${base}/list`, method: 'get' });
}
