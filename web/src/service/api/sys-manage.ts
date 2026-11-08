import { request } from '../request';

export interface PageParams {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
}

/** 用户分页 — 供权限包选人等；管理 CRUD 已迁 Biz 页面 */
export function fetchGetUserPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemManage.User>>({
    url: '/biz/sysuserlist/qlist',
    method: 'post',
    data: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 10,
      keyword: params.keyword ?? null
    }
  });
}

/** 菜单 / 组织树 — 供权限包等场景；管理 CRUD 已迁 Biz 页面 */
export function fetchGetMenuTree() {
  return request<Api.SystemManage.Menu[]>({ url: '/biz/sysmenulist/tree', method: 'post' });
}

export function fetchGetOrgTree() {
  return request<Api.SystemManage.Org[]>({ url: '/biz/sysorglist/tree', method: 'post' });
}

/** sys config — 管理 CRUD 已迁 Biz_Sys_ConfigList */
