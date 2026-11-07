import { getServiceBaseURL } from '@/utils/service';
import { localStg } from '@/utils/storage';
import { request } from '../request';

export type LogPageParams = {
  page?: number;
  pageSize?: number;
  keyword?: string | null;
  startTime?: string | null;
  endTime?: string | null;
};

/** sys file */
export function fetchGetFilePage(params: Api.SysInfra.FilePageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SysInfra.SysFile>>({
    url: '/api/sysFile/page',
    params
  });
}

export function fetchUploadFile(file: File) {
  const formData = new FormData();
  formData.append('file', file);
  return request<Api.SysInfra.SysFile>({
    url: '/api/sysFile/upload',
    method: 'post',
    data: formData,
    headers: { 'Content-Type': 'multipart/form-data' }
  });
}

export function fetchDeleteFile(id: string) {
  return request({ url: `/api/sysFile/${id}`, method: 'delete' });
}

export async function downloadFile(id: string, fileName: string) {
  const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
  const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);
  const token = localStg.get('token');
  const response = await fetch(`${baseURL}/api/sysFile/download/${id}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {}
  });
  if (!response.ok) throw new Error('下载失败');
  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

/** sys log */
export function fetchGetOpLogPage(params: LogPageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SysInfra.SysLogOp>>({
    url: '/api/sysLog/op/page',
    params
  });
}

export function fetchGetVisLogPage(params: LogPageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SysInfra.SysLogVis>>({
    url: '/api/sysLog/vis/page',
    params
  });
}

/** monitor */
export function fetchGetServerInfo() {
  return request<Api.SysInfra.ServerInfo>({ url: '/api/sysServer/info' });
}

export function fetchGetRedisInfo() {
  return request<Api.SysInfra.RedisInfo>({ url: '/api/sysRedis/info' });
}

export function fetchGetRedisKeyPage(params: Api.SysInfra.RedisKeyPageParams) {
  return request<Api.Common.PaginatingQueryRecord<Api.SysInfra.RedisKey>>({
    url: '/api/sysRedis/keys/page',
    params
  });
}

export function fetchGetRedisKeyValue(key: string) {
  return request<Api.SysInfra.RedisKeyValue>({
    url: `/api/sysRedis/keys/${encodeURIComponent(key)}`
  });
}

export function fetchDeleteRedisKey(key: string) {
  return request({
    url: `/api/sysRedis/keys/${encodeURIComponent(key)}`,
    method: 'delete'
  });
}
