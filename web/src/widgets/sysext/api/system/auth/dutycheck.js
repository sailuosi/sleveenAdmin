import request from '@/utils/requestAPI6.js'


export function loadHisRoles(params) {
    return request({
        url: '/hhdutyitem/loadhisroles',
        method: 'post',
        data: params
    })
}


export function loadhisVisitors(params) {
    return request({
        url: '/hhdutyitem/loadhisVisitors',
        method: 'post',
        data: params
    })
}


export function loadhisDutys(params) {
    return request({
        url: '/hhdutyitem/loadhisDutys',
        method: 'post',
        data: params
    })
}

export function loadHisWorkors(params) {
    return request({
        url: '/hhdutyitem/loadHisWorkors',
        method: 'post',
        data: params
    })
}

export function loadHisManConfs(params) {
    return request({
        url: '/hhdutyitem/loadHisManConfs',
        method: 'post',
        data: params
    })
}


export function clearHisCache(params) {
    return request({
        url: '/hhdutyitem/clearHisCache',
        method: 'post',
        data: params
    })
}


export function loadAppPageconfig(params) {
    return request({
        url: '/hoportpage/loadApppPageconfig',
        method: 'post',
        data: params
    })
}


export function loginToCCFlow(params) {
    return request({
        url: '/api/sysAuth/ccflowlogin',
        method: 'post',
        data: params
    })
}
