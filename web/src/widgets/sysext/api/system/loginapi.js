import request from '@/utils/requestAPI6.js'

export function loginByTen4Token(code) {
    return request({
        url: '/api/sysAuth/ten4login',
        method: 'post',
        data: code
    })
}


export function LoginBySSOTicket(code) {
    return request({
        url: '/api/sysAuth/ssoticketlogin',
        method: 'post',
        data: code
    })
}