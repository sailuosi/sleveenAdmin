import request from '@/utils/requestAPI6.js'

export function loadRegionlist(params){
    return request({
        url: '/api/sysRegion/page',
        method: 'post',
        data: params
    })
}

export function loadRegionAll(params){
    return request({
        url: '/api/sysRegion/list',
        method: 'post',
        data: params
    })
}


export function addRegion(params){
    return request({
        url: '/api/sysRegion/add',
        method: 'post',
        data: params
    })
}


export function setRegion(params){
    return request({
        url: '/api/sysRegion/update',
        method: 'post',
        data: params
    })
}


export function rmRegion(params){
    return request({
        url: '/api/sysRegion/delete',
        method: 'post',
        data: params
    })
}


export function syncRegion(params){
    return request({
        url: '/api/sysRegion/sync',
        method: 'post',
        data: params
    })
}
