/** §C 字段编辑性 string 枚举（设计器 / 落盘 / Workbench / 表单契约共用） */
export type SailFieldMode = 'editable' | 'readonly' | 'hidden' | 'masked';

export const SAIL_FIELD_MODE_OPTIONS: Array<{ label: string; value: SailFieldMode }> = [
  { label: '可编辑', value: 'editable' },
  { label: '只读', value: 'readonly' },
  { label: '隐藏', value: 'hidden' },
  { label: '脱敏', value: 'masked' }
];

/** 归一旧 READ/WRITE/NONE/DESENSITIZE → 小写四码 */
export function normalizeFieldMode(raw: string | null | undefined): SailFieldMode {
  const s = String(raw ?? '').trim().toLowerCase();
  if (s === 'editable' || s === 'write') return 'editable';
  if (s === 'hidden' || s === 'none') return 'hidden';
  if (s === 'masked' || s === 'desensitize' || s === 'desensitise') return 'masked';
  return 'readonly';
}

/** 内置表单脱敏展示（不改提交原文由调用方保证） */
export function maskDisplayValue(v: unknown): string {
  const s = v == null ? '' : String(v);
  if (!s) return '';
  if (s.length <= 2) return '**';
  if (s.length <= 6) return `${s[0]}***${s[s.length - 1]}`;
  return `${s.slice(0, 2)}****${s.slice(-2)}`;
}

export type FieldPermissionLike = {
  FieldName?: string;
  fieldName?: string;
  field?: string;
  Mode?: string;
  mode?: string;
  permission?: string;
  IsNotVisible?: boolean;
  isNotVisible?: boolean;
  IsReadOnly?: boolean;
  isReadOnly?: boolean;
};

export function resolvePermissionMode(p: FieldPermissionLike): SailFieldMode {
  const explicit = p.Mode ?? p.mode ?? p.permission;
  if (explicit != null && String(explicit).trim() !== '') {
    return normalizeFieldMode(String(explicit));
  }
  if (p.IsNotVisible ?? p.isNotVisible) return 'hidden';
  if (p.IsReadOnly ?? p.isReadOnly) return 'readonly';
  return 'editable';
}

export function fieldNameOf(p: FieldPermissionLike): string {
  return String(p.FieldName ?? p.fieldName ?? p.field ?? '');
}
