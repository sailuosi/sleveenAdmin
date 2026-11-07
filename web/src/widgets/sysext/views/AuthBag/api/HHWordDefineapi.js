import request from '@/utils/requestAPI6.js'

// 查询词条定义列表
export function selectHHWordDefinelist(params) {
    return request({
        url: '/hhworddefine/hhworddefinelist',
        method: 'post',
        data: params
    })
}
// 根据id获取词条定义
export function getHHWordDefineById(params) {
    return request({
        url: '/hhworddefine/loadhhworddefinebyid',
        method: 'post',
        data: params
    })
}

// 增加词条定义
export function addHHWordDefine(params) {
    return request({
        url: '/hhworddefine/savehhworddefines',
        method: 'post',
        data: params
    })
}
// 更新词条定义
export function updateHHWordDefine(params) {
    return request({
        url: '/hhworddefine/savehhworddefines',
        method: 'post',
        data: params
    })
}
//
// 删除词条定义
export function removeHHWordDefine(params) {
    return request({
        url: '/hhworddefine/removehhworddefine',
        method: 'post',
        data: params
    })
}


export function loadWordsForSelect(params) {
    return request({
        url: '/hhworddefine/worddefineopt',
        method: 'post',
        data: params
    })
}