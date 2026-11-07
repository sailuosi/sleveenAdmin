import request from '@/utils/requestAPI6.js'

export function apiSysOrgListGet(params){
    return request({
        url: '/api/sysOrg/list',
        method: 'get',
        query: params
    })
}


export function apiSysOrgUpdatePost(params){
    return request({
        url: '/api/sysOrg/update',
        method: 'post',
        data: params
    })
}

export function apiSysOrgAddPost(params){
    return request({
        url: '/api/sysOrg/add',
        method: 'post',
        data: params
    })
}
