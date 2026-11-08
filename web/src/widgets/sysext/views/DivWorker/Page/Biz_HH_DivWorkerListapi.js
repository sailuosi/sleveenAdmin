///////////////////////////////////////////////////////////
//  Biz_HH_DivWorkerListAPI.js
//  单位管理员业务页面前端API接口定义
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/10 9:25:22
///////////////////////////////////////////////////////////

import request from '@/utils/requestAPI6.js';

// 加载单位管理员页面的初始化信息
export function loadPageInit(para) {
	return request({
		url: '/bpo/hh/divworkerlist/init',
		method: 'post',
		data:para
	});
}
// #region [autoGenerate] [cover] [勿改] BC接口
// 查询单位管理员数据列表，支持分页排序筛选等
export function queryBCHHDivWorkerlist(para) {
	return request({
		url: '/bpo/hh/divworkerlist/hh/divworker/qlist',
		method: 'post',
		data:para
	});
}
//查询一条单位管理员数据详情
export function queryBCHHDivWorkerById(id) {
	return request({
		url: '/bpo/hh/divworkerlist/hh/divworker/qone/'+id,
		method: 'get',
	});
}
//批量保存单位管理员数据，支持批量插入和更新
export function saveBCHHDivWorkerList(para ) {
	return request({
		url: '/bpo/hh/divworkerlist/hh/divworker/save',
		method: 'post',
		data:para
	});
}
//根据ID批量删除单位管理员数据
export function removeBCHHDivWorkerByIds(para) {
	return request({
		url: '/bpo/hh/divworkerlist/hh/divworker/rmbyids',
		method: 'post',
		data:para
	});
}
//导出单位管理员数据
export function exportBCHHDivWorkerToExcel(para) {
	return request({
		url: '/bpo/hh/divworkerlist/hh/divworker/exptexcel',
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
/** 撤销授权：删除从表 HH_WorkorScope 及主表 HH_DivWorker（与后端 removeAuth 一致） */
export function removeAuth(para) {
	return request({
		url: '/bpo/hh/divworkerlist/removeAuth',
		method: 'post',
		data: para
	});
}

/** 查询当前登录人的关联单位/地点列表（后端：/bpo/hh/divworkerlist/my/qlist） */
export function queryMyDivWorkerList(para) {
	return request({
		url: '/bpo/hh/divworkerlist/my/qlist',
		method: 'post',
		data: para
	});
}

/** 切换当前登录人的主岗（后端：/bpo/hh/divworkerlist/my/setmain） */
export function setMyMainDivWorker(para) {
	return request({
		url: '/bpo/hh/divworkerlist/my/setmain',
		method: 'post',
		data: para
	});
}

/** 查询当前登录人在主岗下拥有的管理员类别标题（WT_Title） */
export function queryMyWorkorTypeTitles(para) {
	return request({
		url: '/bpo/hh/divworkerlist/my/workortitles',
		method: 'post',
		data: para
	});
}
// #endregion [autoGenerate] [lv3] 扩展业务逻辑