import axios from 'axios';
import { localStg } from '@/utils/storage';
import { getServiceBaseURL } from '@/utils/service';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

const service = axios.create({
  baseURL,
  timeout: 30000
});

service.interceptors.request.use(config => {
  const token = localStg.get('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

service.interceptors.response.use(
  response => {
    const data = response.data;
    if (data && (data.code === '0000' || data.code === 0)) {
      return data.data ?? data;
    }
    // HTTP 风格 / 部分门户
    if (data && data.status === 200) {
      return data;
    }
    // Sailflow ResponseResult：Status=1 成功，-1 失败（JSON 多为 camelCase）
    const sailStatus = data?.status ?? data?.Status;
    if (data && (sailStatus === 1 || sailStatus === '1')) {
      return data;
    }
    if (data && (sailStatus === -1 || sailStatus === '-1')) {
      return Promise.reject(new Error(data?.message || data?.Message || '请求失败'));
    }
    return Promise.reject(new Error(data?.msg || data?.message || '请求失败'));
  },
  error => Promise.reject(error)
);

export default service;
