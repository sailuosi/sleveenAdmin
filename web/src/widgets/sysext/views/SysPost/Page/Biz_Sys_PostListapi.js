import request from '@/utils/requestAPI6.js';

export function loadPageInit(para) {
	return request({ url: '/bpo/sys/post/init', method: 'post', data: para });
}

export function querySysPostlist(para) {
	return request({ url: '/bpo/sys/post/qlist', method: 'post', data: para });
}

export function querySysPostById(id) {
	return request({ url: '/bpo/sys/post/qone/' + id, method: 'get' });
}

export function saveSysPostList(para) {
	return request({ url: '/bpo/sys/post/save', method: 'post', data: para });
}

export function removeSysPostByIds(para) {
	return request({ url: '/bpo/sys/post/rmbyids', method: 'post', data: para });
}

export function removeAuth(para) {
	return request({ url: '/bpo/sys/post-auth/posts/rmbyids', method: 'post', data: para });
}
