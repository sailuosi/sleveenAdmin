import request from '@/utils/requestAPI6.js'

export function apiSysServerServerBaseGet(params){
    return request({
        url: '/api/sysServer/serverBase',
        method: 'get',
        data: params
    })
}

export function apiSysServerServerUsedGet(params){
    return request({
        url: '/api/sysServer/serverUsed',
        method: 'get',
        data: params
    })
}

export function apiSysServerServerDiskGet(params){
    return request({
        url: '/api/sysServer/serverDisk',
        method: 'get',
        data: params
    })
}

export function apiSysServerAssemblyListGet(params){
    return request({
        url: '/api/sysServer/assemblyList',
        method: 'get',
        data: params
    })
}
