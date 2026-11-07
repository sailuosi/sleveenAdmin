import request from '@/utils/requestAPI6.js'


export function loginToCCFlow(params) {
    return request({
        url: '/api/sysAuth/ccflowlogin',
        method: 'post',
        data: params
    })
}
