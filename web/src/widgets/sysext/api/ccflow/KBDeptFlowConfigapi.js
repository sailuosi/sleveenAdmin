import request from '@/utils/requestAPI6.js'

// 查询部门流程配置列表
export function selectKBDeptFlowConfiglist(params) {
    return request({
        url: '/kbdeptflowconfig/kbdeptflowconfiglist',
        method: 'post',
        data: params
    })
}
// 根据id获取部门流程配置
export function getKBDeptFlowConfigById(params) {
    return request({
        url: '/kbdeptflowconfig/loadkbdeptflowconfigbyid',
        method: 'post',
        data: params
    })
}

// 根据id复制部门流程配置
export function copyKBDeptFlowConfigById(params) {
    return request({
        url: '/kbdeptflowconfig/copydeptflowconfigbyid',
        method: 'post',
        data: params
    })
}


// 增加部门流程配置
export function addKBDeptFlowConfig(params) {
    return request({
        url: '/kbdeptflowconfig/savekbdeptflowconfigs',
        method: 'post',
        data: params
    })
}
// 更新部门流程配置
export function updateKBDeptFlowConfig(params) {
    return request({
        url: '/kbdeptflowconfig/savekbdeptflowconfigs',
        method: 'post',
        data: params
    })
}
//
// 删除部门流程配置
export function removeKBDeptFlowConfig(params) {
    return request({
        url: '/kbdeptflowconfig/removekbdeptflowconfig',
        method: 'post',
        data: params
    })
}

export function loadDeptConfigFlow(params) {
    return request({
        url: '/kbdeptflowconfig/loaddeptconfig',
        method: 'post',
        data: params
    })
}


