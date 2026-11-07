import request from '@/utils/requestAPI6.js';

const base = '/biz/sysappletcatalist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysAppletCatalist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysAppletCataById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysAppletCataList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysAppletCataByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
