import request from '@/utils/requestAPI6.js';

const base = '/biz/saebcatalist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSaEbCatalist(para) {
  return request({ url: `${base}/ebook/ebcata/qlist`, method: 'post', data: para || {} });
}

export function queryBizSaEbCataById(id) {
  return request({ url: `${base}/ebook/ebcata/qone/${id}`, method: 'get' });
}

export function saveBizSaEbCataList(para) {
  return request({ url: `${base}/ebook/ebcata/save`, method: 'post', data: para });
}

export function removeBizSaEbCataByIds(para) {
  return request({ url: `${base}/ebook/ebcata/rmbyids`, method: 'post', data: para });
}
