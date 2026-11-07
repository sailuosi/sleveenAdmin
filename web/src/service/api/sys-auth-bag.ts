import { request } from '../request';

export interface AuthBagPageParams {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
  bagId?: string | null;
  type?: string | null;
}

export interface AuthBagSelectItem {
  id: string;
  name?: string;
  code?: string;
}

export interface AuthDutyItem {
  id?: string;
  name?: string;
  code?: string;
  note?: string;
  remark?: string;
  idx?: number;
  visitorBagId?: string | null;
  wordBagId?: string | null;
  goodsBagId?: string | null;
  visitorBagName?: string | null;
  wordBagName?: string | null;
  goodsBagName?: string | null;
}

export interface AuthGoodsBag {
  id?: string;
  name?: string;
  code?: string;
  note?: string;
  remark?: string;
  type?: string;
  idx?: number;
  menuBindCount?: number;
}

export interface AuthVisitorBag {
  id?: string;
  name?: string;
  code?: string;
  note?: string;
  remark?: string;
  type?: string;
  keyWord?: string;
  idx?: number;
  parentId?: string | null;
}

export interface AuthVisitorItem {
  id?: string;
  visitorBagId?: string;
  type?: string;
  name?: string;
  code?: string;
  oid?: string;
  isOn?: boolean;
  idx?: number;
}

export interface AuthWordBag {
  id?: string;
  name?: string;
  code?: string;
  note?: string;
  remark?: string;
  type?: string;
  idx?: number;
}

export interface AuthWordItem {
  id?: string;
  wordBagId?: string;
  wordDefineId?: string | null;
  name?: string;
  code?: string;
  type?: string;
  idx?: number;
}

export interface AuthWordDefine {
  id?: string;
  name?: string;
  code?: string;
  type?: string;
  para?: string;
  parser?: string;
  note?: string;
  idx?: number;
  isOn?: boolean;
}

export function fetchAuthDutyPage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthDutyItem>>({
    url: '/api/authBag/dutyItem/page',
    params
  });
}

export function fetchSaveAuthDuty(data: AuthDutyItem) {
  return request<{ id: string }>({ url: '/api/authBag/dutyItem', method: 'post', data });
}

export function fetchDeleteAuthDuty(id: string) {
  return request({ url: `/api/authBag/dutyItem/${id}`, method: 'delete' });
}

export function fetchCleanAuthCache(mode: string) {
  return request<{ count: number }>({ url: '/api/authBag/dutyItem/cleanCache', method: 'post', data: { mode } });
}

export function fetchAuthGoodsBagPage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthGoodsBag>>({
    url: '/api/authBag/goodsBag/page',
    params
  });
}

export function fetchSaveAuthGoodsBag(data: AuthGoodsBag) {
  return request<{ id: string }>({ url: '/api/authBag/goodsBag', method: 'post', data });
}

export function fetchDeleteAuthGoodsBag(id: string) {
  return request({ url: `/api/authBag/goodsBag/${id}`, method: 'delete' });
}

export function fetchAuthGoodsBagSelect() {
  return request<AuthBagSelectItem[]>({ url: '/api/authBag/goodsBag/select' });
}

export function fetchLoadMenuBindKeys(goodsBagId: string, type = '1') {
  return request<Array<{ id: string; oid: string; name?: string }>>({
    url: '/api/authBag/goodsItem/bindKeys',
    params: { goodsBagId, type }
  });
}

export function fetchSaveMenuBindKeys(data: { goodsBagId: string; type: string; checkMenus: string[] }) {
  return request({ url: '/api/authBag/goodsItem/bindKeys', method: 'post', data });
}

export function fetchAuthVisitorBagPage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthVisitorBag>>({
    url: '/api/authBag/visitorBag/page',
    params
  });
}

export function fetchSaveAuthVisitorBag(data: AuthVisitorBag) {
  return request<{ id: string }>({ url: '/api/authBag/visitorBag', method: 'post', data });
}

export function fetchDeleteAuthVisitorBag(id: string) {
  return request({ url: `/api/authBag/visitorBag/${id}`, method: 'delete' });
}

export function fetchAuthVisitorBagSelect() {
  return request<AuthBagSelectItem[]>({ url: '/api/authBag/visitorBag/select' });
}

export function fetchAuthVisitorItemPage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthVisitorItem>>({
    url: '/api/authBag/visitorItem/page',
    params
  });
}

export function fetchSaveAuthVisitorItem(data: AuthVisitorItem) {
  return request<{ id: string }>({ url: '/api/authBag/visitorItem', method: 'post', data });
}

export function fetchBatchAuthVisitorItems(data: { visitorBagId: string; items: AuthVisitorItem[] }) {
  return request<{ count: number }>({ url: '/api/authBag/visitorItem/batch', method: 'post', data });
}

export function fetchDeleteAuthVisitorItem(id: string) {
  return request({ url: `/api/authBag/visitorItem/${id}`, method: 'delete' });
}

export function fetchAuthWordBagPage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthWordBag>>({
    url: '/api/authBag/wordBag/page',
    params
  });
}

export function fetchSaveAuthWordBag(data: AuthWordBag) {
  return request<{ id: string }>({ url: '/api/authBag/wordBag', method: 'post', data });
}

export function fetchDeleteAuthWordBag(id: string) {
  return request({ url: `/api/authBag/wordBag/${id}`, method: 'delete' });
}

export function fetchAuthWordBagSelect() {
  return request<AuthBagSelectItem[]>({ url: '/api/authBag/wordBag/select' });
}

export function fetchAuthWordItemPage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthWordItem>>({
    url: '/api/authBag/wordItem/page',
    params
  });
}

export function fetchSaveAuthWordItem(data: AuthWordItem) {
  return request<{ id: string }>({ url: '/api/authBag/wordItem', method: 'post', data });
}

export function fetchDeleteAuthWordItem(id: string) {
  return request({ url: `/api/authBag/wordItem/${id}`, method: 'delete' });
}

export function fetchAuthWordDefinePage(params: AuthBagPageParams) {
  return request<Api.Common.PaginatingQueryRecord<AuthWordDefine>>({
    url: '/api/authBag/wordDefine/page',
    params
  });
}

export function fetchSaveAuthWordDefine(data: AuthWordDefine) {
  return request<{ id: string }>({ url: '/api/authBag/wordDefine', method: 'post', data });
}

export function fetchDeleteAuthWordDefine(id: string) {
  return request({ url: `/api/authBag/wordDefine/${id}`, method: 'delete' });
}

export function fetchWordDefineSelect() {
  return request<Array<{ id: string; name?: string; code?: string; type?: string }>>({
    url: '/api/authBag/wordDefine/select'
  });
}
