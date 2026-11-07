import { useAuthStore } from '@/store/modules/auth';

export function useUserInfo() {
  const authStore = useAuthStore();
  return {
    userInfos: {
      account: authStore.userInfo.userName,
      userName: authStore.userInfo.userName,
      roles: authStore.userInfo.roles
    }
  };
}
