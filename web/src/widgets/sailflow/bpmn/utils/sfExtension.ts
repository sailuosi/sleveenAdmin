import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';

export type ModdleEl = {
  $instanceOf?: (type: string) => boolean;
  get?: (name: string) => unknown;
  extensionElements?: ModdleEl;
  values?: ModdleEl[];
  [key: string]: unknown;
};

/** Supports bpmn-js shapes and vue-bpmn `__sailVue` wrappers. */
export function getBo(element: unknown): ModdleEl {
  const anyEl = element as { __sailVue?: boolean; businessObject?: ModdleEl } | null;
  if (anyEl?.__sailVue && anyEl.businessObject) {
    return anyEl.businessObject;
  }
  if (anyEl?.businessObject && typeof (anyEl.businessObject as ModdleEl).$instanceOf === 'function') {
    return anyEl.businessObject;
  }
  return getBusinessObject(element as never) as ModdleEl;
}

export function getExtension(bo: ModdleEl, type: string): ModdleEl | undefined {
  const values = bo.extensionElements?.values;
  if (!values?.length) return undefined;
  return values.find(e => e.$instanceOf?.(type));
}

export function ensureExtensionElements(
  bo: ModdleEl,
  moddle: { create: (type: string, props?: object) => ModdleEl }
): ModdleEl {
  if (!bo.extensionElements) {
    bo.extensionElements = moddle.create('bpmn:ExtensionElements', { values: [] });
  }
  const ee = bo.extensionElements;
  if (!ee.values) {
    ee.values = [];
  }
  return ee;
}

export function ensureChildExtension(
  bo: ModdleEl,
  type: string,
  moddle: { create: (type: string, props?: object) => ModdleEl }
): ModdleEl {
  const ee = ensureExtensionElements(bo, moddle);
  let child = getExtension(bo, type);
  if (!child) {
    child = moddle.create(type);
    (ee.values as ModdleEl[]).push(child);
  } else if (typeof child.$type === 'string' && child.$type !== type) {
    // Reuse case-variant (AssigneeResolve vs assigneeResolve); keep camelCase $type from create()
    const created = moddle.create(type);
    if (typeof created.$type === 'string') child.$type = created.$type;
  }
  return child;
}

/** Remove a child extension by type; clear extensionElements when empty. */
export function removeChildExtension(bo: ModdleEl, type: string): void {
  const ee = bo.extensionElements;
  const values = ee?.values;
  if (!values?.length) return;
  const next = values.filter(e => !e.$instanceOf?.(type));
  if (next.length === values.length) return;
  if (next.length === 0) {
    bo.extensionElements = undefined;
  } else {
    ee!.values = next;
  }
}

/**
 * 确保 collectionType（如 sf:Scripts）下 listProp 数组至少有一个 itemType（如 sf:Script）
 */
export function ensureNestedListItem(
  bo: ModdleEl,
  collectionType: string,
  listProp: string,
  itemType: string,
  moddle: { create: (type: string, props?: object) => ModdleEl }
): ModdleEl {
  const collection = ensureChildExtension(bo, collectionType, moddle);
  let list = (collection.get?.(listProp) as ModdleEl[] | undefined) ?? (collection[listProp] as ModdleEl[] | undefined);
  if (!Array.isArray(list)) {
    list = [];
    collection[listProp] = list;
  }
  if (!list.length) {
    const item = moddle.create(itemType);
    item.$parent = collection;
    list.push(item);
  }
  return list[0];
}

export type SelectOption = { label: string; value: string; name: string };

export function optionName(options: SelectOption[], value: string): string {
  return options.find(o => o.value === value)?.name ?? '';
}

export function optionValue(options: SelectOption[], name: string): string {
  return options.find(o => o.name === name)?.value ?? '';
}
