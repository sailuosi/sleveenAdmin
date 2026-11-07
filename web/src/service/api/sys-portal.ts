import { request } from '../request';

export interface PortalPageParams {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
  spOrgId?: string | null;
  spPortPageId?: string | null;
  spPortalId?: string | null;
  saAppletCataId?: string | null;
  pName?: string | null;
  pKey?: string | null;
  pTask?: string | null;
  pIsOn?: boolean | null;
}

export function fetchSysPortalPage(params: PortalPageParams) {
  return request<Api.Common.PaginatingQueryRecord<Record<string, unknown>>>({
    url: '/api/sysPortal/page',
    params
  });
}

export function fetchSysPortal(id: string) {
  return request<Record<string, unknown>>({ url: `/api/sysPortal/${id}` });
}

export function createSysPortal(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortal', method: 'post', data });
}

export function updateSysPortal(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortal', method: 'put', data });
}

export function deleteSysPortal(id: string) {
  return request({ url: `/api/sysPortal/${id}`, method: 'delete' });
}

export function fetchSysPortPagePage(params: PortalPageParams) {
  return request({ url: '/api/sysPortPage/page', params });
}

export function fetchSysPortPage(id: string) {
  return request({ url: `/api/sysPortPage/${id}` });
}

export function saveSysPortPage(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortPage', method: 'post', data });
}

export function deleteSysPortPage(id: string) {
  return request({ url: `/api/sysPortPage/${id}`, method: 'delete' });
}

export function fetchSysPortCellPage(params: PortalPageParams) {
  return request({ url: '/api/sysPortCell/page', params });
}

export function fetchSysPortCell(id: string) {
  return request({ url: `/api/sysPortCell/${id}` });
}

export function saveSysPortCell(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortCell', method: 'post', data });
}

export function deleteSysPortCell(id: string) {
  return request({ url: `/api/sysPortCell/${id}`, method: 'delete' });
}

export function fetchSysPortCellLinkPage(params: PortalPageParams) {
  return request({ url: '/api/sysPortCellLink/page', params });
}

export function fetchSysPortCellLink(id: string) {
  return request({ url: `/api/sysPortCellLink/${id}` });
}

export function saveSysPortCellLink(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortCellLink', method: 'post', data });
}

export function deleteSysPortCellLink(id: string) {
  return request({ url: `/api/sysPortCellLink/${id}`, method: 'delete' });
}

export function fetchSysAppletPage(params: PortalPageParams) {
  return request({ url: '/api/sysApplet/page', params });
}

export function fetchSysApplet(id: string) {
  return request({ url: `/api/sysApplet/${id}` });
}

export function saveSysApplet(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysApplet', method: 'post', data });
}

export function deleteSysApplet(id: string) {
  return request({ url: `/api/sysApplet/${id}`, method: 'delete' });
}

export function fetchSysAppletCataPage(params: PortalPageParams) {
  return request({ url: '/api/sysAppletCata/page', params });
}

export function fetchSysAppletCata(id: string) {
  return request({ url: `/api/sysAppletCata/${id}` });
}

export function saveSysAppletCata(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysAppletCata', method: 'post', data });
}

export function deleteSysAppletCata(id: string) {
  return request({ url: `/api/sysAppletCata/${id}`, method: 'delete' });
}

export function fetchSysPortalRolePage(params: PortalPageParams) {
  return request({ url: '/api/sysPortalRole/page', params });
}

export function saveSysPortalRole(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortalRole', method: 'post', data });
}

export function fetchSysPortalLinkPage(params: PortalPageParams) {
  return request({ url: '/api/sysPortalLink/page', params });
}

export function saveSysPortalLink(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPortalLink', method: 'post', data });
}

export function fetchSysPageSelectPage(params: PortalPageParams) {
  return request({ url: '/api/sysPageSelect/page', params });
}

export function saveSysPageSelect(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPageSelect', method: 'post', data });
}

export function fetchSysPageFieldPage(params: PortalPageParams) {
  return request({ url: '/api/sysPageField/page', params });
}

export function saveSysPageField(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPageField', method: 'post', data });
}

export function fetchSysPageConnectPage(params: PortalPageParams) {
  return request({ url: '/api/sysPageConnect/page', params });
}

export function saveSysPageConnect(data: Record<string, unknown>[]) {
  return request({ url: '/api/sysPageConnect', method: 'post', data });
}
