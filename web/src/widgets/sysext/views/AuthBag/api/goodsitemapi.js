import request from '@/utils/requestAPI6.js'

// 查询资源项列表
export function selectHHGoodsItemlist(params) {
    return request({
        url: '/hhgoodsitem/hhgoodsitemlist',
        method: 'post',
        data: params
    })
}
// 根据id获取资源项
export function getHHGoodsItemById(params) {
    return request({
        url: '/hhgoodsitem/loadhhgoodsitembyid',
        method: 'post',
        data: params
    })
}

// 增加资源项
export function addHHGoodsItem(params) {
    return request({
        url: '/hhgoodsitem/savehhgoodsitems',
        method: 'post',
        data: params
    })
}
// 更新资源项
export function updateHHGoodsItem(params) {
    return request({
        url: '/hhgoodsitem/savehhgoodsitems',
        method: 'post',
        data: params
    })
}
//
// 删除资源项
export function removeHHGoodsItem(params) {
    return request({
        url: '/hhgoodsitem/removehhgoodsitem',
        method: 'post',
        data: params
    })
}

export function loadBindKeys(params) {
    return request({
        url: '/hhgoodsitem/loadbindkeys',
        method: 'post',
        data: params
    })
}

export function saveBindKeys(params) {
    return request({
        url: '/hhgoodsitem/savebindkeys',
        method: 'post',
        data: params
    })
}

// 批量设置资源项角色
export function batchSetHHGoodsItemRole(params) {
    return request({
        url: '/hhgoodsitem/batchsetrole',
        method: 'post',
        data: params
    })
}
