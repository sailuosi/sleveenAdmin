///////////////////////////////////////////////////////////
//  BPO_HumanListAPI.js
//  人力人员业务页面前端API接口定义
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 17:18:09
///////////////////////////////////////////////////////////

import request from '@/utils/requestAPI6.js';

// 加载人力人员页面的初始化信息
export function loadPageInit(para) {
	return request({
		url: '/bpo/humanlist/init',
		method: 'post',
		data:para
	});
}
// #region [autoGenerate] [cover] [勿改] BC接口
// 查询人力人员数据列表，支持分页排序筛选等
export function queryBCHRHumanlist(para) {
	return request({
		url: '/bpo/humanlist/hr/human/qlist',
		method: 'post',
		data:para
	});
}
//查询一条人力人员数据详情
export function queryBCHRHumanById(id) {
	return request({
		url: '/bpo/humanlist/hr/human/qone/'+id,
		method: 'get',
	});
}
//批量保存人力人员数据，支持批量插入和更新
export function saveBCHRHumanList(para ) {
	return request({
		url: '/bpo/humanlist/hr/human/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除人力人员数据
export function removeBCHRHumanByIds(para) {
	return request({
		url: '/bpo/humanlist/hr/human/rmbyids',
		method: 'post',
		data:para
	});
}
//导出人力人员数据
export function exportBCHRHumanToExcel(para) {
	return request({
		url: '/bpo/humanlist/hr/human/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询人力组织数据列表，支持分页排序筛选等
export function queryBCHROrganizelist(para) {
	return request({
		url: '/bpo/humanlist/hr/organize/qlist',
		method: 'post',
		data:para
	});
}
//查询一条人力组织数据详情
export function queryBCHROrganizeById(id) {
	return request({
		url: '/bpo/humanlist/hr/organize/qone/'+id,
		method: 'get',
	});
}
//批量保存人力组织数据，支持批量插入和更新
export function saveBCHROrganizeList(para ) {
	return request({
		url: '/bpo/humanlist/hr/organize/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除人力组织数据
export function removeBCHROrganizeByIds(para) {
	return request({
		url: '/bpo/humanlist/hr/organize/rmbyids',
		method: 'post',
		data:para
	});
}
//查询人力组织树结构数据
export function queryBCHROrganizeTree(para) {
	return request({
		url: '/bpo/humanlist/hr/organize/qtree',
		method: 'post',
		data:para
	});
}
//导出人力组织数据
export function exportBCHROrganizeToExcel(para) {
	return request({
		url: '/bpo/humanlist/hr/organize/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// #endregion [autoGenerate] [cover] [勿改] BC接口

// #region [autoGenerate] [lv2] 业务方法
// 切换用户启用/停用状态
export function toggleUserState(para) {
	return request({
		url: '/mancomm/toggleUserState',
		method: 'post',
		data: para
	});
}
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [lv3] 扩展业务逻辑

// #endregion [autoGenerate] [lv3] 扩展业务逻辑