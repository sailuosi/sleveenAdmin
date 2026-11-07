import { request } from '../request';

export interface PageParams {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
}

/** sys user */
export function fetchGetUserPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.User>>({
    url: '/api/sysUser/page',
    params
  });
}

export function fetchGetUser(id: string) {
  return request<Api.SystemManage.User>({ url: `/api/sysUser/${id}` });
}

export function fetchCreateUser(data: Api.SystemManage.UserInput) {
  return request<{ id: string }>({ url: '/api/sysUser', method: 'post', data });
}

export function fetchUpdateUser(data: Api.SystemManage.UserInput) {
  return request({ url: '/api/sysUser', method: 'put', data });
}

export function fetchDeleteUser(id: string) {
  return request({ url: `/api/sysUser/${id}`, method: 'delete' });
}

/** sys menu admin */
export function fetchGetMenuTree() {
  return request<Api.SystemManage.Menu[]>({ url: '/api/sysMenu/tree' });
}

export function fetchCreateMenu(data: Api.SystemManage.MenuInput) {
  return request<{ id: string }>({ url: '/api/sysMenu/add', method: 'post', data });
}

export function fetchUpdateMenu(data: Api.SystemManage.MenuInput) {
  return request({ url: '/api/sysMenu/update', method: 'put', data });
}

export function fetchDeleteMenu(id: string) {
  return request({ url: `/api/sysMenu/delete/${id}`, method: 'delete' });
}

/** sys org */
export function fetchGetOrgTree() {
  return request<Api.SystemManage.Org[]>({ url: '/api/sysOrg/tree' });
}

export function fetchCreateOrg(data: Api.SystemManage.OrgInput) {
  return request<{ id: string }>({ url: '/api/sysOrg', method: 'post', data });
}

export function fetchUpdateOrg(data: Api.SystemManage.OrgInput) {
  return request({ url: '/api/sysOrg', method: 'put', data });
}

export function fetchDeleteOrg(id: string) {
  return request({ url: `/api/sysOrg/${id}`, method: 'delete' });
}

/** sys dict */
export function fetchGetDictTypePage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.DictType>>({
    url: '/api/sysDict/type/page',
    params
  });
}

export function fetchCreateDictType(data: Api.SystemManage.DictTypeInput) {
  return request<{ id: string }>({ url: '/api/sysDict/type', method: 'post', data });
}

export function fetchUpdateDictType(data: Api.SystemManage.DictTypeInput) {
  return request({ url: '/api/sysDict/type', method: 'put', data });
}

export function fetchDeleteDictType(id: string) {
  return request({ url: `/api/sysDict/type/${id}`, method: 'delete' });
}

export function fetchGetDictDataPage(typeId: string, params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.DictData>>({
    url: '/api/sysDict/data/page',
    params: { ...params, typeId }
  });
}

export function fetchCreateDictData(data: Api.SystemManage.DictDataInput) {
  return request<{ id: string }>({ url: '/api/sysDict/data', method: 'post', data });
}

export function fetchUpdateDictData(data: Api.SystemManage.DictDataInput) {
  return request({ url: '/api/sysDict/data', method: 'put', data });
}

export function fetchDeleteDictData(id: string) {
  return request({ url: `/api/sysDict/data/${id}`, method: 'delete' });
}

/** sys config */
export function fetchGetConfigPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.Config>>({
    url: '/api/sysConfig/page',
    params
  });
}

export function fetchCreateConfig(data: Api.SystemManage.ConfigInput) {
  return request<{ id: string }>({ url: '/api/sysConfig', method: 'post', data });
}

export function fetchUpdateConfig(data: Api.SystemManage.ConfigInput) {
  return request({ url: '/api/sysConfig', method: 'put', data });
}

export function fetchDeleteConfig(id: string) {
  return request({ url: `/api/sysConfig/${id}`, method: 'delete' });
}
