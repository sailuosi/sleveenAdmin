export type SelectOpt = { label: string; value: string };

export const RESOLVE_TYPE_OPTIONS: SelectOpt[] = [
  { label: 'Specific', value: 'Specific' },
  { label: 'FixedUsers', value: 'FixedUsers' },
  { label: 'RoleGroup', value: 'RoleGroup' },
  { label: 'ProcessRelative', value: 'ProcessRelative' },
  { label: 'StarterPick', value: 'StarterPick' },
  { label: 'ApproverPick', value: 'ApproverPick' },
  { label: 'UnitMap', value: 'UnitMap' },
  { label: 'JobRelative', value: 'JobRelative' },
  { label: 'BusinessCallback', value: 'BusinessCallback' },
  { label: 'Expression', value: 'Expression' },
  { label: 'Fallback', value: 'Fallback' },
  { label: 'Business.ClassStudents', value: 'Business.ClassStudents' },
  { label: 'deptBoss（部门领导）', value: 'deptBoss' }
];

export const LEAVE_TYPE_OPTIONS: SelectOpt[] = [
  { label: 'Race', value: 'Race' },
  { label: 'Collaborate', value: 'Collaborate' },
  { label: 'GroupRace', value: 'GroupRace' },
  { label: 'GroupCollaborate', value: 'GroupCollaborate' },
  { label: 'Captain', value: 'Captain' }
];

/** 运行时选人器（与 AssigneeResolve 正交） */
export const CANDIDATE_SELECTOR_OPTIONS: SelectOpt[] = [
  { label: 'HostUserSearch（宿主远程搜索）', value: 'HostUserSearch' },
  { label: 'InlineList（预载名单）', value: 'InlineList' },
  { label: 'RoleUserSearch（角色内搜索）', value: 'RoleUserSearch' }
];

export function parseParamsJson(raw: string | undefined | null): Record<string, unknown> {
  if (!raw?.trim()) return {};
  try {
    const v = JSON.parse(raw);
    return v && typeof v === 'object' && !Array.isArray(v) ? (v as Record<string, unknown>) : {};
  } catch {
    return {};
  }
}

export function stringifyParams(obj: Record<string, unknown>): string {
  const keys = Object.keys(obj).filter(k => {
    const v = obj[k];
    if (v == null || v === '') return false;
    if (Array.isArray(v) && v.length === 0) return false;
    return true;
  });
  if (!keys.length) return '';
  const out: Record<string, unknown> = {};
  for (const k of keys) out[k] = obj[k];
  return JSON.stringify(out);
}
