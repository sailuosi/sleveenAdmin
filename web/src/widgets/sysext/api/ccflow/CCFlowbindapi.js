import request from '@/utils/requestAPI6.js'

// 查询流程启动范围列表
export function selectCCFlowbindlist(params) {
    return request({
        url: '/ccflowbind/ccflowbindlist',
        method: 'post',
        data: params
    })
}
// 根据id获取流程启动范围
export function getCCFlowbindById(params) {
    return request({
        url: '/ccflowbind/loadccflowbindbyid',
        method: 'post',
        data: params
    })
}

// 增加流程启动范围
export function addCCFlowbind(params) {
    return request({
        url: '/ccflowbind/saveccflowbinds',
        method: 'post',
        data: params
    })
}
// 更新流程启动范围
export function updateCCFlowbind(params) {
    return request({
        url: '/ccflowbind/saveccflowbinds',
        method: 'post',
        data: params
    })
}
//
// 删除流程启动范围
export function removeCCFlowbind(params) {
    return request({
        url: '/ccflowbind/removeccflowbind',
        method: 'post',
        data: params
    })
}
