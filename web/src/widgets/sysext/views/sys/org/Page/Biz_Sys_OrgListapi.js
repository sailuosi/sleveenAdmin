import request from '@/utils/requestAPI6.js';

const base = '/biz/sysorglist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysOrglist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysOrgById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysOrgList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysOrgByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}

export function treeBizSysOrg() {
  return request({ url: `${base}/tree`, method: 'post' });
}
