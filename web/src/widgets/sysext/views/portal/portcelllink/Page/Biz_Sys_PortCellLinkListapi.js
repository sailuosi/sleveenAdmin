import request from '@/utils/requestAPI6.js';

const base = '/biz/sysportcelllinklist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysPortCellLinklist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysPortCellLinkById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysPortCellLinkList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysPortCellLinkByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
