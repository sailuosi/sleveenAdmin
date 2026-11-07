import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { defaultButtonsSetting, parseButtonsJson } from '../buttonsCatalog';
import {
  ensureChildExtension,
  ensureExtensionElements,
  getBo,
  getExtension,
  type ModdleEl
} from '../utils/sfExtension';
import {
  defaultNodePolicy,
  type SailNodeActionRef,
  type SailNodeConfigModel,
  type SailNodeFormRef,
  type SailNodePerformerRef,
  type SailNodePolicy
} from './types';
import { normalizeFieldMode } from '../../utils/fieldModes';

type Moddle = { create: (type: string, props?: object) => any };
type Modeling = {
  updateProperties: (el: unknown, props: object) => void;
};

function str(v: unknown) {
  return v == null ? '' : String(v);
}

function normalizeFieldPerm(p: {
  field?: string;
  title?: string;
  permission?: string;
}) {
  return {
    field: str(p?.field),
    title: str(p?.title),
    permission: normalizeFieldMode(p?.permission)
  };
}

function readCollection(
  bo: ModdleEl,
  collectionType: string,
  listProp: string
): ModdleEl[] {
  const col = getExtension(bo, collectionType);
  if (!col) return [];
  const list =
    (col.get?.(listProp) as ModdleEl[] | undefined) ??
    (col[listProp] as ModdleEl[] | undefined);
  return Array.isArray(list) ? list : [];
}

function parsePolicy(json: string | undefined | null): SailNodePolicy {
  const base = defaultNodePolicy();
  if (!json?.trim()) return base;
  try {
    const raw = JSON.parse(json) as Partial<SailNodePolicy>;
    return {
      ...base,
      ...raw,
      assignEmptyUserIds: Array.isArray(raw.assignEmptyUserIds)
        ? raw.assignEmptyUserIds.map(String)
        : base.assignEmptyUserIds,
      fieldsPermission: Array.isArray(raw.fieldsPermission)
        ? raw.fieldsPermission.map(normalizeFieldPerm)
        : base.fieldsPermission,
      sendBackTarget:
        raw.sendBackTarget === 'Prev' ||
        raw.sendBackTarget === 'History' ||
        raw.sendBackTarget === 'Starter'
          ? raw.sendBackTarget
          : base.sendBackTarget,
      resubmitMode:
        raw.resubmitMode === 'ForwardNormal' || raw.resubmitMode === 'ReturnToSender'
          ? raw.resubmitMode
          : base.resubmitMode
    };
  } catch {
    return base;
  }
}

export function loadNodeConfig(element: unknown): SailNodeConfigModel {
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string; code?: string; url?: string };
  const resolve = getExtension(bo, 'sf:AssigneeResolve');
  const leave = getExtension(bo, 'sf:LeaveJudge');
  const ms = getExtension(bo, 'sf:MultiSignDetail');
  const buttons = getExtension(bo, 'sf:ButtonsSetting');
  const policyExt = getExtension(bo, 'sf:NodePolicy');

  const performers = readCollection(bo, 'sf:Performers', 'values').map(p => ({
    name: str(p.name),
    outerId: str(p.outerId),
    outerCode: str(p.outerCode),
    outerType: str(p.outerType) || 'User'
  }));

  const forms = readCollection(bo, 'sf:Forms', 'values').map(f => ({
    name: str(f.name),
    outerId: str(f.outerId),
    outerCode: str(f.outerCode)
  }));

  const notifications = readCollection(bo, 'sf:Notifications', 'values').map(n => ({
    name: str(n.name),
    outerId: str(n.outerId),
    outerCode: str(n.outerCode),
    outerType: str(n.outerType) || 'User'
  }));

  const actions = readCollection(bo, 'sf:Actions', 'actions').map(a => ({
    fireType: str(a.fireType) || '0',
    methodType: str(a.methodType) || '0',
    subMethodType: str(a.subMethodType) || '0',
    argus: str(a.argus),
    expression: str(a.expression)
  }));

  return {
    id: str(bo.id || (element as any)?.id),
    name: str(bo.name),
    code: str(bo.code),
    url: str(bo.url),
    assigneeResolveType: str(resolve?.type),
    assigneeResolveParams: str(resolve?.params),
    leaveJudgeType: str(leave?.type),
    leaveJudgeParams: str(leave?.params),
    multiSign: {
      complexType: str(ms?.complexType) || '0',
      mergeType: str(ms?.mergeType) || '0',
      compareType: str(ms?.compareType) || '0',
      completeOrder: str(ms?.completeOrder)
    },
    performers,
    forms,
    notifications,
    actions,
    buttons: parseButtonsJson(buttons?.json != null ? String(buttons.json) : ''),
    policy: parsePolicy(policyExt?.json != null ? String(policyExt.json) : '')
  };
}

