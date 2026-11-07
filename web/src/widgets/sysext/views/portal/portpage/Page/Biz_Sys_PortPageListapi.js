import request from '@/utils/requestAPI6.js';

const base = '/biz/sysportpagelist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysPortPagelist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysPortPageById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysPortPageList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysPortPageByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
