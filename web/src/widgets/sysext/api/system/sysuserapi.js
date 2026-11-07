

///api/sysUser/baseInfo

import request from '@/utils/requestAPI6.js'

export function apiSysUserBaseInfoGet(params){
    return request({
        url: '/api/sysUser/baseInfo',
        method: 'get',
        data: params
    })
}

export function apiSysUserBaseInfoPost(params){
    return request({
        url: '/api/sysUser/baseInfo',
        method: 'post',
        data: params
    })
}

export function apiSysUserPagePost(params){
    return request({
        url: '/api/sysUser/page',
        method: 'post',
        data: params
    })
}


export function apiSysUserOwnRoleListUserIdGet(userId){
    return request({
        url: '/api/sysUser/ownRoleList/'+userId,
        method: 'get',
        //data: params
    })
}

export function apiSysUserOwnExtOrgListUserIdGet(userId){
    return request({
        url: '/api/sysUser/ownExtOrgList/'+userId,
        method: 'get',
        //data: params
    })
}

export function apiSysUserChangePwdPost(params){
    return request({
        url: '/api/sysUser/changePwd',
        method: 'post',
        data: params
    })
}

export function apiSysUserUpdatePost(params){
    return request({
        url: '/api/sysUser/update',
        method: 'post',
        data: params
    })
}

export function apiSysUserAddPost(params){
    return request({
        url: '/api/sysUser/add',
        method: 'post',
        data: params
    })
}

export function apiSysUserDeletePost(params){
    return request({
        url: '/api/sysUser/delete',
        method: 'post',
        data: params
    })
}

export function apiSysUserSetStatusPost(params){
    return request({
        url: '/api/sysUser/setStatus',
        method: 'post',
        data: params
    })
}

export function apiSysUserResetPwdPost(params){
    return request({
        url: '/api/sysUser/resetPwd',
        method: 'post',
        data: params
    })
}