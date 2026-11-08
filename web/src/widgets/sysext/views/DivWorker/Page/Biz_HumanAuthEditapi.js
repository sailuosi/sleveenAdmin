///////////////////////////////////////////////////////////
//  Biz_HumanAuthEditAPI.js
//  人员授权业务页面前端API接口定义
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/12 11:57:25
///////////////////////////////////////////////////////////

import request from '@/utils/requestAPI6.js';

// 加载人员授权页面的初始化信息
export function loadPageInit(para) {
	return request({
		url: '/bpo/humanauthedit/init',
		method: 'post',
		data:para
	});
}
// #region [autoGenerate] [cover] [勿改] BC接口
// 查询人力人员数据列表，支持分页排序筛选等
export function queryBCHRHumanlist(para) {
	return request({
		url: '/bpo/humanauthedit/hr/human/qlist',
		method: 'post',
		data:para
	});
}
//查询一条人力人员数据详情
export function queryBCHRHumanById(id) {
	return request({
		url: '/bpo/humanauthedit/hr/human/qone/'+id,
		method: 'get',
	});
}
//批量保存人力人员数据，支持批量插入和更新
export function saveBCHRHumanList(para ) {
	return request({
		url: '/bpo/humanauthedit/hr/human/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除人力人员数据
export function removeBCHRHumanByIds(para) {
	return request({
		url: '/bpo/humanauthedit/hr/human/rmbyids',
		method: 'post',
		data:para
	});
}
//导出人力人员数据
export function exportBCHRHumanToExcel(para) {
	return request({
		url: '/bpo/humanauthedit/hr/human/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询单位管理员数据列表，支持分页排序筛选等
export function queryBCHHDivWorkerlist(para) {
	return request({
		url: '/bpo/humanauthedit/hh/divworker/qlist',
		method: 'post',
		data:para
	});
}
//查询一条单位管理员数据详情
export function queryBCHHDivWorkerById(id) {
	return request({
		url: '/bpo/humanauthedit/hh/divworker/qone/'+id,
		method: 'get',
	});
}
//批量保存单位管理员数据，支持批量插入和更新
export function saveBCHHDivWorkerList(para ) {
	return request({
		url: '/bpo/humanauthedit/hh/divworker/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除单位管理员数据
export function removeBCHHDivWorkerByIds(para) {
	return request({
		url: '/bpo/humanauthedit/hh/divworker/rmbyids',
		method: 'post',
		data:para
	});
}
//导出单位管理员数据
export function exportBCHHDivWorkerToExcel(para) {
	return request({
		url: '/bpo/humanauthedit/hh/divworker/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询管理员权限数据列表，支持分页排序筛选等
export function queryBCHHWorkorOfScopelist(para) {
	return request({
		url: '/bpo/humanauthedit/hh/workorofscope/qlist',
		method: 'post',
		data:para
	});
}
//查询一条管理员权限数据详情
export function queryBCHHWorkorOfScopeById(id) {
	return request({
		url: '/bpo/humanauthedit/hh/workorofscope/qone/'+id,
		method: 'get',
	});
}
//批量保存管理员权限数据，支持批量插入和更新
export function saveBCHHWorkorOfScopeList(para ) {
	return request({
		url: '/bpo/humanauthedit/hh/workorofscope/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除管理员权限数据
export function removeBCHHWorkorOfScopeByIds(para) {
	return request({
		url: '/bpo/humanauthedit/hh/workorofscope/rmbyids',
		method: 'post',
		data:para
	});
}
//导出管理员权限数据
export function exportBCHHWorkorOfScopeToExcel(para) {
	return request({
		url: '/bpo/humanauthedit/hh/workorofscope/exptexcel',
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

export function toggleUserState(para) {
	return request({
		url: '/mancomm/toggleUserState',
		method: 'post',
		data: para
	});
}

export function queryManAuthChangeLogList(para) {
	return request({
		url: '/bpo/humanauthedit/hh/manauthchangelog/qlist',
		method: 'post',
		data: para
	});
}

// #endregion [autoGenerate] [lv3] 扩展业务逻辑

//
export function loadWorkTypeSelect(para) {
	return request({
		url: '/bpo/humanauthedit/loadWorkTypeSelect',
		method: 'post',
		data:para
	});
}

export function deleteHisAllKeyU(para) {
	return request({
        url: '/cache/deleteHisAllKeyU',
        method: 'post',
		data:para
	});
}
export function clearAllCache(para ) {
	return request({
        url: '/cache/clearAll',
        method: 'post',
		data:para
	});
}