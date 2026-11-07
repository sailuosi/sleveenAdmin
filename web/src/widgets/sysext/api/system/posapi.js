import request from '@/utils/requestAPI6.js'

export function loadPostlist(params){
    return request({
        url: '/api/sysPos/list',
        method: 'post',
        data: params
    })
}

export function addPost(params){
    return request({
        url: '/api/sysPos/add',
        method: 'post',
        data: params
    })
}


export function setPost(params){
    return request({
        url: '/api/sysPos/update',
        method: 'post',
        data: params
    })
}


export function rmPost(params){
    return request({
        url: '/api/sysPos/delete',
        method: 'post',
        data: params
    })
}
