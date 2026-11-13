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

function readDoc(bo: ModdleEl & { documentation?: unknown }): string {
  if (bo.documentation == null) return '';
  if (typeof bo.documentation === 'string') return bo.documentation;
  if (Array.isArray(bo.documentation)) {
    return str((bo.documentation[0] as any)?.text || (bo.documentation[0] as any)?.body);
  }
  return str((bo.documentation as any)?.text || (bo.documentation as any)?.body || bo.documentation);
}

export function loadNodeConfig(element: unknown): SailNodeConfigModel {
  const bo = getBo(element) as ModdleEl & {
    id?: string;
    name?: string;
    code?: string;
    url?: string;
    documentation?: unknown;
    loopCharacteristics?: unknown;
    loopCardinality?: unknown;
    isSequential?: unknown;
  };
  const resolve = getExtension(bo, 'sf:AssigneeResolve');
  const candSel = getExtension(bo, 'sf:CandidateSelector');
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

  let loopCharacteristics = str(bo.loopCharacteristics);
  if (!loopCharacteristics && bo.isSequential === true) loopCharacteristics = 'sequential';
  if (loopCharacteristics !== 'parallel' && loopCharacteristics !== 'sequential') {
    loopCharacteristics = '';
  }

  return {
    id: str(bo.id || (element as any)?.id),
    name: str(bo.name),
    code: str(bo.code),
    url: str(bo.url),
    documentation: readDoc(bo),
    loopCharacteristics,
    loopCardinality: str(bo.loopCardinality),
    assigneeResolveType: str(resolve?.type),
    assigneeResolveParams: str(resolve?.params),
    candidateSelectorType: str(candSel?.type),
    candidateSelectorParams: str(candSel?.params),
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
    const created = moddle.create(collectionType, { [listProp]: [] }) as ModdleEl;
    (ee.values as ModdleEl[]).push(created);
    col = created;
  }
  const collection = col;
  const list = items.map(row => {
    const data: Record<string, string> = {};
    for (const k of props) data[k] = row[k] ?? '';
    const el = moddle.create(itemType, data);
    el.$parent = collection;
    return el;
  });
  collection[listProp] = list;
  if (typeof collection.set === 'function') collection.set(listProp, list);
}

export function saveNodeConfig(
  element: unknown,
  model: SailNodeConfigModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);

  // 基本属性 + BPMN 文档 / 多实例
  modeling.updateProperties(element, {
    name: model.name,
    code: model.code || undefined,
    url: model.url || undefined,
    documentation: model.documentation || undefined,
    loopCharacteristics: model.loopCharacteristics || undefined,
    isSequential: model.loopCharacteristics === 'sequential',
    loopCardinality: model.loopCardinality || undefined,
  });

  // 重新取 BO（updateProperties 后仍同一引用）
  const resolve = ensureChildExtension(bo, 'sf:AssigneeResolve', moddle);
  resolve.type = model.assigneeResolveType;
  resolve.params = model.assigneeResolveParams;

  const candSel = ensureChildExtension(bo, 'sf:CandidateSelector', moddle);
  candSel.type = model.candidateSelectorType;
  candSel.params = model.candidateSelectorParams;

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
      const created = moddle.create('sf:Actions', { actions: [] }) as ModdleEl;
      (ee.values as ModdleEl[]).push(created);
      col = created;
    }
    const collection = col;
    const list = (model.actions || []).map((a: SailNodeActionRef) => {
      const el = moddle.create('sf:Action', {
        fireType: a.fireType,
        methodType: a.methodType,
        subMethodType: a.subMethodType,
        argus: a.argus,
        expression: a.expression
      });
      el.$parent = collection;
      return el;
    });
    collection.actions = list;
  }

  modeling.updateProperties(element, {
    extensionElements: bo.get?.('extensionElements') || bo.extensionElements
  });
}

export type { SailNodeConfigModel, SailNodeFormRef, SailNodePerformerRef, SailNodeActionRef };
