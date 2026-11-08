import { request } from '../request';
import type { PageParams } from './sys-manage';

function pageBody(params: PageParams) {
  return {
    page: params.page ?? 1,
    pageSize: params.pageSize ?? 10,
    keyword: params.keyword ?? null
  };
}

/** tenant */
export function fetchGetTenantPage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemP1.Tenant>>({
    url: '/biz/systenantlist/qlist',
    method: 'post',
    data: pageBody(params)
  });
}

export function fetchGetTenantList() {
  return request<Api.SystemP1.Tenant[]>({ url: '/biz/systenantlist/list', method: 'get' });
}

export function fetchCreateTenant(data: Api.SystemP1.TenantInput) {
  return request({ url: '/biz/systenantlist/save', method: 'post', data: [data] });
}

export function fetchUpdateTenant(data: Api.SystemP1.TenantInput) {
  return request({ url: '/biz/systenantlist/save', method: 'post', data: [data] });
}

export function fetchDeleteTenant(id: string) {
  return request({ url: '/biz/systenantlist/rmbyids', method: 'post', data: [id] });
}

/** notice admin */
export function fetchGetNoticePage(params: PageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SystemP1.Notice>>({
    url: '/biz/sysnoticelist/qlist',
    method: 'post',
    data: pageBody(params)
  });
}

export function fetchCreateNotice(data: Api.SystemP1.NoticeInput) {
  return request({ url: '/biz/sysnoticelist/save', method: 'post', data: [data] });
}

export function fetchPublishNotice(id: string) {
  return request({ url: `/biz/sysnoticelist/publish/${id}`, method: 'post' });
}

export function fetchDeleteNotice(id: string) {
  return request({ url: '/biz/sysnoticelist/rmbyids', method: 'post', data: [id] });
}

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
