import request from '@/utils/requestAPI6.js'

// 查询门户配置列表
export function getCodeValue(code) {
    return request({
        url: '/mancomm/getcodevalue',
        method: 'post',
        data: {value:code}
    })
}
// 根据id获取门户配置
export function getEumValue(params) {
    return request({
        url: '/zhportal/loaportalbyid',
        method: 'post',
        data: params
    })
}


export function apiSysDictTypePagePost(params) {
    return request({
        url: '/api/sysDictType/page',
        method: 'post',
        data: params
    })
}

export function apiSysDictTypeDeletePost(params) {
    return request({
        url: '/api/sysDictType/delete',
        method: 'post',
        data: params
    })
}

export function apiSysDictTypeAddPost(params) {
    return request({
        url: '/api/sysDictType/add',
        method: 'post',
        data: params
    })
}

export function apiSysDictTypeUpdatePost(params) {
    return request({
        url: '/api/sysDictType/update',
        method: 'post',
        data: params
    })
}

export function apiSysDictDataPagePost(params) {
    return request({
        url: '/api/sysDictData/page',
        method: 'post',
        data: params
    })
}

export function apiSysDictDataDeletePost(params) {
    return request({
        url: '/api/sysDictData/delete',
        method: 'post',
        data: params
    })
}

export function apiSysDictDataAddPost(params) {
    return request({
        url: '/api/sysDictData/add',
        method: 'post',
        data: params
    })
}

export function apiSysDictDataUpdatePost(params) {
    return request({
        url: '/api/sysDictData/update',
        method: 'post',
        data: params
    })
}
