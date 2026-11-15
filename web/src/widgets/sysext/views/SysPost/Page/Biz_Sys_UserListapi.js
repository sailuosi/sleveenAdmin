import request from '@/utils/requestAPI6.js';

export function loadPageInit(para) {
	return request({ url: '/biz/sysuserlist/init', method: 'post', data: para });
}

export function querySysUserlist(para) {
	return request({ url: '/biz/sysuserlist/qlist', method: 'post', data: para });
}

export function querySysUserById(id) {
	return request({ url: '/biz/sysuserlist/qone/' + id, method: 'get' });
}

export function loadHrOrgTree(para) {
	return request({ url: '/bpo/sys/hr-organize/tree', method: 'post', data: para || {} });
}
