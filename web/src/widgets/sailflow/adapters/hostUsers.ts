import { fetchGetUserPage } from '@/service/api/sys-manage';

/** R3：宿主 SysUser → 选人候选（UserId=Account）。 */
export type HostCandidateUser = {
  UserId: string;
  UserName: string;
  Account: string;
};

export async function loadHostCandidateUsers(keyword?: string, pageSize = 100): Promise<HostCandidateUser[]> {
  try {
    const res = await fetchGetUserPage({ page: 1, pageSize, keyword: keyword || null });
    const page = (res as { data?: { records?: unknown[] } })?.data;
    const rows = (page?.records || []) as Array<Record<string, unknown>>;
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
