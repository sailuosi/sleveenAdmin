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
  return Promise.reject(error);
}

service.interceptors.response.use(
  response => {
    const data = response.data;
    if (data && isLogoutCode(data.code)) {
      void forceLogout();
      return Promise.reject(new Error(data?.msg || data?.message || data?.info || '登录已失效'));
    }
    if (data && (data.code === '0000' || data.code === 0)) {
      return data.data ?? data;
    }
    // minehub AjaxResult：{ status:'success', total, data }
    if (data && (data.status === 'success' || data.status === 200)) {
      return data;
    }
    return Promise.reject(new Error(data?.msg || data?.message || data?.info || '请求失败'));
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
