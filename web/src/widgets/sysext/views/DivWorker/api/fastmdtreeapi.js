import request from '@/utils/requestAPI6.js';

/** MD_Tree 扁平树（前端自行挂父子） */
export function loadFastMdTreeFlat(para) {
	return request({
		url: '/fastmdtree/tree/flat',
		method: 'post',
		data: para || {},
	});
}

/** MD_Tree 按父节点懒加载一层子节点；根不传 MD_ParentOID 或传空 / 全零 GUID；返回行含 isLeaf、flowStatus、selectable */
export function loadFastMdTreeChildren(para) {
	return request({
		url: '/fastmdtree/tree/children',
		method: 'post',
		data: para || {},
	});
}

/** 地点树关键字搜索（名称/全路径 OR），最多 50 条；matchCode=true 时含编码；keyword 或 txt；onlyApproved 默认 true */
export function loadFastMdTreeSearchHits(para) {
	return request({
		url: '/fastmdtree/tree/search-hits',
		method: 'post',
		data: para || {},
	});
}

/**
 * mdinput 等输入框下拉联想：同 search-hits，专用路由 /input/suggest。
 * 可选 orgNo：与 MD_Tree.OrgNo 精确匹配；matchCode=true 时关键字另对 MD_Code 模糊（默认否）。
 * onlyApproved 默认 true（后端）：仅返回已审核；树浏览接口另附 selectable/flowStatus。
 */
export function loadFastMdTreeInputSuggest(para) {
	return request({
		url: '/fastmdtree/input/suggest',
		method: 'post',
		data: para || {},
	});
}

/** 自叶子沿父链到根，返回 { chain: root→leaf 的 MD_TreeOID 字符串[] } */
export function loadFastMdTreeAncestorPath(para) {
	return request({
		url: '/fastmdtree/tree/ancestor-path',
		method: 'post',
		data: para || {},
	});
}

/** MD_Tree 分页列表，每页 30；listKeyword 时 OR 模糊；matchCode 控制是否含 MD_Code */
export function loadFastMdTreeList(para) {
	return request({
		url: '/fastmdtree/list',
		method: 'post',
		data: para || {},
	});
}
