import request from '@/utils/requestAPI6.js'

// 查询门户配置列表
export function selectZHPortallist(params) {
    return request({
        url: '/zhportal/zhportallist',
        method: 'post',
        data: params
    })
}
// 根据id获取门户配置
export function getZHPortalById(params) {
    return request({
        url: '/zhportal/loadzhportalbyid',
        method: 'post',
        data: params
    })
}

// 增加门户配置
export function addZHPortal(params) {
    return request({
        url: '/zhportal/savezhportals',
        method: 'post',
        data: params
    })
}
// 更新门户配置
export function updateZHPortal(params) {
    return request({
        url: '/zhportal/savezhportals',
        method: 'post',
        data: params
    })
}
//
// 删除门户配置
export function removeZHPortal(params) {
    return request({
        url: '/zhportal/removezhportal',
        method: 'post',
        data: params
    })
}

export function getPoartalUses(params) {
    return request({
        url: '/zhportal/getPoartalUses',
        method: 'post',
        data: params
    })
}


// 查询门户区块列表
export function selectZHPortCelllist(params) {
    return request({
        url: '/zhportcell/zhportcelllist',
        method: 'post',
        data: params
    })
}
// 通过portalpage关联查询portalcell
export function selectZHPortCellByPortalPage(params) {
  return request({
    url: '/zhportcell/zhportcelllistByPortalPage',
    method: 'post',
    data: params
  })
}
// 根据id获取门户区块
export function getZHPortCellById(params) {
    return request({
        url: '/zhportcell/loadzhportcellbyid',
        method: 'post',
        data: params
    })
}

// 增加门户区块
export function addZHPortCell(params) {
    return request({
        url: '/zhportcell/savezhportcells',
        method: 'post',
        data: params
    })
}
// 更新门户区块
export function updateZHPortCell(params) {
    return request({
        url: '/zhportcell/savezhportcells',
        method: 'post',
        data: params
    })
}
//
export function setCellNameBycont(params) {
    return request({
        url: '/zhportcell/setcellnamebycont',
        method: 'post',
        data: params
    })
}

// 删除门户区块
export function removeZHPortCell(params) {
    return request({
        url: '/zhportcell/removezhportcell',
        method: 'post',
        data: params
    })
}




// 查询区块组件列表
export function selectZHCellLinklist(params) {
    return request({
        url: '/zhcelllink/zhcelllinklist',
        method: 'post',
        data: params
    })
}
// 根据id获取区块组件
export function getZHCellLinkById(params) {
    return request({
        url: '/zhcelllink/loadzhcelllinkbyid',
        method: 'post',
        data: params
    })
}

// 增加区块组件
export function addZHCellLink(params) {
    return request({
        url: '/zhcelllink/savezhcelllinks',
        method: 'post',
        data: params
    })
}
// 更新区块组件
export function updateZHCellLink(params) {
    return request({
        url: '/zhcelllink/savezhcelllinks',
        method: 'post',
        data: params
    })
}
//
// 删除区块组件
export function removeZHCellLink(params) {
    return request({
        url: '/zhcelllink/removezhcelllink',
        method: 'post',
        data: params
    })
}

export function loadPaotallist(params) {
    return request({
        url: '/zhcelllink/loadPaotallist',
        method: 'post',
        data: params
    })
}

export function addCelllLinks(params) {
    return request({
        url: '/zhcelllink/addCelllLinks',
        method: 'post',
        data: params
    })
}

export function getCellLinkUses(params) {
    return request({
        url: '/zhcelllink/getCellLinkUses',
        method: 'post',
        data: params
    })
}


// 查询门户组件分类列表
export function selectZHAppletCatalist(params) {
    return request({
        url: '/zhappletcata/zhappletcatalist',
        method: 'post',
        data: params
    })
}
// 根据id获取门户组件分类
export function getZHAppletCataById(params) {
    return request({
        url: '/zhappletcata/loadzhappletcatabyid',
        method: 'post',
        data: params
    })
}

// 增加门户组件分类
export function addZHAppletCata(params) {
    return request({
        url: '/zhappletcata/savezhappletcatas',
        method: 'post',
        data: params
    })
}
// 更新门户组件分类
export function updateZHAppletCata(params) {
    return request({
        url: '/zhappletcata/savezhappletcatas',
        method: 'post',
        data: params
    })
}
//
// 删除门户组件分类
export function removeZHAppletCata(params) {
    return request({
        url: '/zhappletcata/removezhappletcata',
        method: 'post',
        data: params
    })
}


