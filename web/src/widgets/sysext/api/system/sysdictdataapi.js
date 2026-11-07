import request from '@/utils/requestAPI6.js'

// 查询部门信息
export function apiSysDictDataDataListCodeGet(code) {
    return request({
        url: '/api/sysDictData/dataList/' + code,
        method: 'get',
        //data: data
    })
}