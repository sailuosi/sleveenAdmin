import request from '@/utils/requestAPI6.js';

const base = '/biz/sysdicttypelist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysDictTypelist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysDictTypeById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysDictTypeList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysDictTypeByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
