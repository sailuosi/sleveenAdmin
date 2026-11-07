import request from '@/utils/requestAPI6.js'

// 查询词条集合列表
export function selectHHWordBaglist(params) {
    return request({
        url: '/hhwordbag/hhwordbaglist',
        method: 'post',
        data: params
    })
}
// 根据id获取词条集合
export function getHHWordBagById(params) {
    return request({
        url: '/hhwordbag/loadhhwordbagbyid',
        method: 'post',
        data: params
    })
}

// 增加词条集合
export function addHHWordBag(params) {
    return request({
        url: '/hhwordbag/savehhwordbags',
        method: 'post',
        data: params
    })
}
// 更新词条集合
export function updateHHWordBag(params) {
    return request({
        url: '/hhwordbag/savehhwordbags',
        method: 'post',
        data: params
    })
}
//
// 删除词条集合
export function removeHHWordBag(params) {
    return request({
        url: '/hhwordbag/removehhwordbag',
        method: 'post',
        data: params
    })
}
