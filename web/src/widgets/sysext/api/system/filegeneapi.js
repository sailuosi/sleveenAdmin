import request from '@/utils/requestAPI6.js'

// 查询门户配置列表
export function selectAttachmentlist(params) {
    return request({
        url: '/filegene/attachmentlist',
        method: 'post',
        data: params
    })
}

// 查询任务进度附件列表
export function selectAttachmentlistForTaskDetail(params) {
    return request({
        url: '/filegene/attachmentlistForTD',
        method: 'post',
        data: params
    })
}


export function deleteGeneFile(params) {
    return request({
        url: '/filegene/removegenefile',
        method: 'post',
        data: params
    })
}

export function downloadFile(params) {
    return request({
        url: '/filegene/downloadFile',
        method: 'post',
        data: params
    })
}

export function downloadFileBlob(fileId, headers) {
    return request({
        url: '/filegene/downloadFile?oid=' + fileId,
        method: 'get',
        headers: headers || {}
    })
}