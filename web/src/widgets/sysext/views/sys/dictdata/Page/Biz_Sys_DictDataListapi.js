import request from '@/utils/requestAPI6.js';

const base = '/biz/sysdictdatalist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysDictDatalist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysDictDataById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysDictDataList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysDictDataByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
