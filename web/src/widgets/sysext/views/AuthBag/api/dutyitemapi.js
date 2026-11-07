import request from '@/utils/requestAPI6.js'

// 查询责任项列表
export function selectHHDutyItemlist(params) {
    return request({
        url: '/hhdutyitem/hhdutyitemlist',
        method: 'post',
        data: params
    })
}
// 根据id获取责任项
export function getHHDutyItemById(params) {
    return request({
        url: '/hhdutyitem/loadhhdutyitembyid',
        method: 'post',
        data: params
    })
}

// 增加责任项
export function addHHDutyItem(params) {
    return request({
        url: '/hhdutyitem/savehhdutyitems',
        method: 'post',
        data: params
    })
}
// 更新责任项
export function updateHHDutyItem(params) {
    return request({
        url: '/hhdutyitem/savehhdutyitems',
        method: 'post',
        data: params
    })
}
//
// 删除责任项
export function removeHHDutyItem(params) {
    return request({
        url: '/hhdutyitem/removehhdutyitem',
        method: 'post',
        data: params
    })
}

export function cleanDutyCache(params) {
    return request({
        url: '/hhdutyitem/cleancache',
        method: 'post',
        data: params
    })
}