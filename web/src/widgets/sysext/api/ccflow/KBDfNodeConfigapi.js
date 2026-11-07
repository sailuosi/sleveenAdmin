import request from '@/utils/requestAPI6.js'

// 查询部门流程节点配置列表
export function selectKBDfNodeConfiglist(params) {
    return request({
        url: '/kbdfnodeconfig/kbdfnodeconfiglist',
        method: 'post',
        data: params
    })
}
// 根据id获取部门流程节点配置
export function getKBDfNodeConfigById(params) {
    return request({
        url: '/kbdfnodeconfig/loadkbdfnodeconfigbyid',
        method: 'post',
        data: params
    })
}

// 增加部门流程节点配置
export function addKBDfNodeConfig(params) {
    return request({
        url: '/kbdfnodeconfig/savekbdfnodeconfigs',
        method: 'post',
        data: params
    })
}
// 更新部门流程节点配置
export function updateKBDfNodeConfig(params) {
    return request({
        url: '/kbdfnodeconfig/savekbdfnodeconfigs',
        method: 'post',
        data: params
    })
}
//
// 删除部门流程节点配置
export function removeKBDfNodeConfig(params) {
    return request({
        url: '/kbdfnodeconfig/removekbdfnodeconfig',
        method: 'post',
        data: params
    })
}
