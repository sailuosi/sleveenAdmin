import request from '@/utils/requestAPI6.js'

// 查询部门信息
export function apiSysCacheKeyListGet(data) {
    return request({
        url: '/api/sysCache/keyList',
        method: 'get',
        data: data
    })
}

export function apiSysCacheValueKeyGet(key) {
    return request({
        url: '/api/sysCache/value/'+key,
        method: 'get',
        //data: data
    })
}

export function apiSysCacheDeleteKeyPost(key) {
    return request({
        url: '/api/sysCache/delete/'+key,
        method: 'post',
        //data: data
    })
}

