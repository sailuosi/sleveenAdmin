import request from '@/utils/portal/requestAPI6.js';

export function fetchSysPortalPage(params) {
  return request({ url: '/api/sysPortal/page', method: 'get', params });
}

export function fetchSysPortal(id) {
  return request({ url: `/api/sysPortal/${id}`, method: 'get' });
}

export function createSysPortal(rows) {
  return request({ url: '/api/sysPortal', method: 'post', data: rows });
}

export function updateSysPortal(rows) {
  return request({ url: '/api/sysPortal', method: 'put', data: rows });
}

export function deleteSysPortal(id) {
  return request({ url: `/api/sysPortal/${id}`, method: 'delete' });
}

export function fetchSysPortPagePage(params) {
  return request({ url: '/api/sysPortPage/page', method: 'get', params });
}

export function fetchSysPortPage(id) {
  return request({ url: `/api/sysPortPage/${id}`, method: 'get' });
}

export function saveSysPortPage(rows) {
  return request({ url: '/api/sysPortPage', method: 'post', data: rows });
}

export function deleteSysPortPage(id) {
  return request({ url: `/api/sysPortPage/${id}`, method: 'delete' });
}

export function fetchSysPortCellPage(params) {
  return request({ url: '/api/sysPortCell/page', method: 'get', params });
}

export function fetchSysPortCell(id) {
  return request({ url: `/api/sysPortCell/${id}`, method: 'get' });
}

export function saveSysPortCell(rows) {
  return request({ url: '/api/sysPortCell', method: 'post', data: rows });
}

export function deleteSysPortCell(id) {
  return request({ url: `/api/sysPortCell/${id}`, method: 'delete' });
}

export function fetchSysPortCellLinkPage(params) {
  return request({ url: '/api/sysPortCellLink/page', method: 'get', params });
}

export function fetchSysPortCellLink(id) {
  return request({ url: `/api/sysPortCellLink/${id}`, method: 'get' });
}

export function saveSysPortCellLink(rows) {
  return request({ url: '/api/sysPortCellLink', method: 'post', data: rows });
}

export function deleteSysPortCellLink(id) {
  return request({ url: `/api/sysPortCellLink/${id}`, method: 'delete' });
}

export function fetchSysAppletPage(params) {
  return request({ url: '/api/sysApplet/page', method: 'get', params });
}

export function fetchSysApplet(id) {
  return request({ url: `/api/sysApplet/${id}`, method: 'get' });
}

export function saveSysApplet(rows) {
  return request({ url: '/api/sysApplet', method: 'post', data: rows });
}

export function deleteSysApplet(id) {
  return request({ url: `/api/sysApplet/${id}`, method: 'delete' });
}

export function fetchSysAppletCataPage(params) {
  return request({ url: '/api/sysAppletCata/page', method: 'get', params });
}

export function fetchSysAppletCata(id) {
  return request({ url: `/api/sysAppletCata/${id}`, method: 'get' });
}

export function saveSysAppletCata(rows) {
  return request({ url: '/api/sysAppletCata', method: 'post', data: rows });
}

export function deleteSysAppletCata(id) {
  return request({ url: `/api/sysAppletCata/${id}`, method: 'delete' });
}

export function fetchSysPortalRolePage(params) {
  return request({ url: '/api/sysPortalRole/page', method: 'get', params });
}

export function saveSysPortalRole(rows) {
  return request({ url: '/api/sysPortalRole', method: 'post', data: rows });
}

export function createSysPortalRole(rows) {
  return saveSysPortalRole(rows);
}

export function updateSysPortalRole(rows) {
  return saveSysPortalRole(rows);
}

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
  return request({ url: '/api/sysPortCell', method: 'post', data: [params] });
}

export function addCelllLinks(params) {
  return request({ url: '/api/sysPortCellLink', method: 'post', data: params });
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
