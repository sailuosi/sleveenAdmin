import request from '@/utils/requestAPI6.js'

// 查询流程业务配置列表
export function selectCCBizFlowlist(params) {
    return request({
        url: '/ccbizflow/ccbizflowlist',
        method: 'post',
        data: params
    })
}
// 根据id获取流程业务配置
export function getCCBizFlowById(params) {
    return request({
        url: '/ccbizflow/loadccbizflowbyid',
        method: 'post',
        data: params
    })
}

// 增加流程业务配置
export function addCCBizFlow(params) {
    return request({
        url: '/ccbizflow/saveccbizflows',
        method: 'post',
        data: params
    })
}
// 更新流程业务配置
export function updateCCBizFlow(params) {
    return request({
        url: '/ccbizflow/saveccbizflows',
        method: 'post',
        data: params
    })
}
//
// 删除流程业务配置
export function removeCCBizFlow(params) {
    return request({
        url: '/ccbizflow/removeccbizflow',
        method: 'post',
        data: params
    })
}


// 删除流程业务配置
export function loadStartFlow(params) {
    return request({
        url: '/ccbizflow/loaddeptconfig',
        method: 'post',
        data: params
    })
}
