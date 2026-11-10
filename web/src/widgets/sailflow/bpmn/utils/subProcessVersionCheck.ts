/**
 * D1：子流程绑定钉住定义行主键（subId）；检测是否存在同 ProcessId 的更新定义行。
 * 不自动跟版；由调用方提示并经用户确认后再改关联。
 */
import { getProcessListSimple } from '@sail/api/process';

export type ProcessDefRow = {
  ProcessDefId: string;
  ProcessId: string;
  ProcessName: string;
  Version: string;
  UpdatedDateTime?: string | null;
};

export type SubBinding = {
  subId: string;
  subProcessId: string;
  subProcessName?: string;
};

export type StaleSubBinding = SubBinding & {
  pinnedVersion?: string;
  latest: ProcessDefRow;
};

function pick(row: Record<string, unknown>, ...keys: string[]): unknown {
  for (const k of keys) {
    if (row[k] != null && row[k] !== '') return row[k];
  }
  return undefined;
}

export function normalizeProcessDefRow(raw: Record<string, unknown>): ProcessDefRow | null {
  const processDefId = String(pick(raw, 'ProcessDefId', 'processDefId', 'Id', 'id') ?? '');
  const processId = String(pick(raw, 'ProcessId', 'processId') ?? '');
  if (!processDefId || !processId) return null;
  return {
    ProcessDefId: processDefId,
    ProcessId: processId,
    ProcessName: String(pick(raw, 'ProcessName', 'processName') ?? ''),
    Version: String(pick(raw, 'Version', 'version') ?? ''),
    UpdatedDateTime: (pick(raw, 'UpdatedDateTime', 'updatedDateTime') as string | null | undefined) ?? null
  };
}

/** 同 ProcessId 下取「最新」定义行：优先 UpdatedDateTime，再 Version 字符串，再 ProcessDefId。 */
export function pickLatestDef(rows: ProcessDefRow[], processId: string): ProcessDefRow | null {
  const list = rows.filter(r => r.ProcessId === processId);
  if (!list.length) return null;
  return [...list].sort((a, b) => {
    const ta = a.UpdatedDateTime ? Date.parse(a.UpdatedDateTime) : 0;
    const tb = b.UpdatedDateTime ? Date.parse(b.UpdatedDateTime) : 0;
    if (tb !== ta) return tb - ta;
    const cv = String(b.Version).localeCompare(String(a.Version), undefined, { numeric: true });
    if (cv !== 0) return cv;
    return String(b.ProcessDefId).localeCompare(String(a.ProcessDefId));
  })[0];
}

export function checkSubBindingStale(
  binding: SubBinding,
  rows: ProcessDefRow[]
): StaleSubBinding | null {
  if (!binding.subProcessId || !binding.subId) return null;
  const latest = pickLatestDef(rows, binding.subProcessId);
  if (!latest) return null;
  if (latest.ProcessDefId === binding.subId) return null;
  const pinned = rows.find(r => r.ProcessDefId === binding.subId);
  return {
    ...binding,
    pinnedVersion: pinned?.Version,
    latest
  };
}

/** 从 BPMN XML 抽出 sf:SubInfo 绑定（属性名大小写不敏感）。 */
export function extractSubInfoBindingsFromXml(xml: string): SubBinding[] {
  if (!xml) return [];
  const out: SubBinding[] = [];
  const re = /<\s*[\w.-]*:?SubInfo\b([^>]*)\/?>/gi;
  let m: RegExpExecArray | null;
  while ((m = re.exec(xml))) {
    const attrs = m[1] || '';
    const get = (name: string) => {
      const am = attrs.match(new RegExp(`\\b${name}\\s*=\\s*["']([^"']*)["']`, 'i'));
      return am?.[1] ?? '';
    };
    const subId = get('subId');
    const subProcessId = get('subProcessId');
    if (!subId && !subProcessId) continue;
    out.push({
      subId,
      subProcessId,
      subProcessName: get('subProcessName')
    });
  }
  return out;
}

export function findStaleSubBindings(
  bindings: SubBinding[],
  rows: ProcessDefRow[]
): StaleSubBinding[] {
  const seen = new Set<string>();
  const stale: StaleSubBinding[] = [];
  for (const b of bindings) {
    const hit = checkSubBindingStale(b, rows);
    if (!hit) continue;
    const key = `${hit.subProcessId}|${hit.subId}`;
    if (seen.has(key)) continue;
    seen.add(key);
    stale.push(hit);
  }
  return stale;
}

export async function fetchProcessDefRows(): Promise<ProcessDefRow[]> {
  const data = await getProcessListSimple();
  if (data?.Status === -1 || data?.Status === '-1') {
    throw new Error(data?.Message || '加载流程列表失败');
  }
  const list = data?.Entity ?? data?.EntityList ?? data;
  const arr = Array.isArray(list) ? list : [];
  return arr
    .map((r: Record<string, unknown>) => normalizeProcessDefRow(r))
    .filter((r): r is ProcessDefRow => !!r);
}

export function formatStaleMessage(stale: StaleSubBinding[]): string {
  const lines = stale.map(s => {
    const name = s.subProcessName || s.latest.ProcessName || s.subProcessId;
    const pinned = s.pinnedVersion ? `v${s.pinnedVersion}` : `行 ${s.subId.slice(0, 8)}…`;
    return `· ${name}：当前钉住 ${pinned}，最新 v${s.latest.Version || '?'}`;
  });
  return [
    '以下子流程关联仍钉住旧定义行，不会自动升版。',
    '请打开子流程配置并确认后「更新关联」，或仍保存当前绑定。',
    '',
    ...lines
  ].join('\n');
}
