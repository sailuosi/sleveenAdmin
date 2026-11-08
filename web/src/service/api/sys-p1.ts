import { request } from '../request';

/** tenant — 管理 CRUD 走 Biz Page；下拉仍用 list */
export function fetchGetTenantList() {
  return request<Api.SystemP1.Tenant[]>({ url: '/biz/systenantlist/list', method: 'get' });
}

/** notice admin — 已迁 Biz_Sys_NoticeList；本文件仅保留收件箱 */

/** my notice inbox */
export function fetchGetMyNoticePage(params: { page?: number; pageSize?: number; keyword?: string | null }) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemP1.MyNotice>>({
    url: '/api/sysNotice/my/page',
    params
  });
}

export function fetchGetNoticeUnreadCount() {
  return request<{ count: number }>({ url: '/api/sysNotice/my/unread-count' });
}

export function fetchMarkNoticeRead(id: string) {
  return request({ url: `/api/sysNotice/${id}/read`, method: 'post' });
}

/** online user */
export function fetchGetOnlineUserList() {
  return request<Api.SystemP1.OnlineUser[]>({ url: '/api/sysOnlineUser/list' });
}

export function fetchGetOnlineUserCount() {
  return request<{ count: number }>({ url: '/api/sysOnlineUser/count' });
}
