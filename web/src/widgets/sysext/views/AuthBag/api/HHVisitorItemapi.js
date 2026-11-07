import request from '@/utils/requestAPI6.js'

// 查询访客项列表
export function selectHHVisitorItemlist(params) {
    return request({
        url: '/hhvisitoritem/hhvisitoritemlist',
        method: 'post',
        data: params
    })
}
// 根据id获取访客项
export function getHHVisitorItemById(params) {
    return request({
        url: '/hhvisitoritem/loadhhvisitoritembyid',
        method: 'post',
        data: params
    })
}

// 增加访客项
export function addHHVisitorItem(params) {
    return request({
        url: '/hhvisitoritem/savehhvisitoritems',
        method: 'post',
        data: params
    })
}
// 更新访客项
export function updateHHVisitorItem(params) {
    return request({
        url: '/hhvisitoritem/savehhvisitoritems',
        method: 'post',
        data: params
    })
}
//
// 删除访客项
export function removeHHVisitorItem(params) {
    return request({
        url: '/hhvisitoritem/removehhvisitoritem',
        method: 'post',
        data: params
    })
}
