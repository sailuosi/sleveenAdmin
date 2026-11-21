import request from '@/utils/requestAPI6.js'

function asFormData(data) {
    if (data instanceof FormData) return data;
    const fm = new FormData();
    // File / Blob 直接挂 file；UploadFile 取 .raw
    const file = data?.raw ?? data;
    fm.append('file', file);
    return fm;
}

// 上传头像
export function apiSysFileUploadAvatarPostForm(data) {
    return request({
        url: '/api/sysFile/uploadAvatar',
        method: 'post',
        headers: { 'Content-Type': 'multipart/form-data' },
        data: asFormData(data)
    })
}

// 上传电子签名
export function apiSysFileUploadSignaturePostForm(data) {
    return request({
        url: '/api/sysFile/uploadSignature',
        method: 'post',
        headers: { 'Content-Type': 'multipart/form-data' },
        data: asFormData(data)
    })
}
