import { request } from '../request';

export function fetchPortalConfig(key: string) {
  return request<Record<string, unknown>>({
    url: '/api/sysPortal/common/loadportalconfig',
    method: 'post',
    data: { value: key }
  });
}

export function fetchPortalConfigByOid(oid: string) {
  return request<Record<string, unknown>>({
    url: '/api/sysPortal/common/loadportalconfigbyoid',
    method: 'post',
    data: { value: oid }
  });
}

export function fetchHomePortal(pageCode = 'home') {
  return request<Record<string, unknown>>({
    url: '/api/sysPortal/common/loadhomeportal',
    method: 'post',
    data: { value: pageCode }
  });
}

export function savePortalCells(portalId: string, cells: unknown[]) {
  return request({
    url: '/api/sysPortal/common/savecell',
    method: 'post',
    data: { portalId, cells }
  });
}

export function savePortalApplets(portalId: string, applets: unknown[]) {
  return request({
    url: '/api/sysPortal/common/saveapplets',
    method: 'post',
    data: { portalId, applets }
  });
}

/** 兼容门户展示组件的函数名 */
export const getPortConfig = fetchPortalConfig;
export const getPortConfigByOid = fetchPortalConfigByOid;
export const getHomePortal = fetchHomePortal;

export function savePortCells(portalId: string, cells: unknown[]) {
  return savePortalCells(portalId, cells);
}

export function saveApplets(data: { portalId?: string; applets?: unknown[]; moid?: string }) {
  const portalId = data.portalId || data.moid || '';
  return savePortalApplets(portalId, data.applets || []);
}
