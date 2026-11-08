import { request } from '../request';

export interface PageParams {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
}

function pageBody(params: PageParams) {
  return {
    page: params.page ?? 1,
    pageSize: params.pageSize ?? 10,
    keyword: params.keyword ?? null
  };
}

/** 用户 — manage 页与权限包选人共用 Biz */
export function fetchGetUserPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.User>>({
    url: '/biz/sysuserlist/qlist',
    method: 'post',
    data: pageBody(params)
  });
}

export function fetchCreateUser(data: Api.SystemManage.UserInput) {
  return request({ url: '/biz/sysuserlist/save', method: 'post', data: [data] });
}

export function fetchUpdateUser(data: Api.SystemManage.UserInput) {
  return request({ url: '/biz/sysuserlist/save', method: 'post', data: [data] });
}

export function fetchDeleteUser(id: string) {
  return request({ url: '/biz/sysuserlist/rmbyids', method: 'post', data: [id] });
}

/** 菜单 */
export function fetchGetMenuTree() {
  return request<Api.SystemManage.Menu[]>({ url: '/biz/sysmenulist/tree', method: 'post' });
}

export function fetchCreateMenu(data: Api.SystemManage.MenuInput) {
  return request({ url: '/biz/sysmenulist/save', method: 'post', data: [data] });
}

export function fetchUpdateMenu(data: Api.SystemManage.MenuInput) {
  return request({ url: '/biz/sysmenulist/save', method: 'post', data: [data] });
}

export function fetchDeleteMenu(id: string) {
  return request({ url: '/biz/sysmenulist/rmbyids', method: 'post', data: [id] });
}

/** 组织 */
export function fetchGetOrgTree() {
  return request<Api.SystemManage.Org[]>({ url: '/biz/sysorglist/tree', method: 'post' });
}

export function fetchCreateOrg(data: Api.SystemManage.OrgInput) {
  return request({ url: '/biz/sysorglist/save', method: 'post', data: [data] });
}

export function fetchUpdateOrg(data: Api.SystemManage.OrgInput) {
  return request({ url: '/biz/sysorglist/save', method: 'post', data: [data] });
}

export function fetchDeleteOrg(id: string) {
  return request({ url: '/biz/sysorglist/rmbyids', method: 'post', data: [id] });
}

/** 字典类型 */
export function fetchGetDictTypePage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.DictType>>({
    url: '/biz/sysdicttypelist/qlist',
    method: 'post',
    data: pageBody(params)
  });
}

export function fetchCreateDictType(data: Api.SystemManage.DictTypeInput) {
  return request({ url: '/biz/sysdicttypelist/save', method: 'post', data: [data] });
}

export function fetchUpdateDictType(data: Api.SystemManage.DictTypeInput) {
  return request({ url: '/biz/sysdicttypelist/save', method: 'post', data: [data] });
}

export function fetchDeleteDictType(id: string) {
  return request({ url: '/biz/sysdicttypelist/rmbyids', method: 'post', data: [id] });
}

/** 字典数据 */
export function fetchGetDictDataPage(typeId: string, params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.DictData>>({
    url: '/biz/sysdictdatalist/qlist',
    method: 'post',
    data: {
      ...pageBody(params),
      filters: { typeId }
    }
  });
}

export function fetchCreateDictData(data: Api.SystemManage.DictDataInput) {
  return request({ url: '/biz/sysdictdatalist/save', method: 'post', data: [data] });
}

export function fetchUpdateDictData(data: Api.SystemManage.DictDataInput) {
  return request({ url: '/biz/sysdictdatalist/save', method: 'post', data: [data] });
}

export function fetchDeleteDictData(id: string) {
  return request({ url: '/biz/sysdictdatalist/rmbyids', method: 'post', data: [id] });
}

/** 系统配置 */
export function fetchGetConfigPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.Config>>({
    url: '/biz/sysconfiglist/qlist',
    method: 'post',
    data: pageBody(params)
  });
}

export function fetchCreateConfig(data: Api.SystemManage.ConfigInput) {
  return request({ url: '/biz/sysconfiglist/save', method: 'post', data: [data] });
}

export function fetchUpdateConfig(data: Api.SystemManage.ConfigInput) {
  return request({ url: '/biz/sysconfiglist/save', method: 'post', data: [data] });
}

export function fetchDeleteConfig(id: string) {
  return request({ url: '/biz/sysconfiglist/rmbyids', method: 'post', data: [id] });
}
