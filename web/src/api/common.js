import request from '@/utils/requestAPI6.js';

// 帆软数字签名获取接口
export function inittoken(data) {
	return request({
		url: '/frbb/inittoken',
		method: 'post',
		data: data
	});
}

/** 生产数据统计报表页：IP 白名单校验 + 访问日志（匿名，不强制带 token） */
export function productStatReportPrevisit(data) {
	return request({
		url: '/common/product-stat-report/previsit',
		method: 'post',
		data: data || {},
		headers: { isToken: false }
	});
}


export function getHumanByMainCode(data) {
	return request({
		url: '/common/getHumanByMainCode',
		method: 'post',
		data: data || {},
	});
}

export function getHumanByCurrent() {
	return request({
		url: '/common/getHumanByCurrent',
		method: 'post',
	});
}

export function getHumanByHrId(data) {
	return request({
		url: '/common/getHumanByHrId',
		method: 'post',
		data: data || {},
	});
}

export function getHumanByOid(data) {
	return request({
		url: '/common/getHumanByOid',
		method: 'post',
		data: data || {},
	});
}

/** 管理员类别：下属角色菜单权限并集 */
export function getWorkorTypeMenuBindKeys(workorTypeId) {
	return request({
		url: '/common/workortype/menu-bindkeys',
		method: 'post',
		data: { workorTypeId },
	});
}

/** 菜单权限持有者：拥有该菜单的管理员类别 / 角色 */
export function getMenuAuthHolders(menuId) {
	return request({
		url: '/common/menu/auth-holders',
		method: 'post',
		data: { menuId },
	});
}

/** 人员：WorkorScope 下角色菜单权限并集 */
export function getHumanMenuBindKeys(humanOid) {
	return request({
		url: '/common/human/menu-bindkeys',
		method: 'post',
		data: { humanOid },
	});
}

/** 权限查看专用：全量菜单树（不按登录人过滤，不走 sysMenu:list） */
export function getSysMenuFullTree() {
	return request({
		url: '/common/sysmenu/full-tree',
		method: 'post',
		data: {},
	});
}

/** 重大风险短信发送（content: 短信内容） */
export function Send_MajorRisk_Sms(data) {
	return request({
		url: '/APP_SafeRiskHub_API/Send_MajorRisk_Sms',
		method: 'post',
		data: data || {},
	});
}

/** 重大风险APP消息推送（content: 消息内容） */
export function Send_MajorRisk_AppMsg(data) {
	return request({
		url: '/APP_SafeRiskHub_API/Send_MajorRisk_AppMsg',
		method: 'post',
		data: data || {},
	});
}