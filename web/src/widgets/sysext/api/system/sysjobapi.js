import request from '@/utils/requestAPI6.js'

export function apiSysJobPageJobDetailPost(params){
    return request({
        url: '/api/sysJob/pageJobDetail',
        method: 'post',
        data: params
    })
}

export function apiSysJobDeleteJobDetailPost(params){
    return request({
        url: '/api/sysJob/deleteJobDetail',
        method: 'post',
        data: params
    })
}

export function apiSysJobDeleteJobTriggerPost(params){
    return request({
        url: '/api/sysJob/deleteJobTrigger',
        method: 'post',
        data: params
    })
}

export function apiSysJobStartAllJobPost(params){
    return request({
        url: '/api/sysJob/startAllJob',
        method: 'post',
        data: params
    })
}

export function apiSysJobPauseAllJobPost(params){
    return request({
        url: '/api/sysJob/pauseAllJob',
        method: 'post',
        data: params
    })
}

export function apiSysJobRunJobPost(params){
    return request({
        url: '/api/sysJob/runJob',
        method: 'post',
        data: params
    })
}

export function apiSysJobStartJobPost(params){
    return request({
        url: '/api/sysJob/startJob',
        method: 'post',
        data: params
    })
}

export function apiSysJobPauseJobPost(params){
    return request({
        url: '/api/sysJob/pauseJob',
        method: 'post',
        data: params
    })
}

export function apiSysJobStartTriggerPost(params){
    return request({
        url: '/api/sysJob/startTrigger',
        method: 'post',
        data: params
    })
}

export function apiSysJobPauseTriggerPost(params){
    return request({
        url: '/api/sysJob/pauseTrigger',
        method: 'post',
        data: params
    })
}

export function apiSysJobAddJobDetailPost(params){
    return request({
        url: '/api/sysJob/addJobDetail',
        method: 'post',
        data: params
    })
}

export function apiSysJobUpdateJobDetailPost(params){
    return request({
        url: '/api/sysJob/updateJobDetail',
        method: 'post',
        data: params
    })
}

export function apiSysJobAddJobTriggerPost(params){
    return request({
        url: '/api/sysJob/addJobTrigger',
        method: 'post',
        data: params
    })
}

export function apiSysJobUpdateJobTriggerPost(params){
    return request({
        url: '/api/sysJob/updateJobTrigger',
        method: 'post',
        data: params
    })
}

export function apiSysJobJobClusterListGet(params){
    return request({
        url: '/api/sysJob/jobClusterList',
        method: 'get',
        query: params
    })
}

export function apiSysJobCancelSleepPost(params){
    return request({
        url: '/api/sysJob/cancelSleep',
        method: 'post',
        data: params
    })
}

export function apiSysJobPersistAllPost(params){
    return request({
        url: '/api/sysJob/persistAll',
        method: 'post',
        data: params
    })
}
