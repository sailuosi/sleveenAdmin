import { request } from '../request';
import type { PageParams } from './sys-manage';

/** tenant */
export function fetchGetTenantPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemP1.Tenant>>({
    url: '/api/sysTenant/page',
    params
  });
}

export function fetchGetTenantList() {
  return request<Api.SystemP1.Tenant[]>({ url: '/api/sysTenant/list' });
}

export function fetchCreateTenant(data: Api.SystemP1.TenantInput) {
  return request<{ id: string }>({ url: '/api/sysTenant', method: 'post', data });
}

export function fetchUpdateTenant(data: Api.SystemP1.TenantInput) {
  return request({ url: '/api/sysTenant', method: 'put', data });
}

export function fetchDeleteTenant(id: string) {
  return request({ url: `/api/sysTenant/${id}`, method: 'delete' });
}

/** notice admin */
export function fetchGetNoticePage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemP1.Notice>>({
    url: '/api/sysNotice/page',
    params
  });
}

export function fetchCreateNotice(data: Api.SystemP1.NoticeInput) {
  return request<{ id: string }>({ url: '/api/sysNotice', method: 'post', data });
}

export function fetchPublishNotice(id: string) {
  return request({ url: `/api/sysNotice/${id}/publish`, method: 'post' });
}

export function fetchDeleteNotice(id: string) {
  return request({ url: `/api/sysNotice/${id}`, method: 'delete' });
}

/** my notice inbox */
export function fetchGetMyNoticePage(params: PageParams) {
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
