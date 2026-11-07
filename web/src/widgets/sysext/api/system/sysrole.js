import request from '@/utils/requestAPI6.js'

export function addRoleMancode(params){
    return request({
        url: '/api/sysRole/addrolemancode',
        method: 'post',
        data: params
    })
}

export function apiSysRoleListGet(params){
    return request({
        url: '/api/sysRole/list',
        method: 'get',
        data: params
    })
}