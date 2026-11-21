import axios from 'axios';
import { localStg } from '@/utils/storage';
import { getServiceBaseURL } from '@/utils/service';
import { getAuthorization, handleExpiredRequest } from '@/service/request/shared';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

const service = axios.create({
  baseURL,
  timeout: 30000
});

/** 与 @/service/request 共用刷新互斥；仅本实例使用 */
const expiredState = {
  refreshTokenPromise: null,
  errMsgStack: []
};

service.interceptors.request.use(config => {
  const token = localStg.get('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

function isLogoutCode(code) {
  const logoutCodes = import.meta.env.VITE_SERVICE_LOGOUT_CODES?.split(',') || [];
  const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
  const c = String(code ?? '');
  return logoutCodes.includes(c) || modalLogoutCodes.includes(c);
}

async function forceLogout() {
  const { useAuthStore } = await import('@/store/modules/auth');
  await useAuthStore().resetStore();
}

/**
 * HTTP 401：先刷新 token 并重试一次；失败则清登录态并跳转登录。
 */
async function handleHttp401(error) {
  const config = error?.config;
  if (!config || config.__retry401) {
    await forceLogout();
    return Promise.reject(error);
  }
  const ok = await handleExpiredRequest(expiredState);
  if (ok) {
    config.__retry401 = true;
    config.headers = config.headers || {};
    const Authorization = getAuthorization();
    if (Authorization) Object.assign(config.headers, { Authorization });
    return service.request(config);
  }
  // handleExpiredRequest 失败时已 resetStore
  return Promise.reject(error);
}

service.interceptors.response.use(
  response => {
    const data = response.data;
    // 业务码登出（HTTP 仍 200）
    if (data && isLogoutCode(data.code)) {
      void forceLogout();
      return Promise.reject(new Error(data?.msg || data?.message || '登录已失效'));
    }
    if (data && (data.code === '0000' || data.code === 0)) {
      return data.data ?? data;
    }
    // minehub AjaxResult / HTTP 风格门户
    if (data && (data.status === 'success' || data.status === 200)) {
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
  error => {
    const status = error?.response?.status;
    if (status === 401) {
      return handleHttp401(error);
    }
    return Promise.reject(error);
  }
);

export default service;
