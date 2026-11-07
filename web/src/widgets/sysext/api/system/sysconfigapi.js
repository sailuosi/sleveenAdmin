import request from '@/utils/requestAPI6.js'

// 查询部门信息
export function apiSysConfigPagePost(data) {
    return request({
        url: '/api/sysConfig/page',
        method: 'post',
        data: data
    })
}

export function apiSysConfigGroupListGet(data) {
    return request({
        url: '/api/sysConfig/groupList',
        method: 'get',
        //data: data
    })
}



export function apiSysConfigDeletePost(data) {
    return request({
        url: '/api/sysConfig/delete',
        method: 'post',
        data: data
    })
}


export function apiSysConfigBatchDeletePost(data) {
    return request({
        url: '/api/sysConfig/batchDelete',
        method: 'post',
        data: data
    })
}

export function apiSysConfigAddPost(data) {
    return request({
        url: '/api/sysConfig/add',
        method: 'post',
        data: data
    })
}

export function apiSysConfigUpdatePost(data) {
    return request({
        url: '/api/sysConfig/update',
        method: 'post',
        data: data
    })
}