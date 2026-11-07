import request from '@/utils/requestAPI6.js'

// 查询访客集合列表
export function selectHHVisitorBaglist(params) {
    return request({
        url: '/hhvisitorbag/hhvisitorbaglist',
        method: 'post',
        data: params
    })
}
// 根据id获取访客集合
export function getHHVisitorBagById(params) {
    return request({
        url: '/hhvisitorbag/loadhhvisitorbagbyid',
        method: 'post',
        data: params
    })
}

// 增加访客集合
export function addHHVisitorBag(params) {
    return request({
        url: '/hhvisitorbag/savehhvisitorbags',
        method: 'post',
        data: params
    })
}
// 更新访客集合
export function updateHHVisitorBag(params) {
    return request({
        url: '/hhvisitorbag/savehhvisitorbags',
        method: 'post',
        data: params
    })
}
//
// 删除访客集合
export function removeHHVisitorBag(params) {
    return request({
        url: '/hhvisitorbag/removehhvisitorbag',
        method: 'post',
        data: params
    })
}
