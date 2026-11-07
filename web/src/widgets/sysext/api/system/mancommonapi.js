import request from '@/utils/requestAPI6.js'

export function loadManSearch(params){
    return request({
        url: '/mancomm/searchtxt',
        method: 'post',
        data: params
        /*{
            value:key,
            deptcode:deptcode
        }*/
    })
}

export function loadOrgTree(data){
    return request({
        url: '/mancomm/loadorgtree',
        method: 'post',
        data: data
    });
}

export function queryorglist(data){
    return request({
        url: '/mancomm/queryorglist',
        method: 'post',
        data: data
    });
}

export function loadOrgParent(data){
    return request({
        url: '/mancomm/loadOrgParent',
        method: 'post',
        data: data
    });
}

export function loadThisOrgTree(data){
    return request({
        url: '/mancomm/loadthisorgtree',
        method: 'post',
        data: data
    });
}

export function searchAnyRoleByTxt(key){

    return request({
        url: '/mancomm/searchroletxt',
        method: 'post',
        data:{
            value:key
        }
    })
}


export function loadDeptMan(data){

    return request({
        url: '/mancomm/loadDeptMan',
        method: 'post',
        data:data
    })
}

export function calcOrgDeep(data){

    return request({
        url: '/mancomm/calcOrgDeep',
        method: 'post',
        data:data
    })
}

