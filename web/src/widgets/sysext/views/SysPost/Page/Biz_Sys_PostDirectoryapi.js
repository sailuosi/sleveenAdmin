import request from '@/utils/requestAPI6.js';

export function listByOrg(para) {
	return request({ url: '/bpo/sys/post-directory/query', method: 'post', data: para });
}

export function loadHrOrgTree(para) {
	return request({ url: '/bpo/sys/hr-organize/tree', method: 'post', data: para || {} });
}

export function loadSysOrgTree() {
	return request({ url: '/biz/sysorglist/tree', method: 'post', data: {} });
}
