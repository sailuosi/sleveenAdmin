import request from '@/utils/requestAPI6.js';

export function loadPageInit(para) {
	return request({ url: '/bpo/sys/post-auth/init', method: 'post', data: para });
}

export function loadUserAuth(userId) {
	return request({ url: '/bpo/sys/post-auth/load/' + userId, method: 'get' });
}

export function savePosts(userId, para) {
	return request({ url: '/bpo/sys/post-auth/posts/save/' + userId, method: 'post', data: para });
}

export function removePosts(para) {
	return request({ url: '/bpo/sys/post-auth/posts/rmbyids', method: 'post', data: para });
}

export function saveScopes(userId, para) {
	return request({ url: '/bpo/sys/post-auth/scopes/save/' + userId, method: 'post', data: para });
}

export function removeScopes(para) {
	return request({ url: '/bpo/sys/post-auth/scopes/rmbyids', method: 'post', data: para });
}

export function loadAssignableTypes(postId) {
	return request({ url: '/bpo/sys/post-auth/assignable-types/' + postId, method: 'get' });
}

export function clearUserCache(userId) {
	return request({ url: '/bpo/sys/post-auth/clear-cache/' + userId, method: 'post' });
}

export function querySysPostlist(para) {
	return request({ url: '/bpo/sys/post/qlist', method: 'post', data: para });
}

export function saveSysPostList(para) {
	return request({ url: '/bpo/sys/post/save', method: 'post', data: para });
}

export function querySysPostTypelist(para) {
	return request({ url: '/bpo/sys/post-type/qlist', method: 'post', data: para });
}
