import request from '@/utils/requestAPI6.js';

const base = '/biz/sysappletlist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysAppletlist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysAppletById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysAppletList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysAppletByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
