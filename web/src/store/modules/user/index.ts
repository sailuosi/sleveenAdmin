import { computed } from 'vue';
import { defineStore } from 'pinia';
import { SetupStoreId } from '@/enum';
import { useAuthStore } from '../auth';

/** ccflow 兼容的用户信息格式 */
export interface CcflowUserInfo {
  userID: string;
  orgName: string;
  personName: string;
  divisionNo: string;
  divisionName: string;
}

export const useUserStore = defineStore(SetupStoreId.User, () => {
  const authStore = useAuthStore();

  const getUserInfo = computed<CcflowUserInfo>(() => {
    const info = authStore.userInfo as Record<string, unknown>;
    return {
      userID: (info.userId as string) || '',
      orgName: (info.OrgName as string) || '',
      personName: (info.userName as string) || '',
      divisionNo: (info.divisionNo as string) || (info.OrgNo as string) || '',
      divisionName: (info.divisionName as string) || (info.OrgName as string) || ''
    };
  });

  return { getUserInfo };
});
