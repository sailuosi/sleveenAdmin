import {
  checkAuth,
  checkAuthAsync,
  hasGlobalAuth,
  hasPageAuth,
  type AuthBinding
} from '@/utils/authPermission';

export function useAuth() {
  function hasAuth(codes: string | string[]) {
    if (typeof codes === 'string') {
      return hasGlobalAuth(codes);
    }
    return codes.some(code => hasGlobalAuth(code));
  }

  function hasPageAuthCode(code: string, page: Parameters<typeof hasPageAuth>[1]) {
    return hasPageAuth(code, page);
  }

  function checkAuthBinding(binding: AuthBinding) {
    return checkAuth(binding);
  }

  async function checkAuthBindingAsync(binding: AuthBinding) {
    return checkAuthAsync(binding);
  }

  return {
    hasAuth,
    hasPageAuth: hasPageAuthCode,
    checkAuth: checkAuthBinding,
    checkAuthAsync: checkAuthBindingAsync
  };
}
