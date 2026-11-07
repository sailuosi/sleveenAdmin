import request from '@/utils/requestAPI6.js';

const base = '/biz/sysportcelllist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysPortCelllist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysPortCellById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysPortCellList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysPortCellByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
