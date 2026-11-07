import request from '@/utils/requestAPI6.js'

// 查询部门信息
export function apiSysFileUploadAvatarPostForm(data) {
    return request({
        url: '/api/sysFile/uploadAvatar',
        method: 'post',
        headers: { 'content-type': 'multipart/form-data' },
        data: data
    })
}


export function apiSysFileUploadSignaturePostForm(data) {
    return request({
        url: '/api/sysFile/uploadSignature',
        method: 'post',
        data: data
    })
}
