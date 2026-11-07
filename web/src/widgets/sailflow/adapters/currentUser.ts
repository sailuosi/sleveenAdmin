import { computed } from 'vue';
import { useAuthStore } from '@/store/modules/auth';

/**
 * 当前登录用户 → FlowCommandContext UserId/UserName。
 * R3：UserId 用登录账号 Account（非 GUID 主键）。
 */
export function useSailCurrentUser() {
  const auth = useAuthStore();
  const userId = computed(
    () =>
      String(auth.userInfo?.account || '').trim() ||
      String(auth.userInfo?.userId || '').trim() ||
      'demo'
  );
  const userName = computed(() => String(auth.userInfo?.userName || '').trim() || userId.value);
  return { userId, userName, auth };
}
