import request from '@/utils/requestAPI6.js'



export function getPortConfig(key){
    //console.log("getPortConfig")
    return request({
        url: '/portal/common/loadportalconfig',
        method: 'post',
        data:{
            value:key
        }
    })
}

export function getPortConfigByOid(oid){
    return request({
        url: '/portal/common/loadportalconfigbyoid',
        method: 'post',
        data:{
            value: oid
        }
    })
}

/**
 * 清除门户配置缓存（服务端 OID/Key/首页解析）。
 * @param {string} oid 门户 OID
 * @param {string} [portalKey] 门户 P_Key，传入可少一次查库
 */
export function clearPortConfigByOid(oid, portalKey){
    return request({
        url: '/portal/common/clearportalconfigbyoid',
        method: 'post',
        data:{
            value: oid,
            key: portalKey || undefined
        }
    })
}

/** 首页按页面编码选择门户Key */
export function getHomePortal(pageCode = 'home'){
    return request({
        url: '/portal/common/loadhomeportal',
        method: 'post',
        data: {
            value: pageCode
        }
    })
}

/**
 * 子门户外壳：按页面编码列出可选门户（切换下拉）。
 * @param {string} pageCode 页面编码 / P_Key（如 BidReviewDesk）
 */
export function loadPortalList(pageCode = 'home'){
    return request({
        url: '/portal/common/loadportallist',
        method: 'post',
        data: {
            value: pageCode
        }
    })
}

/**
 * 保存门户格子布局。
 * @param {string|object[]} cellsOrJson 格子数组，或 JSON 字符串（兼容旧调用）
 * @param {string} portalId 门户 OID
 */
export function savePortCells(cellsOrJson, portalId){
    let cells = cellsOrJson;
    if (typeof cellsOrJson === 'string') {
        try {
            cells = JSON.parse(cellsOrJson);
        } catch {
            cells = [];
        }
    }
    if (!Array.isArray(cells)) cells = [];
    return request({
        url: '/portal/common/savecell',
        method: 'post',
        data: {
            portalId,
            cells
        }
    })
}

//最新项目进展
export function getLastProjReport(val){
    return request({
        url: '/portal/common/lastppreport',
        method: 'post',
        data:{value:val}
    })
}


//最新项目进展
export function getRelocateSumm(val){
    return request({
        url: '/portal/common/RelocateSumm',
        method: 'post',
        data:{
            value:val,
        }
    })

}


//近期隐患清单
export function getRencentTroubles(val){
    return request({
        url: '/portal/common/RencentTroubles',
        method: 'post',
        data:{
            value:val,
        }
    })
}
//风险清单
export function getRencentRisk(val){
    return request({
        url: '/portal/common/RencentRisk',
        method: 'post',
        data:{
            value:val,
        }
    })
}


//水质数据
export function getRencentWaterQuality(val){
    return request({
        url: '/portal/common/RencentWaterQuality',
        method: 'post',
        data:{
            value:val,
        }
    })
}


//搬迁协议统计
export function getRelocateSummary(val){
    return request({
        url: '/portal/common/RelocateSummary',
        method: 'post',
        data:{
            value:val,
        }
    })
}

//年度沉陷统计
export function getSubsideYearSum(val){
    return request({
        url: '/portal/common/SubsideYearSum',
        method: 'post',
        data:{
            value:val,
        }
    })
}


//土地按用途归类
export function getLandUsetoSum(val){
    return request({
        url: '/portal/common/SubsideYearSum',
        method: 'post',
        data:{
            value:val,
        }
    })
}


//门户消息
export function getPortalNotings(val){
    return request({
        url: '/portal/common/PortalNotings',
        method: 'post',
        data:{
            value:val,
        }
    })
}