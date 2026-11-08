import { queryBizSysUserlist } from '@/widgets/sysext/views/sys/user/Page/Biz_Sys_UserListapi.js';

/** R3：宿主 SysUser → 选人候选（UserId=Account）。 */
export type HostCandidateUser = {
  UserId: string;
  UserName: string;
  Account: string;
};

export async function loadHostCandidateUsers(keyword?: string, pageSize = 100): Promise<HostCandidateUser[]> {
  try {
    const res = await queryBizSysUserlist({ page: 1, pageSize, keyword: keyword || null });
    const rows = (res?.Items || res?.records || res?.Records || []) as Array<Record<string, unknown>>;
    if (!Array.isArray(rows)) return [];
    return rows
      .map(u => {
        const account = String(u.account ?? u.Account ?? '').trim();
        if (!account) return null;
        const name = String(u.realName ?? u.RealName ?? account).trim() || account;
        return { UserId: account, UserName: name, Account: account } as HostCandidateUser;
      })
      .filter((x): x is HostCandidateUser => x != null);
  } catch {
    return [];
  }
}
