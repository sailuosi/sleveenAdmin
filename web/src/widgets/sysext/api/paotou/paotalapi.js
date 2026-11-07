import request from '@/utils/requestAPI6.js';

/** 兼容旧 paotalconfig 调用：一期主实体转发到 Biz_* 端点；Role/Link 等仍走旧 Aux。 */

function bizList(base, params = {}) {
  const filters = { ...(params.filters || {}) };
  Object.keys(params).forEach((k) => {
    if (['pageNum', 'pageSize', 'page', 'keyword', 'orderBy', 'filters', 'conditions'].includes(k)) return;
    if (params[k] != null && params[k] !== '') filters[k] = String(params[k]);
  });
  return request({
    url: `${base}/qlist`,
    method: 'post',
    data: {
      pageNum: params.pageNum || params.page || 1,
      pageSize: params.pageSize || 20,
      keyword: params.keyword,
      filters,
    },
  }).then((res) => {
    const items = res?.Items || res?.records || res?.Records || [];
    const total = res?.Total ?? res?.total ?? 0;
    return { Items: items, records: items, Total: total, total };
  });
}

function bizOne(base, id) {
  return request({ url: `${base}/qone/${id}`, method: 'get' });
}

function bizSave(base, rows) {
  return request({ url: `${base}/save`, method: 'post', data: rows });
}

function bizRemove(base, idOrIds) {
  const ids = Array.isArray(idOrIds) ? idOrIds : [idOrIds];
  return request({ url: `${base}/rmbyids`, method: 'post', data: ids });
}

const portalBase = '/biz/sysportallist';
const pageBase = '/biz/sysportpagelist';
const cellBase = '/biz/sysportcelllist';
const linkBase = '/biz/sysportcelllinklist';
const appletBase = '/biz/sysappletlist';
const cataBase = '/biz/sysappletcatalist';

export function fetchSysPortalPage(params) { return bizList(portalBase, params); }
export function fetchSysPortal(id) { return bizOne(portalBase, id); }
export function createSysPortal(rows) { return bizSave(portalBase, rows); }
export function updateSysPortal(rows) { return bizSave(portalBase, rows); }
export function deleteSysPortal(id) { return bizRemove(portalBase, id); }

export function fetchSysPortPagePage(params) { return bizList(pageBase, params); }
export function fetchSysPortPage(id) { return bizOne(pageBase, id); }
export function saveSysPortPage(rows) { return bizSave(pageBase, rows); }
export function deleteSysPortPage(id) { return bizRemove(pageBase, id); }

export function fetchSysPortCellPage(params) { return bizList(cellBase, params); }
export function fetchSysPortCell(id) { return bizOne(cellBase, id); }
export function saveSysPortCell(rows) { return bizSave(cellBase, rows); }
export function deleteSysPortCell(id) { return bizRemove(cellBase, id); }

export function fetchSysPortCellLinkPage(params) { return bizList(linkBase, params); }
export function fetchSysPortCellLink(id) { return bizOne(linkBase, id); }
export function saveSysPortCellLink(rows) { return bizSave(linkBase, rows); }
export function deleteSysPortCellLink(id) { return bizRemove(linkBase, id); }

export function fetchSysAppletPage(params) { return bizList(appletBase, params); }
export function fetchSysApplet(id) { return bizOne(appletBase, id); }
export function saveSysApplet(rows) { return bizSave(appletBase, rows); }
export function deleteSysApplet(id) { return bizRemove(appletBase, id); }

export function fetchSysAppletCataPage(params) { return bizList(cataBase, params); }
export function fetchSysAppletCata(id) { return bizOne(cataBase, id); }
export function saveSysAppletCata(rows) { return bizSave(cataBase, rows); }
export function deleteSysAppletCata(id) { return bizRemove(cataBase, id); }

export function fetchSysPortalRolePage(params) {
  return request({ url: '/api/sysPortalRole/page', method: 'get', params });
}
export function saveSysPortalRole(rows) {
  return request({ url: '/api/sysPortalRole', method: 'post', data: rows });
}
export function createSysPortalRole(rows) { return saveSysPortalRole(rows); }
export function updateSysPortalRole(rows) { return saveSysPortalRole(rows); }
export function deleteSysPortalRole(id) {
  return request({ url: `/api/sysPortalRole/${id}`, method: 'delete' });
}

export function fetchSysPortalLinkPage(params) {
  return request({ url: '/api/sysPortalLink/page', method: 'get', params });
}
export function saveSysPortalLink(rows) {
  return request({ url: '/api/sysPortalLink', method: 'post', data: rows });
}

export function fetchSysPageSelectPage(params) {
  return request({ url: '/api/sysPageSelect/page', method: 'get', params });
}
export function saveSysPageSelect(rows) {
  return request({ url: '/api/sysPageSelect', method: 'post', data: rows });
}

export function fetchSysPageFieldPage(params) {
  return request({ url: '/api/sysPageField/page', method: 'get', params });
}
export function saveSysPageField(rows) {
  return request({ url: '/api/sysPageField', method: 'post', data: rows });
}

export function fetchSysPageConnectPage(params) {
  return request({ url: '/api/sysPageConnect/page', method: 'get', params });
}
export function saveSysPageConnect(rows) {
  return request({ url: '/api/sysPageConnect', method: 'post', data: rows });
}

export function setCellNameBycont(params) {
  return bizSave(cellBase, [params]);
}
export function addCelllLinks(params) {
  return bizSave(linkBase, params);
}

export function selectUserWithDefaultPage() {
  return Promise.resolve({ records: [] });
}
export function saveUseDefaultPage() {
  return Promise.resolve({});
}
export function removeUseDefaultPage() {
  return Promise.resolve({});
}
export function PortPageAccessList() {
  return Promise.resolve({ records: [] });
}
