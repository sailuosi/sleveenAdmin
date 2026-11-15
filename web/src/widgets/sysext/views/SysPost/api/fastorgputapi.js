import request from '@/utils/requestAPI6.js';

/** 组织（HR_Organize） */
export function loadFastPutOrg(para) {
	return request({
		url: '/fastput/sh/org',
		method: 'post',
		data: para
	});
}

/** 选人面板专用：模糊查组织并返回 PathName —— `POST /fastput/sh/orgpanel/search` */
export function loadFastPutOrgPanelSearch(para) {
	return request({
		url: '/fastput/sh/orgpanel/search',
		method: 'post',
		data: para
	});
}

/**
 * 选人面板：叶子组织 OID → root→leaf 展开链 + 叶子行（取代多次 BPO organize/qone）
 * `POST /fastput/sh/orgpanel/ancestor-path` body: `{ HR_OrganizeOID }` 或 `{ leafOrgOid }`
 */
export function loadFastPutOrgPanelAncestorPath(para) {
	return request({
		url: '/fastput/sh/orgpanel/ancestor-path',
		method: 'post',
		data: para
	});
}

/** 批量过滤组织面板默认项：只返回当前 BPO 权限下可见的传入组织 */
export function loadFastPutOrgPanelAuthDefaults(para) {
	return request({
		url: '/fastput/sh/orgpanel/auth-defaults',
		method: 'post',
		data: para
	});
}

/** 选人面板：按组织关系组编码 + 主单位查询绑定组织列表 */
export function loadFastPutOrgPanelOrgLinkBinds(para) {
	return request({
		url: '/fastput/sh/orgpanel/orglink-binds',
		method: 'post',
		data: para
	});
}
