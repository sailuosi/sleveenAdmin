///////////////////////////////////////////////////////////
//  BPO_HH_DutyItemCheckAPI.js
//  选择角色业务页面前端API接口定义
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/12 14:42:26
///////////////////////////////////////////////////////////

import request from '@/utils/requestAPI6.js';

// 加载选择角色页面的初始化信息
export function loadPageInit(para) {
	return request({
		url: '/bpo/hh/dutyitemcheck/init',
		method: 'post',
		data:para
	});
}
// #region [autoGenerate] [cover] [勿改] BC接口
// 查询责任设置数据列表，支持分页排序筛选等
// 默认：序号升序，创建时间倒序
export function queryBCHHDutyItemlist(para) {
	const data = Object.assign({}, para);
	if (!Array.isArray(data.orderBys) || data.orderBys.length === 0) {
		data.orderBys = [
			{ idx: 0, field: "Di_Idx", order: "asc" },
			{ idx: 1, field: "SYS_Created", order: "desc" },
		];
	}
	return request({
		url: '/bpo/hh/dutyitemcheck/hh/dutyitem/qlist',
		method: 'post',
		data
	});
}
//查询一条责任设置数据详情
export function queryBCHHDutyItemById(id) {
	return request({
		url: '/bpo/hh/dutyitemcheck/hh/dutyitem/qone/'+id,
		method: 'get',
	});
}
//导出责任设置数据
export function exportBCHHDutyItemToExcel(para) {
	return request({
		url: '/bpo/hh/dutyitemcheck/hh/dutyitem/exptexcel',
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