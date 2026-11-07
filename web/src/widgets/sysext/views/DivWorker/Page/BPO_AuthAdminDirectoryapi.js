import request from '@/utils/requestAPI6.js';

/** 当前登录人所属行政单位 */
export function fetchMyBelongOrg() {
	return request({
		url: '/manAuthAdmin/myBelongOrg',
		method: 'post',
		data: {},
	});
}

/** 按行政组织查询权限管理员及可分配模块 */
export function fetchAuthAdminByOrg(para) {
	return request({
		url: '/manAuthAdmin/listByOrg',
		method: 'post',
		data: para,
	});
}
