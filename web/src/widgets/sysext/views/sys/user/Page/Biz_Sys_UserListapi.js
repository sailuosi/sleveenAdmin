import request from '@/utils/requestAPI6.js';

const base = '/biz/sysuserlist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysUserlist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysUserById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysUserList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysUserByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}
