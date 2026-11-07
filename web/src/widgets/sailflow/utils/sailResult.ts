/** 解包 Sail ResponseResult（兼容大小写） */
export function unpackSailResult<T = unknown>(data: any): {
  ok: boolean;
  message: string;
  entity: T | null;
  total: number;
  newId: unknown;
  extra: unknown;
} {
  const status = data?.Status ?? data?.status;
  const ok = status === 1 || status === '1' || status === true;
  const entity = (data?.Entity ?? data?.entity ?? null) as T | null;
  const total = Number(data?.TotalRowsCount ?? data?.totalRowsCount ?? 0);
  return {
    ok: status === -1 || status === '-1' ? false : ok || (entity != null && status !== -1),
    message: String(data?.Message ?? data?.message ?? ''),
    entity,
    total,
    newId: data?.NewId ?? data?.newId,
    extra: data?.ExtraData ?? data?.extraData
  };
}

export function pickField<T = unknown>(row: Record<string, unknown> | null | undefined, ...keys: string[]): T | undefined {
  if (!row) return undefined;
  for (const k of keys) {
    if (row[k] !== undefined && row[k] !== null) return row[k] as T;
    const lower = k.charAt(0).toLowerCase() + k.slice(1);
    if (row[lower] !== undefined && row[lower] !== null) return row[lower] as T;
  }
  return undefined;
}
