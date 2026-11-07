import request from '@/utils/requestAPI6.js'

export function sayOoutNotice(params){
    return request({
        url: '/api/sysNotice/sayout',
        method: 'post',
        data: params
    })
}



export function loadSysNoticePaged(params){
    return request({
        url: '/api/sysNotice/page',
        method: 'post',
        data: params
    })
}


export function apiSysNoticeAddPost(params){
    return request({
        url: '/api/sysNotice/add',
        method: 'post',
        data: params
    })
}

export function apiSysNoticeUpdatePost(params){
    return request({
        url: '/api/sysNotice/update',
        method: 'post',
        data: params
    })
}

export function loadNoticeMan(params){
    return request({
        url: '/api/sysNotice/loadnman',
        method: 'post',
        data: params
    })
}

export function addNoticeMan(params){
    return request({
        url: '/api/sysNotice/addnman',
        method: 'post',
        data: params
    })
}

export function removeNoticeMan(params){
    return request({
        url: '/api/sysNotice/rmnman',
        method: 'post',
        data: params
    })
}