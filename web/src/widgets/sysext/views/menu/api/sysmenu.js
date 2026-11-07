import request from '@/utils/requestAPI6.js'

export function apiSysMenuListGet(params){
    return request({
        url: '/api/sysMenu/list',
        method: 'get',
 
    })
}


export function apiSysMenuUpdatePost(params){
    return request({
        url: '/api/sysMenu/update',
        method: 'post',
        data:params
    })
}

export function apiSysMenuAddPost(params){
    return request({
        url: '/api/sysMenu/add',
        method: 'post',
        data:params
    })
}

export function apiSysMenuDeletePost(params){
    return request({
        url: '/api/sysMenu/delete',
        method: 'post',
        data:params
    })
}

export function apiSysMenuInitButtonsPost(params){
    return request({
        url: '/api/sysMenu/initButtons',
        method: 'post',
        data: params
    })
}