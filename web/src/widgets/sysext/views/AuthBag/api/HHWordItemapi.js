import request from '@/utils/requestAPI6.js'

// 查询词条定义列表
export function selectHHWordItemlist(params) {
    return request({
        url: '/hhworditem/hhworditemlist',
        method: 'post',
        data: params
    })
}
// 根据id获取词条定义
export function getHHWordItemById(params) {
    return request({
        url: '/hhworditem/loadhhworditembyid',
        method: 'post',
        data: params
    })
}

// 保存词条定义
export function saveHHWordItem(params) {
    return request({
        url: '/hhworditem/savehhworditems',
        method: 'post',
        data: params
    });
}

// 删除词条定义
export function removeHHWordItem(params) {
    return request({
        url: '/hhworditem/removehhworditem',
        method: 'post',
        data: params
    })
}

// 查询词条定义指定单位列表
export function selectHHWordItemOrglist(params) {
    return request({
        url: '/hhworditem/hhworditemorglist',
        method: 'post',
        data: params
    })
}