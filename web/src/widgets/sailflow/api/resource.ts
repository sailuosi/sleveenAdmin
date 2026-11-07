/**
 * 设计器资源：角色 / 用户 / 表单（经本机 Web.Api · requestAPI6）
 */
import request from '@/utils/requestAPI6.js';

export type RoleRow = {
  Id?: string;
  RoleName?: string;
  RoleCode?: string;
};

export type UserRow = {
  UserId?: string;
  UserName?: string;
  EMail?: string;
};

export type FormRow = {
  FormId?: string;
  FormName?: string;
  FormCode?: string;
};

export function getRoleAll() {
  return request({
    url: '/api/Wf2Xml/GetRoleAll',
    method: 'post'
  });
}

export function getUserAll() {
  return request({
    url: '/api/Wf2Xml/GetUserAll',
    method: 'post'
  });
}

export function getFormAll() {
  return request({
    url: '/api/Wf2Xml/GetFormAll',
    method: 'post'
  });
}

export function unwrapList<T>(data: any): T[] {
  if (Array.isArray(data)) return data;
  const entity = data?.Entity ?? data?.entity ?? data;
  return Array.isArray(entity) ? entity : [];
}
