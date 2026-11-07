import request from '@/utils/requestAPI6.js'

// 查询部门信息
export function createEntityMooSQL(data) {
    return request({
        url: '/api/sysDatabase/createEntityMooSQL',
        method: 'post',
        data: data
    })
}

export function geneCodeByTmp(data) {
    return request({
        url: '/codegenetmp/genebytmp',
        method: 'post',
        data: data
    })
}

export function geneCodeEntity(data) {
    return request({
        url: '/api/sysDatabase/createntitytxt',
        method: 'post',
        data: data
    })
}


export function geneFilebytmp(data) {
    return request({
        url: '/codegenetmp/geneFilebytmp',
        method: 'post',
        data: data
    })
}

export function geneEntity(data) {
    return request({
        url: '/codegenetmp/geneEntity',
        method: 'post',
        data: data
    })
}