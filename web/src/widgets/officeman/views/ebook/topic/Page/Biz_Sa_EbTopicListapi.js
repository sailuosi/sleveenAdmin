import request from '@/utils/requestAPI6.js';

const base = '/biz/saebtopiclist';

export function loadPageInit(para) {
  return request({ url: `${base}/init`, method: 'post', data: para || {} });
}

export function queryBizSaEbTopiclist(para) {
  return request({ url: `${base}/ebook/ebtopic/qlist`, method: 'post', data: para || {} });
}

export function queryBizSaEbTopicById(id) {
  return request({ url: `${base}/ebook/ebtopic/qone/${id}`, method: 'get' });
}

export function saveBizSaEbTopicList(para) {
  return request({ url: `${base}/ebook/ebtopic/save`, method: 'post', data: para });
}

export function removeBizSaEbTopicByIds(para) {
  return request({ url: `${base}/ebook/ebtopic/rmbyids`, method: 'post', data: para });
}

export function loadTopicStats(para) {
  return request({ url: `${base}/ebook/ebtopic/stats`, method: 'post', data: para || {} });
}

export function advanceTopic(para) {
  return request({ url: `${base}/ebook/ebtopic/advance`, method: 'post', data: para });
}

export function listTopicMat(para) {
  return request({ url: `${base}/ebook/ebtopic/listMat`, method: 'post', data: para });
}

export function bindTopicMat(para) {
  return request({ url: `${base}/ebook/ebtopic/bindMat`, method: 'post', data: para });
}

export function unbindTopicMat(para) {
  return request({ url: `${base}/ebook/ebtopic/unbindMat`, method: 'post', data: para });
}
