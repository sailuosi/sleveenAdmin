import request from '@/utils/requestAPI6.js'

// 查询资源集合列表
export function selectHHGoodsBaglist(params) {
    return request({
        url: '/hhgoodsbag/hhgoodsbaglist',
        method: 'post',
        data: params
    })
}
// 根据id获取资源集合
export function getHHGoodsBagById(params) {
    return request({
        url: '/hhgoodsbag/loadhhgoodsbagbyid',
        method: 'post',
        data: params
    })
}

// 增加资源集合
export function addHHGoodsBag(params) {
    return request({
        url: '/hhgoodsbag/savehhgoodsbags',
        method: 'post',
        data: params
    })
}
// 更新资源集合
export function updateHHGoodsBag(params) {
    return request({
        url: '/hhgoodsbag/savehhgoodsbags',
        method: 'post',
        data: params
    })
}
//
// 删除资源集合
export function removeHHGoodsBag(params) {
    return request({
        url: '/hhgoodsbag/removehhgoodsbag',
        method: 'post',
        data: params
    })
}


