import request from '@/utils/requestAPI6.js';

export function loadPageInit(para) {
	return request({ url: '/bpo/sys/post-type/init', method: 'post', data: para });
}

export function querySysPostTypelist(para) {
	return request({ url: '/bpo/sys/post-type/qlist', method: 'post', data: para });
}

export function querySysPostTypeById(id) {
	return request({ url: '/bpo/sys/post-type/qone/' + id, method: 'get' });
}
