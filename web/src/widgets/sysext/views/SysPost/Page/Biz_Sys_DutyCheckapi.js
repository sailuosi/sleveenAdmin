import request from '@/utils/requestAPI6.js';

export function loadPageInit(para) {
	return request({ url: '/bpo/sys/duty-check/init', method: 'post', data: para });
}

export function queryDutyChecklist(para) {
	return request({ url: '/bpo/sys/duty-check/qlist', method: 'post', data: para });
}
