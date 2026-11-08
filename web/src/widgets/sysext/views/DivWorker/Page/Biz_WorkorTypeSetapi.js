///////////////////////////////////////////////////////////
//  Biz_WorkorTypeSetAPI.js
//  管理员类别业务页面前端API接口定义
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 16:00:49
///////////////////////////////////////////////////////////

import request from '@/utils/requestAPI6.js';

// 加载管理员类别页面的初始化信息
export function loadPageInit(para) {
	return request({
		url: '/bpo/workortypeset/init',
		method: 'post',
		data:para
	});
}
// #region [autoGenerate] [cover] [勿改] BC接口
// 查询管理员类别数据列表，支持分页排序筛选等
export function queryBCHHWorkorTypelist(para) {
	return request({
		url: '/bpo/workortypeset/hh/workortype/qlist',
		method: 'post',
		data:para
	});
}
//查询一条管理员类别数据详情
export function queryBCHHWorkorTypeById(id) {
	return request({
		url: '/bpo/workortypeset/hh/workortype/qone/'+id,
		method: 'get',
	});
}
//批量保存管理员类别数据，支持批量插入和更新
export function saveBCHHWorkorTypeList(para ) {
	return request({
		url: '/bpo/workortypeset/hh/workortype/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除管理员类别数据
export function removeBCHHWorkorTypeByIds(para) {
	return request({
		url: '/bpo/workortypeset/hh/workortype/rmbyids',
		method: 'post',
		data:para
	});
}
//导出管理员类别数据
export function exportBCHHWorkorTypeToExcel(para) {
	return request({
		url: '/bpo/workortypeset/hh/workortype/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询管理员角色数据列表，支持分页排序筛选等
// 默认排序由后端处理：Di_Idx 升序，SYS_Created 倒序
export function queryBCHHWorkorRolelist(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorrole/qlist',
		method: 'post',
		data: para
	});
}
//查询一条管理员角色数据详情
export function queryBCHHWorkorRoleById(id) {
	return request({
		url: '/bpo/workortypeset/hh/workorrole/qone/'+id,
		method: 'get',
	});
}
//批量保存管理员角色数据，支持批量插入和更新
export function saveBCHHWorkorRoleList(para ) {
	return request({
		url: '/bpo/workortypeset/hh/workorrole/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除管理员角色数据
export function removeBCHHWorkorRoleByIds(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorrole/rmbyids',
		method: 'post',
		data:para
	});
}
//导出管理员角色数据
export function exportBCHHWorkorRoleToExcel(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorrole/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询可分配类别数据列表，支持分页排序筛选等
// 默认：排序升序，创建时间倒序
export function queryBCHHWorkorChildslist(para) {
	const data = Object.assign({}, para);
	if (!Array.isArray(data.orderBys) || data.orderBys.length === 0) {
		data.orderBys = [
			{ idx: 0, field: "WT_Order", order: "asc" },
			{ idx: 1, field: "WT_Created", order: "desc" },
		];
	}
	return request({
		url: '/bpo/workortypeset/hh/workorchilds/qlist',
		method: 'post',
		data
	});
}
//查询一条可分配类别数据详情
export function queryBCHHWorkorChildsById(id) {
	return request({
		url: '/bpo/workortypeset/hh/workorchilds/qone/'+id,
		method: 'get',
	});
}
//批量保存可分配类别数据，支持批量插入和更新
export function saveBCHHWorkorChildsList(para ) {
	return request({
		url: '/bpo/workortypeset/hh/workorchilds/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除可分配类别数据
export function removeBCHHWorkorChildsByIds(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorchilds/rmbyids',
		method: 'post',
		data:para
	});
}
//导出可分配类别数据
export function exportBCHHWorkorChildsToExcel(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorchilds/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询类别下的人数据列表，支持分页排序筛选等
export function queryBCHHWorkorScopeManlist(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorscopeman/qlist',
		method: 'post',
		data:para
	});
}
//查询一条类别下的人数据详情
export function queryBCHHWorkorScopeManById(id) {
	return request({
		url: '/bpo/workortypeset/hh/workorscopeman/qone/'+id,
		method: 'get',
	});
}
//批量保存类别下的人数据，支持批量插入和更新
export function saveBCHHWorkorScopeManList(para ) {
	return request({
		url: '/bpo/workortypeset/hh/workorscopeman/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除类别下的人数据
export function removeBCHHWorkorScopeManByIds(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorscopeman/rmbyids',
		method: 'post',
		data:para
	});
}
//导出类别下的人数据
export function exportBCHHWorkorScopeManToExcel(para) {
	return request({
		url: '/bpo/workortypeset/hh/workorscopeman/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// #endregion [autoGenerate] [cover] [勿改] BC接口

// #region [autoGenerate] [lv2] 业务方法
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [lv3] 扩展业务逻辑

// #endregion [autoGenerate] [lv3] 扩展业务逻辑