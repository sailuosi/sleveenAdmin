import request from '@/utils/requestAPI6.js'

// 查询列表
export function querylist(params) {
  return request({
    url: '/PushRecord/list',
    method: 'get',
    params: params
  })
}


