import request from '@/utils/requestAPI6.js';

const base = '/biz/saebnotelist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSaEbNotelist(para) {
  return request({ url: `${base}/ebook/ebnote/qlist`, method: 'post', data: para || {} });
}

export function queryBizSaEbNoteById(id) {
  return request({ url: `${base}/ebook/ebnote/qone/${id}`, method: 'get' });
}

export function saveBizSaEbNoteList(para) {
  return request({ url: `${base}/ebook/ebnote/save`, method: 'post', data: para });
}

export function removeBizSaEbNoteByIds(para) {
  return request({ url: `${base}/ebook/ebnote/rmbyids`, method: 'post', data: para });
}
