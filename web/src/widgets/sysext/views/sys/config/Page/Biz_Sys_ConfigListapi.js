import request from '@/utils/requestAPI6.js';

const base = '/biz/sysconfiglist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysConfiglist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysConfigById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysConfigList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysConfigByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