// 查询门户组件列表
export function selectZHAppletlist(params) {
    return request({
        url: '/zhapplet/zhappletlist',
        method: 'post',
        data: params
    })
}
// 根据id获取门户组件
export function getZHAppletById(params) {
    return request({
        url: '/zhapplet/loadzhappletbyid',
        method: 'post',
        data: params
    })
}

// 增加门户组件
export function addZHApplet(params) {
    return request({
        url: '/zhapplet/savezhapplets',
        method: 'post',
        data: params
    })
}
// 更新门户组件
export function updateZHApplet(params) {
    return request({
        url: '/zhapplet/savezhapplets',
        method: 'post',
        data: params
    })
}
//
// 删除门户组件
export function removeZHApplet(params) {
    return request({
        url: '/zhapplet/removezhapplet',
        method: 'post',
        data: params
    })
}

// 查询门户角色列表
export function selectZHPortalRolelist(params) {
    return request({
        url: '/zhportalrole/zhportalrolelist',
        method: 'post',
        data: params
    })
}
// 根据id获取门户角色
export function getZHPortalRoleById(params) {
    return request({
        url: '/zhportalrole/loadzhportalrolebyid',
        method: 'post',
        data: params
    })
}

// 增加门户角色
export function addZHPortalRole(params) {
    return request({
        url: '/zhportalrole/savezhportalroles',
        method: 'post',
        data: params
    })
}
// 更新门户角色
export function updateZHPortalRole(params) {
    return request({
        url: '/zhportalrole/savezhportalroles',
        method: 'post',
        data: params
    })
}
//
// 删除门户角色
export function removeZHPortalRole(params) {
    return request({
        url: '/zhportalrole/removezhportalrole',
        method: 'post',
        data: params
    })
}



// 查询门户页面列表
export function selectZHPortPagelist(params) {
    return request({
        url: '/zhportpage/zhportpagelist',
        method: 'post',
        data: params
    })
}
// 根据id获取门户页面
export function getZHPortPageById(params) {
    return request({
        url: '/zhportpage/loadzhportpagebyid',
        method: 'post',
        data: params
    })
}

// 增加门户页面
export function addZHPortPage(params) {
    return request({
        url: '/zhportpage/savezhportpages',
        method: 'post',
        data: params
    })
}
// 更新门户页面
export function updateZHPortPage(params) {
    return request({
        url: '/zhportpage/savezhportpages',
        method: 'post',
        data: params
    })
}
//
// 删除门户页面
export function removeZHPortPage(params) {
    return request({
        url: '/zhportpage/removezhportpage',
        method: 'post',
        data: params
    })
}

//加载门户页面的配置
export function loadPageConfig(key){
    return request({
        url: '/zhportpage/loadpageconfig',
        method: 'post',
        data: key
    })
}

//加载门户页面的配置
export function PortPageAccessList(key){
    return request({
        url: '/zhportpage/pageacc',
        method: 'post',
        data: key
    })
}

//以用户表为主表加载用户默认页面列表
export function selectUserWithDefaultPage(data){
    return request({
        url: '/zhportpage/userwithdefaultpage',
        method: 'post',
        data: data
    })
}

//保存用户默认页面
export function saveUseDefaultPage(data){
    return request({
        url: '/zhportpage/saveuserdefaultpage',
        method: 'post',
        data: data
    })
}

//删除用户默认页面
export function removeUseDefaultPage(data){
    return request({
        url: '/zhportpage/removeuserdefaultpage',
        method: 'post',
        data: data
    })
}

//查询用户是否拥有看板使用情况报表权限
export function isShowKbReport(data){
    return request({
        url: '/zhportpage/isshowkbreport',
        method: 'post',
        data: data
    })
}

export function listKbReportData(){
    return request({
        url: '/zhportpage/listkbreportdata',
        method: 'post'
    })
}

export function listUserPower(data){
    return request({
        url: '/zhportpage/listuserpower',
        method: 'post',
        data: data
    })
}

export function saveKbReportData(data){
    return request({
        url: '/zhportpage/savekbreportdata',
        method: 'post',
        data: data
    })
}

export function selectReportUser(data){
    return request({
        url: '/zhportpage/selectreportuser',
        method: 'post',
        data: data
    })
}
