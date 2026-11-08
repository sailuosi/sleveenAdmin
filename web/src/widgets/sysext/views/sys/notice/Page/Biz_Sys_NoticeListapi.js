import request from '@/utils/requestAPI6.js';

const base = '/biz/sysnoticelist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysNoticelist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysNoticeById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysNoticeList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysNoticeByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}

export function publishBizSysNotice(id) {
  return request({ url: `${base}/publish/${id}`, method: 'post' });
}
