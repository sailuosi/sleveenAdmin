export type SelectOpt = { label: string; value: string };

/**
 * Resolve Type 业务文案（对照 szkfpt BpmTaskCandidateStrategyEnum；value 仍为 Sail string）。
 * 高级/少用策略排后。
 */
export const RESOLVE_TYPE_OPTIONS: SelectOpt[] = [
  { label: '角色（RoleGroup）', value: 'RoleGroup' },
  { label: '部门领导（deptBoss）', value: 'deptBoss' },
  { label: '固定用户（FixedUsers）', value: 'FixedUsers' },
  { label: '发起人自选（StarterPick）', value: 'StarterPick' },
  { label: '审批人自选（ApproverPick）', value: 'ApproverPick' },
  { label: '表单字段 / 表达式（Expression）', value: 'Expression' },
  { label: '流程相对人（ProcessRelative）', value: 'ProcessRelative' },
  { label: '实例预选（Preselect）', value: 'Preselect' },
  { label: '指定传入（Specific）', value: 'Specific' },
  { label: '单位映射（UnitMap）', value: 'UnitMap' },
  { label: '岗位相对（JobRelative）', value: 'JobRelative' },
  { label: '业务回调（BusinessCallback）', value: 'BusinessCallback' },
  { label: '班级学生（Business.ClassStudents）', value: 'Business.ClassStudents' },
  { label: '空人回落（Fallback）', value: 'Fallback' }
];

/** szkfpt 策略码 → Sail Resolve Type（文档/面板对照；引擎不读此表） */
export const SZKFPT_STRATEGY_TO_SAIL: Record<string, string> = {
  ROLE: 'RoleGroup',
  DEPT_LEADER: 'deptBoss',
  DEPT_MEMBER: 'RoleGroup',
  POST: 'JobRelative',
  USER: 'FixedUsers',
  START_USER_SELECT: 'StarterPick',
  APPROVE_USER_SELECT: 'ApproverPick',
  FORM_USER: 'Expression',
  EXPRESSION: 'Expression',
  START_USER: 'ProcessRelative'
};

export const LEAVE_TYPE_OPTIONS: SelectOpt[] = [
  { label: '或签（Race）', value: 'Race' },
  { label: '会签（Collaborate）', value: 'Collaborate' },
  { label: '组内或签（GroupRace）', value: 'GroupRace' },
  { label: '组内会签（GroupCollaborate）', value: 'GroupCollaborate' },
  { label: '队长关卡（Captain）', value: 'Captain' }
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
