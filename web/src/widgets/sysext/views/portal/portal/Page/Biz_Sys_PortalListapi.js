import request from '@/utils/requestAPI6.js';

const base = '/biz/sysportallist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysPortallist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysPortalById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysPortalList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysPortalByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
