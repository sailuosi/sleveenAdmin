/**
 * form-create Conf/Fields 编解码（对齐 n_flowable adapters/utils/formCreate）
 */
import { isRef } from 'vue';
import formCreate from '@form-create/element-ui';
import {
  fieldNameOf,
  normalizeFieldMode,
  resolvePermissionMode,
  type FieldPermissionLike,
  type SailFieldMode
} from './fieldModes';

export const encodeConf = (designerRef: { value?: any }) => {
  return formCreate.toJson(designerRef.value.getOption());
};

export const decodeConf = (conf: string) => {
  if (!conf) return {};
  try {
    return formCreate.parseJson(conf);
  } catch {
    return typeof conf === 'string' ? JSON.parse(conf) : conf;
  }
};

export const encodeFields = (designerRef: { value?: any }) => {
  const rule = designerRef.value.getRule() || [];
  const fields: string[] = [];
  rule.forEach((item: any) => {
    fields.push(formCreate.toJson(item));
  });
  return fields;
};

export const decodeFields = (fields: string[] | string) => {
  let arr: string[] = [];
  if (typeof fields === 'string') {
    try {
      arr = JSON.parse(fields);
    } catch {
      arr = [];
    }
  } else {
    arr = fields || [];
  }
  const rule: object[] = [];
  arr.forEach(item => {
    if (typeof item === 'string') {
      try {
        rule.push(formCreate.parseJson(item));
      } catch {
        try {
          rule.push(JSON.parse(item));
        } catch {
          /* skip */
        }
      }
    } else if (item && typeof item === 'object') {
      rule.push(item);
    }
  });
  return rule;
};

export const setConfAndFields = (
  designerRef: { value?: any },
  conf: string,
  fields: string[] | string
) => {
  designerRef.value.setOption(decodeConf(conf));
  designerRef.value.setRule(decodeFields(fields));
};

export const setConfAndFields2 = (
  detailPreview: any,
  conf: string,
  fields: string[] | string,
  value?: object
) => {
  const target = isRef(detailPreview) ? detailPreview.value : detailPreview;
  target.option = decodeConf(conf);
  target.rule = decodeFields(fields);
  if (value) {
    target.value = value;
  }
};

/** 将 form-create 值扁平为 Conditions 字典 */
export function formValuesToConditions(values: Record<string, unknown> | null | undefined) {
  const out: Record<string, string> = {};
  if (!values) return out;
  Object.entries(values).forEach(([k, v]) => {
    if (v === undefined || v === null) return;
    out[k] = typeof v === 'string' ? v : JSON.stringify(v);
  });
  return out;
}

/** 从 form-create fields/rules 收集 field 名（白名单）。 */
export function collectFormFieldNames(fields: string[] | string | any[] | null | undefined): Set<string> {
  const names = new Set<string>();
  const walk = (list: any[]) => {
    (list || []).forEach(item => {
      if (!item || typeof item !== 'object') return;
      const field = item.field ?? item.name;
      if (field != null && String(field).trim()) names.add(String(field));
      if (Array.isArray(item.children)) walk(item.children);
    });
  };
  walk(decodeFields((fields as any) || []));
  return names;
}

/**
 * 办理提交：有 fieldModes 时仅 editable 字段进入 Conditions；
 * 无权限配置时全量（兼容发起）。
 */
export function formValuesToConditionsForSubmit(
  values: Record<string, unknown> | null | undefined,
  fieldModes?: Record<string, string> | null
) {
  const all = formValuesToConditions(values);
  if (!fieldModes || Object.keys(fieldModes).length === 0) return all;
  const out: Record<string, string> = {};
  Object.entries(all).forEach(([k, v]) => {
    const mode = normalizeFieldMode(fieldModes[k]);
    if (mode === 'editable') out[k] = v;
  });
  return out;
}

/** 回填：只保留表单字段白名单内的键。 */
export function filterFormValuesByFields(
  values: Record<string, unknown> | null | undefined,
  fields: string[] | string | any[] | null | undefined
) {
  if (!values) return {};
  const allowed = collectFormFieldNames(fields);
  if (allowed.size === 0) return { ...values };
  const out: Record<string, unknown> = {};
  Object.entries(values).forEach(([k, v]) => {
    if (allowed.has(k)) out[k] = v;
  });
  return out;
}

/** 按字段权限改写 rule（§C：editable|readonly|hidden|masked） */
export function applyFieldPermissions(
  rules: any[],
  permissions: FieldPermissionLike[] | null | undefined
) {
  if (!permissions?.length) return rules;
  const map = new Map<string, SailFieldMode>();
  permissions.forEach(p => {
    const name = fieldNameOf(p);
    if (!name) return;
    map.set(name, resolvePermissionMode(p));
  });
  const walk = (list: any[]): any[] =>
    (list || [])
      .map(item => {
        const field = item?.field || item?.name;
        const mode = field ? map.get(String(field)) : undefined;
        if (mode === 'hidden') return null;
        const next = { ...item };
        if (mode === 'readonly' || mode === 'masked') {
          next.props = {
            ...(next.props || {}),
            disabled: true,
            readonly: true,
            ...(mode === 'masked' ? { 'data-sf-masked': true } : {})
          };
        }
        if (Array.isArray(next.children)) {
          next.children = walk(next.children).filter(Boolean);
        }
        return next;
      })
      .filter(Boolean);
  return walk(rules);
}
