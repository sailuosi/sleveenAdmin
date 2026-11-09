/** 与后端 ProcessValidateResultTypeEnum / ProcessValidateMessage 对齐 */

export const VALIDATE_MSG: Record<number, string> = {
  0: '未得到校验结果',
  1: '校验通过：存在开始/结束事件，且有通路；用户任务 Resolve/Leave 已配置',
  2: '校验未通过：缺少开始事件',
  3: '校验未通过：缺少结束事件',
  4: '校验未通过：开始到结束无连通路径',
  5: '校验未通过：用户任务缺少 AssigneeResolve 或 LeaveJudge Type（发布前须配置）'
};

export type ProcessValidateMessage = {
  level: string;
  code: string;
  text: string;
  activityId?: string;
  activityName?: string;
  activityType?: string;
  assigneeResolveType?: string;
  leaveJudgeType?: string;
  nextActivityIds: string[];
  nextActivityNames: string[];
  nextAssigneeSummary?: string;
};

export type ParsedValidateResult = {
  code: number;
  passed: boolean;
  summary: string;
  issues: string[];
  detail: string;
  messages: ProcessValidateMessage[];
};

function pick(obj: Record<string, unknown>, ...keys: string[]): unknown {
  for (const k of keys) {
    if (obj[k] != null) return obj[k];
  }
  return undefined;
}

function asStringList(v: unknown): string[] {
  if (!Array.isArray(v)) return [];
  return v.map(x => String(x ?? '')).filter(Boolean);
}

function parseMessage(raw: unknown): ProcessValidateMessage | null {
  if (!raw || typeof raw !== 'object') return null;
  const m = raw as Record<string, unknown>;
  return {
    level: String(pick(m, 'Level', 'level') ?? 'Info'),
    code: String(pick(m, 'Code', 'code') ?? ''),
    text: String(pick(m, 'Text', 'text') ?? ''),
    activityId: pick(m, 'ActivityId', 'activityId') != null
      ? String(pick(m, 'ActivityId', 'activityId'))
      : undefined,
    activityName: pick(m, 'ActivityName', 'activityName') != null
      ? String(pick(m, 'ActivityName', 'activityName'))
      : undefined,
    activityType: pick(m, 'ActivityType', 'activityType') != null
      ? String(pick(m, 'ActivityType', 'activityType'))
      : undefined,
    assigneeResolveType: pick(m, 'AssigneeResolveType', 'assigneeResolveType') != null
      ? String(pick(m, 'AssigneeResolveType', 'assigneeResolveType'))
      : undefined,
    leaveJudgeType: pick(m, 'LeaveJudgeType', 'leaveJudgeType') != null
      ? String(pick(m, 'LeaveJudgeType', 'leaveJudgeType'))
      : undefined,
    nextActivityIds: asStringList(pick(m, 'NextActivityIds', 'nextActivityIds')),
    nextActivityNames: asStringList(pick(m, 'NextActivityNames', 'nextActivityNames')),
    nextAssigneeSummary: pick(m, 'NextAssigneeSummary', 'nextAssigneeSummary') != null
      ? String(pick(m, 'NextAssigneeSummary', 'nextAssigneeSummary'))
      : undefined
  };
}

/**
 * 解析 ValidateProcess 响应（兼容 Status/Entity Pascal/camel）。
 */
export function parseValidateProcessResponse(data: unknown): ParsedValidateResult {
  const root = (data && typeof data === 'object' ? data : {}) as Record<string, unknown>;
  const failStatus = pick(root, 'Status', 'status');
  if (failStatus === -1 || failStatus === '-1') {
    throw new Error(String(pick(root, 'Message', 'message') || '校验失败'));
  }

  const entityRaw = pick(root, 'Entity', 'entity') ?? root;
  const entity = (entityRaw && typeof entityRaw === 'object'
    ? entityRaw
    : {}) as Record<string, unknown>;

  const code = Number(pick(entity, 'ProcessValidatedResultType', 'processValidatedResultType') ?? 0);
  const issues = asStringList(pick(entity, 'Issues', 'issues'));
  const detail = String(pick(entity, 'Message', 'message') ?? '').trim();
  const rawMessages = pick(entity, 'Messages', 'messages');
  const messages: ProcessValidateMessage[] = [];
  if (Array.isArray(rawMessages)) {
    for (const item of rawMessages) {
      const parsed = parseMessage(item);
      if (parsed) messages.push(parsed);
    }
  }

  const summaryMsg = messages.find(m => m.code === 'Summary');
  const summary =
    summaryMsg?.text ||
    VALIDATE_MSG[code] ||
    `校验结果码：${code}`;

  return {
    code,
    passed: code === 1,
    summary,
    issues,
    detail,
    messages
  };
}
