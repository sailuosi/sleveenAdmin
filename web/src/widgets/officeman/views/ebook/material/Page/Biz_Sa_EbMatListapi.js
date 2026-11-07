import request from '@/utils/requestAPI6.js';

const base = '/biz/saebmatlist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSaEbMatlist(para) {
  return request({ url: `${base}/ebook/ebmat/qlist`, method: 'post', data: para || {} });
}

export function queryBizSaEbMatById(id) {
  return request({ url: `${base}/ebook/ebmat/qone/${id}`, method: 'get' });
}

export function saveBizSaEbMatList(para) {
  return request({ url: `${base}/ebook/ebmat/save`, method: 'post', data: para });
}

export function removeBizSaEbMatByIds(para) {
  return request({ url: `${base}/ebook/ebmat/rmbyids`, method: 'post', data: para });
}

export function loadMatStats(para) {
  return request({ url: `${base}/ebook/ebmat/stats`, method: 'post', data: para || {} });
}
