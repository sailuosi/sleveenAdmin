import request from '@/utils/requestAPI6.js'

// 查询流程实例列表
export function selectCCGeneFlowlist(params) {
    return request({
        url: '/ccgeneflow/ccgeneflowlist',
        method: 'post',
        data: params
    })
}
// 根据id获取流程实例
export function getCCGeneFlowById(params) {
    return request({
        url: '/ccgeneflow/loadccgeneflowbyid',
        method: 'post',
        data: params
    })
}

// 增加流程实例
export function addCCGeneFlow(params) {
    return request({
        url: '/ccgeneflow/saveccgeneflows',
        method: 'post',
        data: params
    })
}
// 更新流程实例
export function updateCCGeneFlow(params) {
    return request({
        url: '/ccgeneflow/saveccgeneflows',
        method: 'post',
        data: params
    })
}
//
// 删除流程实例
export function removeCCGeneFlow(params) {
    return request({
        url: '/ccgeneflow/removeccgeneflow',
        method: 'post',
        data: params
    })
}
