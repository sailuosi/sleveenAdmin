///////////////////////////////////////////////////////////
//  BPO_TreeAPI.js
//  地点主数据导航树业务页面前端API接口定义
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/20 16:36:52
///////////////////////////////////////////////////////////

import request from '@/utils/requestAPI6.js';

// 加载地点主数据导航树页面的初始化信息
export function loadPageInit(para) {
	return request({
		url: '/bpo/tree/init',
		method: 'post',
		data:para
	});
}
// #region [autoGenerate] [cover] [勿改] BC接口
// 查询导航树数据列表，支持分页排序筛选等
export function queryBCMDTreelist(para) {
	return request({
		url: '/bpo/tree/md/tree/qlist',
		method: 'post',
		data:para
	});
}
//查询一条导航树数据详情
export function queryBCMDTreeById(id) {
	return request({
		url: '/bpo/tree/md/tree/qone/'+id,
		method: 'get',
	});
}
//批量保存导航树数据，支持批量插入和更新
export function saveBCMDTreeList(para ) {
	return request({
		url: '/bpo/tree/md/tree/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除导航树数据
export function removeBCMDTreeByIds(para) {
	return request({
		url: '/bpo/tree/md/tree/rmbyids',
		method: 'post',
		data:para
	});
}
//查询导航树树结构数据
export function queryBCMDTreeTree(para) {
	return request({
		url: '/bpo/tree/md/tree/qtree',
		method: 'post',
		data:para
	});
}
//导出导航树数据
export function exportBCMDTreeToExcel(para) {
	return request({
		url: '/bpo/tree/md/tree/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询巷道数据列表，支持分页排序筛选等
export function queryBCMDLanelist(para) {
	return request({
		url: '/bpo/tree/md/lane/qlist',
		method: 'post',
		data:para
	});
}
//查询一条巷道数据详情
export function queryBCMDLaneById(id) {
	return request({
		url: '/bpo/tree/md/lane/qone/'+id,
		method: 'get',
	});
}
//批量保存巷道数据，支持批量插入和更新
export function saveBCMDLaneList(para ) {
	return request({
    url: '/bpo/lanelist/md/lane/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除巷道数据
export function removeBCMDLaneByIds(para) {
	return request({
		url: '/bpo/tree/md/lane/rmbyids',
		method: 'post',
		data:para
	});
}
//导出巷道数据
export function exportBCMDLaneToExcel(para) {
	return request({
		url: '/bpo/tree/md/lane/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询硐室数据列表，支持分页排序筛选等
export function queryBCMDChamberlist(para) {
	return request({
		url: '/bpo/tree/md/chamber/qlist',
		method: 'post',
		data:para
	});
}
//查询一条硐室数据详情
export function queryBCMDChamberById(id) {
	return request({
		url: '/bpo/tree/md/chamber/qone/'+id,
		method: 'get',
	});
}
//批量保存硐室数据，支持批量插入和更新
export function saveBCMDChamberList(para ) {
	return request({
		url: '/bpo/chamberlist/md/chamber/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除硐室数据
export function removeBCMDChamberByIds(para) {
	return request({
		url: '/bpo/tree/md/chamber/rmbyids',
		method: 'post',
		data:para
	});
}
//导出硐室数据
export function exportBCMDChamberToExcel(para) {
	return request({
		url: '/bpo/tree/md/chamber/exptexcel',
		method: 'post',
      responseType: 'blob', // 关键配置：接收二进制流
		requestOptions:{ isReturnNativeResponse: true },
		data:para
	});
}
// 查询地点数据列表，支持分页排序筛选等
export function queryBCMDPlacelist(para) {
	return request({
		url: '/bpo/tree/md/place/qlist',
		method: 'post',
		data:para
	});
}
//查询一条地点数据详情
export function queryBCMDPlaceById(id) {
	return request({
		url: '/bpo/tree/md/place/qone/'+id,
		method: 'get',
	});
}
//批量保存地点数据，支持批量插入和更新
export function saveBCMDPlaceList(para ) {
	return request({
		url: '/bpo/placeList/md/place/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除地点数据
export function removeBCMDPlaceByIds(para) {
	return request({
		url: '/bpo/tree/md/place/rmbyids',
		method: 'post',
		data:para
	});
}
//导出地点数据
export function exportBCMDPlaceToExcel(para) {
	return request({
		url: '/bpo/tree/md/place/exptexcel',
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
