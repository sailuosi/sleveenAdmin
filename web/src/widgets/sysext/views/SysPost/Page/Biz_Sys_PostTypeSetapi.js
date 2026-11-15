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

export function saveSysPostTypeList(para) {
	return request({ url: '/bpo/sys/post-type/save', method: 'post', data: para });
}

export function removeSysPostTypeByIds(para) {
	return request({ url: '/bpo/sys/post-type/rmbyids', method: 'post', data: para });
}

export function querySysPostTypeRolelist(para) {
	return request({ url: '/bpo/sys/post-type/role/qlist', method: 'post', data: para });
}

export function querySysPostTypeRoleById(id) {
	return request({ url: '/bpo/sys/post-type/role/qone/' + id, method: 'get' });
}

export function saveSysPostTypeRoleList(para) {
	return request({ url: '/bpo/sys/post-type/role/save', method: 'post', data: para });
}

export function removeSysPostTypeRoleByIds(para) {
	return request({ url: '/bpo/sys/post-type/role/rmbyids', method: 'post', data: para });
}

export function querySysPostTypeChildlist(para) {
	return request({ url: '/bpo/sys/post-type/child/qlist', method: 'post', data: para });
}

export function querySysPostTypeChildById(id) {
	return request({ url: '/bpo/sys/post-type/child/qone/' + id, method: 'get' });
}

export function saveSysPostTypeChildList(para) {
	return request({ url: '/bpo/sys/post-type/child/save', method: 'post', data: para });
}

export function removeSysPostTypeChildByIds(para) {
	return request({ url: '/bpo/sys/post-type/child/rmbyids', method: 'post', data: para });
}
