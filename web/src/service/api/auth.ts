import { request } from '../request';

/** Captcha image */
export function fetchCaptcha() {
  return request<Api.Auth.CaptchaInfo>({ url: '/api/auth/captcha' });
}

/** Login */
export function fetchLogin(
  userName: string,
  password: string,
  captchaId?: string,
  captchaCode?: string
) {
  return request<Api.Auth.LoginToken>({
    url: '/api/auth/login',
    method: 'post',
    data: {
      userName,
      password,
      captchaId,
      captchaCode
    }
  });
}

/** Get user info */
export function fetchGetUserInfo() {
  return request<Api.Auth.UserInfo>({ url: '/api/auth/user-info' });
}

/** Current frontend release (AllowAnonymous) */
export async function fetchAppRelease(): Promise<Api.Auth.AppRelease | null> {
  const { data, error } = await request<Api.Auth.AppRelease>({
    url: '/api/auth/appRelease',
    headers: { 'X-Skip-Token': 'true' }
  });
  if (error) return null;
  return data ?? null;
}

/** Refresh token (no access token required) */
export function fetchRefreshToken(refreshToken: string) {
  return request<Api.Auth.LoginToken>({
    url: '/api/auth/refreshToken',
    method: 'post',
    headers: { 'X-Skip-Token': 'true' },
    data: {
      refreshToken
    }
  });
}

/** return custom backend error */
export function fetchCustomBackendError(code: string, msg: string) {
  return request({ url: '/api/auth/error', params: { code, msg } });
}
