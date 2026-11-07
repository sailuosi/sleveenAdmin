import request from '@/utils/requestAPI6.js'


export function connectID(params) {
    return request({
        url: '/ldkb/connectID',
        method: 'post',
        data: params
    })
}
