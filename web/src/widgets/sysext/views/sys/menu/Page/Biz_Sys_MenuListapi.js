import request from '@/utils/requestAPI6.js';

const base = '/biz/sysmenulist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSysMenulist(para) {
  return request({ url: `${base}/qlist`, method: 'post', data: para || {} });
}

export function queryBizSysMenuById(id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

export function saveBizSysMenuList(para) {
  return request({ url: `${base}/save`, method: 'post', data: para });
}

export function removeBizSysMenuByIds(para) {
  return request({ url: `${base}/rmbyids`, method: 'post', data: para });
}

export function treeBizSysMenu() {
  return request({ url: `${base}/tree`, method: 'post' });
}
