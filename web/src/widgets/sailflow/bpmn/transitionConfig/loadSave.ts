import {
  ensureChildExtension,
  getBo,
  getExtension,
  optionName,
  optionValue,
  type ModdleEl
} from '../utils/sfExtension';
import {
  isApprovalOrSplit,
  isEOrJoin,
  isOrSplit,
  isXOrSplit
} from '../utils/sfModelUtility';
import {
  APPROVAL_OPTIONS,
  RECEIVER_OPTIONS,
  defaultTransitionConfig,
  type SailFlowType,
  type SailTransitionConfigModel,
  type SailTransitionContext
} from './types';

type Moddle = { create: (type: string, props?: object) => any };
type Modeling = {
  updateProperties: (el: unknown, props: object) => void;
};

type FlowEl = {
  id?: string;
  source?: any;
  target?: any;
  businessObject?: any;
};

function str(v: unknown) {
  return v == null ? '' : String(v);
}

function labelOf(el: any): string {
  if (!el) return '';
  const bo = el.businessObject || el;
  return str(bo.name || bo.id || el.id || '');
}

function resolveContext(element: FlowEl): SailTransitionContext {
  const source = element.source;
  const target = element.target;
  return {
    showApproval: !!(source && isApprovalOrSplit(source)),
    showPriority: !!(source && isXOrSplit(source)),
    showDefaultBranch: !!(source && isOrSplit(source)),
    showForcedMerge: !!(target && isEOrJoin(target)),
    sourceLabel: labelOf(source),
    targetLabel: labelOf(target)
  };
}

function readFlowType(element: FlowEl, bo: ModdleEl): SailFlowType {
  const sourceBo = element.source?.businessObject;
  if (sourceBo?.default && (sourceBo.default.id === bo.id || sourceBo.default === bo)) {
    return 'default';
  }
  if ((bo as any).conditionExpression) return 'condition';
  return 'normal';
}

export function loadTransitionConfig(element: unknown): SailTransitionConfigModel {
  const flow = element as FlowEl;
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string; conditionExpression?: any };
  const ext = getExtension(bo, 'sf:GroupBehaviours');
  const base = defaultTransitionConfig();
  const receiverName = str(ext?.recieverType);
  const approvalName = str(ext?.approval);

  return {
    ...base,
    id: str(bo.id || flow.id),
    name: str(bo.name),
    flowType: readFlowType(flow, bo),
    conditionExpression: str(bo.conditionExpression?.body),
    recieverType: optionValue([...RECEIVER_OPTIONS], receiverName) || '0',
    approval: optionValue([...APPROVAL_OPTIONS], approvalName) || '1',
    priority: str(ext?.priority),
    defaultBranch: ext?.defaultBranch === true || ext?.defaultBranch === 'true',
    forcedMerge: ext?.forcedMerge === true || ext?.forcedMerge === 'true',
    context: resolveContext(flow)
  };
}

export function saveTransitionConfig(
  element: unknown,
  model: SailTransitionConfigModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const flow = element as FlowEl;
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const source = flow.source;
  const sourceBo = source?.businessObject;

  // 名称
  modeling.updateProperties(element, { name: model.name || undefined });

  // 流转类型
  if (model.flowType === 'default') {
    modeling.updateProperties(element, { conditionExpression: undefined });
    if (source) {
      modeling.updateProperties(source, { default: element });
    }
  } else if (model.flowType === 'condition') {
    if (sourceBo?.default && (sourceBo.default.id === bo.id || sourceBo.default === bo)) {
      modeling.updateProperties(source, { default: undefined });
    }
    const expression = moddle.create('bpmn:Expression', {
      body: model.conditionExpression || ''
    });
    modeling.updateProperties(element, { conditionExpression: expression });
  } else {
    // normal
    if (sourceBo?.default && (sourceBo.default.id === bo.id || sourceBo.default === bo)) {
      modeling.updateProperties(source, { default: undefined });
    }
    modeling.updateProperties(element, { conditionExpression: undefined });
  }

  // GroupBehaviours
  const ext = ensureChildExtension(bo, 'sf:GroupBehaviours', moddle);
  ext.recieverType = optionName([...RECEIVER_OPTIONS], model.recieverType);
  if (model.context.showApproval) {
    ext.approval = optionName([...APPROVAL_OPTIONS], model.approval);
  }
  if (model.context.showPriority) {
    ext.priority = model.priority || undefined;
  }
  if (model.context.showDefaultBranch) {
    ext.defaultBranch = String(!!model.defaultBranch);
  }
  if (model.context.showForcedMerge) {
    ext.forcedMerge = String(!!model.forcedMerge);
  }
  modeling.updateProperties(element, { extensionElements: bo.extensionElements });
}