function setCollection(
  bo: ModdleEl,
  moddle: Moddle,
  collectionType: string,
  listProp: string,
  itemType: string,
  items: Record<string, string>[],
  props: string[]
) {
  const ee = ensureExtensionElements(bo, moddle);
  let col = getExtension(bo, collectionType);
  if (!col) {
    col = moddle.create(collectionType, { [listProp]: [] });
    (ee.values as ModdleEl[]).push(col);
  }
  const list = items.map(row => {
    const data: Record<string, string> = {};
    for (const k of props) data[k] = row[k] ?? '';
    const el = moddle.create(itemType, data);
    el.$parent = col;
    return el;
  });
  col[listProp] = list;
  if (typeof col.set === 'function') col.set(listProp, list);
}

export function saveNodeConfig(
  element: unknown,
  model: SailNodeConfigModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);
  const businessObject = getBusinessObject(element as never);

  // 基本属性
  modeling.updateProperties(element, {
    name: model.name,
    code: model.code || undefined,
    url: model.url || undefined
  });

  // 重新取 BO（updateProperties 后仍同一引用）
  const resolve = ensureChildExtension(bo, 'sf:AssigneeResolve', moddle);
  resolve.type = model.assigneeResolveType;
  resolve.params = model.assigneeResolveParams;

  const leave = ensureChildExtension(bo, 'sf:LeaveJudge', moddle);
  leave.type = model.leaveJudgeType;
  leave.params = model.leaveJudgeParams;

  const ms = ensureChildExtension(bo, 'sf:MultiSignDetail', moddle);
  ms.complexType = model.multiSign.complexType;
  ms.mergeType = model.multiSign.mergeType;
  ms.compareType = model.multiSign.compareType;
  ms.completeOrder = model.multiSign.completeOrder;

  const buttons = ensureChildExtension(bo, 'sf:ButtonsSetting', moddle);
  buttons.json = JSON.stringify(model.buttons?.length ? model.buttons : defaultButtonsSetting());

  const policy = ensureChildExtension(bo, 'sf:NodePolicy', moddle);
  policy.json = JSON.stringify(model.policy);

  setCollection(
    bo,
    moddle,
    'sf:Performers',
    'values',
    'sf:Performer',
    model.performers as unknown as Record<string, string>[],
    ['name', 'outerId', 'outerCode', 'outerType']
  );
  setCollection(
    bo,
    moddle,
    'sf:Forms',
    'values',
    'sf:Form',
    model.forms as unknown as Record<string, string>[],
    ['name', 'outerId', 'outerCode']
  );
  setCollection(
    bo,
    moddle,
    'sf:Notifications',
    'values',
    'sf:Notification',
    model.notifications as unknown as Record<string, string>[],
    ['name', 'outerId', 'outerCode', 'outerType']
  );

  // Actions 集合属性名是 actions
  {
    const ee = ensureExtensionElements(bo, moddle);
    let col = getExtension(bo, 'sf:Actions');
    if (!col) {
      col = moddle.create('sf:Actions', { actions: [] });
      (ee.values as ModdleEl[]).push(col);
    }
    const list = (model.actions || []).map((a: SailNodeActionRef) => {
      const el = moddle.create('sf:Action', {
        fireType: a.fireType,
        methodType: a.methodType,
        subMethodType: a.subMethodType,
        argus: a.argus,
        expression: a.expression
      });
      el.$parent = col;
      return el;
    });
    col.actions = list;
  }

  modeling.updateProperties(element, {
    extensionElements: businessObject.get('extensionElements') || bo.extensionElements
  });
}

export type { SailNodeConfigModel, SailNodeFormRef, SailNodePerformerRef, SailNodeActionRef };
