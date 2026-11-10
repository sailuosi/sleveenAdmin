import { getBusinessObject, is } from 'bpmn-js/lib/util/ModelUtil';
import {
  ensureChildExtension,
  ensureNestedListItem,
  getBo,
  getExtension,
  optionName,
  optionValue,
  type ModdleEl
} from '../utils/sfExtension';
import { isJoinTarget, isSplitSource } from '../utils/sfModelUtility';
import {
  AI_OPTS,
  HTTP_OPTS,
  JOIN_OPTS,
  METHOD_OPTS,
  PASS_OPTS,
  SCRIPT_OPTS,
  SPLIT_OPTS,
  type SailBasicPropsModel,
  type SailEventModel,
  type SailGatewayModel,
  type SailRuleModel,
  type SailScriptModel,
  type SailServiceModel,
  type SailSubProcessModel
} from './types';

type Moddle = { create: (type: string, props?: object) => any };
type Modeling = {
  updateProperties: (el: unknown, props: object) => void;
  updateModdleProperties?: (el: unknown, m: unknown, props: object) => void;
};

function str(v: unknown) {
  return v == null ? '' : String(v);
}

function getNestedFirst(bo: ModdleEl, collectionType: string, listProp: string) {
  const col = getExtension(bo, collectionType);
  const list =
    (col?.get?.(listProp) as ModdleEl[] | undefined) ??
    (col?.[listProp] as ModdleEl[] | undefined);
  return Array.isArray(list) ? list[0] : undefined;
}

export function readDocumentation(element: unknown): string {
  const bo = getBo(element) as ModdleEl & { documentation?: unknown };
  if (bo.documentation == null) return '';
  if (typeof bo.documentation === 'string') return bo.documentation;
  if (Array.isArray(bo.documentation)) {
    return str((bo.documentation[0] as any)?.text || (bo.documentation[0] as any)?.body);
  }
  return str((bo.documentation as any)?.text || (bo.documentation as any)?.body || bo.documentation);
}

export function loadGateway(element: unknown): SailGatewayModel {
  const flow = element as { id?: string; incoming?: unknown[]; outgoing?: unknown[] };
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const ext = getExtension(bo, 'sf:GatewayDetail');
  return {
    id: str(bo.id || flow.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    extraSplitType: optionValue(SPLIT_OPTS, str(ext?.extraSplitType)) || '0',
    extraJoinType: optionValue(JOIN_OPTS, str(ext?.extraJoinType)) || '0',
    joinPassType: optionValue(PASS_OPTS, str(ext?.joinPassType)) || '0',
    showSplit: isSplitSource(flow),
    showJoin: isJoinTarget(flow)
  };
}

export function saveGateway(
  element: unknown,
  model: SailGatewayModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);
  modeling.updateProperties(element, {
    name: model.name || undefined,
    documentation: model.documentation || undefined,
  });
  const ext = ensureChildExtension(bo, 'sf:GatewayDetail', moddle);
  if (model.showSplit) ext.extraSplitType = optionName(SPLIT_OPTS, model.extraSplitType);
  if (model.showJoin) {
    ext.extraJoinType = optionName(JOIN_OPTS, model.extraJoinType);
    ext.joinPassType = optionName(PASS_OPTS, model.joinPassType);
  }
  modeling.updateProperties(element, { extensionElements: bo.extensionElements });
}

export function loadService(element: unknown): SailServiceModel {
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const svc = getNestedFirst(bo, 'sf:Services', 'services');
  const ai = getNestedFirst(bo, 'sf:AiServices', 'aiServices');
  return {
    id: str(bo.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    methodType: optionValue(METHOD_OPTS, str(svc?.methodType)) || '0',
    subMethodType: optionValue(HTTP_OPTS, str(svc?.subMethodType)) || '0',
    argus: str(svc?.argus),
    expression: str(svc?.expression),
    aiType: optionValue(AI_OPTS, str(ai?.type)) || '',
    configUUID: str(ai?.configUUID),
    toolSetName: str(ai?.toolSetName)
  };
}

export function saveService(
  element: unknown,
  model: SailServiceModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);
  modeling.updateProperties(element, {
    name: model.name || undefined,
    documentation: model.documentation || undefined,
  });
  const svc = ensureNestedListItem(bo, 'sf:Services', 'services', 'sf:Service', moddle);
  svc.methodType = optionName(METHOD_OPTS, model.methodType);
  svc.subMethodType = optionName(HTTP_OPTS, model.subMethodType);
  svc.argus = model.argus || undefined;
  svc.expression = model.expression || undefined;
  if (model.aiType) {
    const ai = ensureNestedListItem(bo, 'sf:AiServices', 'aiServices', 'sf:AiService', moddle);
    ai.type = optionName(AI_OPTS, model.aiType);
    ai.configUUID = model.configUUID || undefined;
    ai.toolSetName = model.aiType === '3' ? model.toolSetName || undefined : undefined;
  }
  modeling.updateProperties(element, { extensionElements: bo.extensionElements });
}

export function loadScript(element: unknown): SailScriptModel {
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const sc = getNestedFirst(bo, 'sf:Scripts', 'scripts');
  return {
    id: str(bo.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    scriptType: optionValue(SCRIPT_OPTS, str(sc?.scriptType)) || '0',
    argus: str(sc?.argus),
    scriptText: str(sc?.scriptText)
  };
}

export function saveScript(
  element: unknown,
  model: SailScriptModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);
  modeling.updateProperties(element, {
    name: model.name || undefined,
    documentation: model.documentation || undefined,
  });
  const sc = ensureNestedListItem(bo, 'sf:Scripts', 'scripts', 'sf:Script', moddle);
  sc.scriptType = optionName(SCRIPT_OPTS, model.scriptType);
  sc.argus = model.argus || undefined;
  sc.scriptText = model.scriptText || undefined;
  modeling.updateProperties(element, { extensionElements: bo.extensionElements });
}

export function loadRule(element: unknown): SailRuleModel {
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const rc = getNestedFirst(bo, 'sf:RuleConfigs', 'ruleConfigs');
  return {
    id: str(bo.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    ruleSetCode: str(rc?.ruleSetCode)
  };
}

export function saveRule(
  element: unknown,
  model: SailRuleModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);
  modeling.updateProperties(element, {
    name: model.name || undefined,
    documentation: model.documentation || undefined,
  });
  if (model.ruleSetCode) {
    const item = ensureNestedListItem(bo, 'sf:RuleConfigs', 'ruleConfigs', 'sf:RuleConfig', moddle);
    item.ruleSetCode = model.ruleSetCode;
    modeling.updateProperties(element, { extensionElements: bo.extensionElements });
  }
}

export function loadSubProcess(element: unknown): SailSubProcessModel {
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const col = getExtension(bo, 'sf:SubInfoes');
  const list =
    (col?.get?.('values') as ModdleEl[] | undefined) ?? (col?.values as ModdleEl[] | undefined);
  const first = Array.isArray(list) ? list[0] : undefined;
  return {
    id: str(bo.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    subId: str(first?.subId),
    subProcessId: str(first?.subProcessId),
    subProcessName: str(first?.subProcessName)
  };
}

export function saveSubProcess(
  element: unknown,
  model: SailSubProcessModel,
  modeling: Modeling,
  moddle: Moddle
) {
  const bo = getBo(element);
  modeling.updateProperties(element, {
    name: model.name || undefined,
    documentation: model.documentation || undefined,
  });
  if (!model.subProcessId) return;
  // D1：subId 必须是定义行主键；禁止用流程设计定义键 ProcessId 兜底
  if (!model.subId) {
    throw new Error('子流程未绑定定义行主键（subId）；请重新选择子流程');
  }
  const col = ensureChildExtension(bo, 'sf:SubInfoes', moddle);
  let list = (col.get?.('values') as ModdleEl[] | undefined) ?? (col.values as ModdleEl[] | undefined);
  if (!Array.isArray(list)) {
    list = [];
    col.values = list;
  }
  let item = list[0];
  if (!item) {
    item = moddle.create('sf:SubInfo');
    item.$parent = col;
    list.push(item);
  }
  item.subProcessId = model.subProcessId;
  item.subProcessName = model.subProcessName;
  item.subId = model.subId;
  modeling.updateProperties(element, { extensionElements: bo.extensionElements });
}

export function loadBasicProps(element: unknown): SailBasicPropsModel {
  const anyEl = element as {
    id?: string;
    type?: string;
    businessObject?: ModdleEl & { processRef?: string; isExecutable?: boolean };
  };
  const bo = getBo(element) as ModdleEl & {
    id?: string;
    name?: string;
    processRef?: string;
    isExecutable?: boolean;
  };
  const type = String(anyEl.type || bo.$type || '');
  return {
    id: str(bo.id || anyEl.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    type,
    isExecutable: type === 'bpmn:Process' ? bo.isExecutable !== false : undefined,
    processRef: type === 'bpmn:Participant' ? str(bo.processRef) : undefined,
  };
}

export function saveBasicProps(
  element: unknown,
  model: SailBasicPropsModel,
  modeling: Modeling,
) {
  const props: Record<string, unknown> = {
    name: model.name || undefined,
    documentation: model.documentation || undefined,
  };
  if (model.type === 'bpmn:Process' && model.isExecutable != null) {
    props.isExecutable = model.isExecutable;
  }
  modeling.updateProperties(element, props);
}

function isVueElement(element: unknown): boolean {
  return Boolean((element as { __sailVue?: boolean } | null)?.__sailVue);
}

function getEventDefinition(element: unknown): any | null {
  if (isVueElement(element)) {
    const bo = getBo(element) as ModdleEl & {
      eventDefinitions?: string[];
      timerKind?: string;
      timeDuration?: string;
      condition?: string;
      messageName?: string;
      signalName?: string;
      errorName?: string;
      escalationName?: string;
      linkName?: string;
    };
    const defs = bo.eventDefinitions;
    if (!Array.isArray(defs) || !defs.length) return null;
    const $type = String(defs[0]);
    return {
      $type,
      __vueFlat: true,
      timerKind: bo.timerKind,
      timeDuration: bo.timeDuration,
      condition: bo.condition != null ? { body: bo.condition } : undefined,
      messageRef: bo.messageName != null ? { name: bo.messageName } : undefined,
      signalRef: bo.signalName != null ? { name: bo.signalName } : undefined,
      errorRef: bo.errorName != null ? { name: bo.errorName } : undefined,
      escalationRef: bo.escalationName != null ? { name: bo.escalationName } : undefined,
      linkName: bo.linkName,
    };
  }
  const bo = getBusinessObject(element as never) as any;
  return bo?.eventDefinitions?.[0] || null;
}

export function loadEvent(element: unknown): SailEventModel {
  const bo = getBo(element) as ModdleEl & { id?: string; name?: string };
  const ed = getEventDefinition(element);
  const eventDefType = str(ed?.$type).replace('bpmn:', '');
  let timerKind: SailEventModel['timerKind'] = 'timeDuration';
  let timerBody = '';
  if (ed?.$type === 'bpmn:TimerEventDefinition' || eventDefType === 'TimerEventDefinition') {
    if (ed.__vueFlat) {
      timerKind = (ed.timerKind as SailEventModel['timerKind']) || 'timeDuration';
      timerBody = str(ed.timeDuration);
    } else if (ed.timeDate) {
      timerKind = 'timeDate';
      timerBody = str(ed.timeDate.body);
    } else if (ed.timeCycle) {
      timerKind = 'timeCycle';
      timerBody = str(ed.timeCycle.body);
    } else {
      timerKind = 'timeDuration';
      timerBody = str(ed.timeDuration?.body);
    }
  }
  return {
    id: str(bo.id),
    name: str(bo.name),
    documentation: readDocumentation(element),
    eventDefType,
    timerKind,
    timerBody,
    condition: str(ed?.condition?.body ?? ed?.condition),
    messageName: str(ed?.messageRef?.name ?? ed?.messageName),
    signalName: str(ed?.signalRef?.name ?? ed?.signalName),
    errorName: str(ed?.errorRef?.name ?? ed?.errorName),
    escalationName: str(ed?.escalationRef?.name ?? ed?.escalationName),
    linkName: str(ed?.linkName ?? ed?.name),
  };
}

export function saveEvent(
  element: unknown,
  model: SailEventModel,
  modeling: Modeling,
  moddle: Moddle
) {
  if (isVueElement(element)) {
    const props: Record<string, unknown> = {
      name: model.name || undefined,
      documentation: model.documentation || undefined,
    };
    const def = model.eventDefType
      ? model.eventDefType.startsWith('bpmn:')
        ? model.eventDefType
        : `bpmn:${model.eventDefType}`
      : '';
    if (def) props.eventDefinitions = [def];
    if (model.eventDefType === 'TimerEventDefinition' || def.includes('Timer')) {
      props.timerKind = model.timerKind;
      props.timeDuration = model.timerBody || '';
    } else if (model.eventDefType === 'ConditionalEventDefinition' || def.includes('Conditional')) {
      props.condition = model.condition || '';
    } else if (model.eventDefType === 'MessageEventDefinition' || def.includes('Message')) {
      props.messageName = model.messageName || '';
    } else if (model.eventDefType === 'SignalEventDefinition' || def.includes('Signal')) {
      props.signalName = model.signalName || '';
    } else if (model.eventDefType === 'ErrorEventDefinition' || def.includes('Error')) {
      props.errorName = model.errorName || '';
    } else if (model.eventDefType === 'EscalationEventDefinition' || def.includes('Escalation')) {
      props.escalationName = model.escalationName || '';
    } else if (model.eventDefType === 'LinkEventDefinition' || def.includes('Link')) {
      props.linkName = model.linkName || '';
    }
    modeling.updateProperties(element, props);
    return;
  }

  modeling.updateProperties(element, { name: model.name || undefined });
  const ed = getEventDefinition(element);
  if (!ed) return;
  const update = modeling.updateModdleProperties?.bind(modeling);
  if (ed.$type === 'bpmn:TimerEventDefinition' && update) {
    const formal = moddle.create('bpmn:FormalExpression', { body: model.timerBody || '' });
    update(element, ed, {
      timeDate: model.timerKind === 'timeDate' ? formal : undefined,
      timeDuration: model.timerKind === 'timeDuration' ? formal : undefined,
      timeCycle: model.timerKind === 'timeCycle' ? formal : undefined
    });
  } else if (ed.$type === 'bpmn:ConditionalEventDefinition') {
    ed.condition = moddle.create('bpmn:FormalExpression', { body: model.condition || '' });
  } else if (ed.$type === 'bpmn:MessageEventDefinition') {
    const bo = getBusinessObject(element as never) as any;
    const definitions = bo.$parent?.$parent;
    if (definitions?.rootElements) {
      let ref = ed.messageRef;
      if (!ref) {
        ref = moddle.create('bpmn:Message', {
          id: `Message_${Math.random().toString(36).slice(2, 8)}`,
          name: model.messageName || '消息'
        });
        definitions.rootElements.push(ref);
        ed.messageRef = ref;
      } else {
        ref.name = model.messageName;
      }
    }
  } else if (ed.$type === 'bpmn:SignalEventDefinition') {
    const bo = getBusinessObject(element as never) as any;
    const definitions = bo.$parent?.$parent;
    if (definitions?.rootElements) {
      let ref = ed.signalRef;
      if (!ref) {
        ref = moddle.create('bpmn:Signal', {
          id: `Signal_${Math.random().toString(36).slice(2, 8)}`,
          name: model.signalName || '信号'
        });
        definitions.rootElements.push(ref);
        ed.signalRef = ref;
      } else {
        ref.name = model.signalName;
      }
    }
  }
}

function elementTypeOf(element: unknown): string {
  const anyEl = element as { type?: string; $type?: string; businessObject?: { $type?: string } };
  return String(anyEl?.type || anyEl?.$type || anyEl?.businessObject?.$type || '');
}

export function detectElementKind(element: unknown): string | null {
  if (!element) return null;
  if (isVueElement(element)) {
    const t = elementTypeOf(element);
    if (t === 'bpmn:UserTask' || t === 'bpmn:Task') return 'node';
    if (t === 'bpmn:SequenceFlow') return 'transition';
    if (t.includes('Gateway')) return 'gateway';
    if (t === 'bpmn:ServiceTask') return 'service';
    if (t === 'bpmn:ScriptTask') return 'script';
    if (t === 'bpmn:BusinessRuleTask') return 'rule';
    if (t === 'bpmn:SubProcess' || t === 'bpmn:CallActivity') return 'subprocess';
    if (
      t === 'bpmn:StartEvent' ||
      t === 'bpmn:EndEvent' ||
      t === 'bpmn:IntermediateCatchEvent' ||
      t === 'bpmn:IntermediateThrowEvent' ||
      t === 'bpmn:BoundaryEvent'
    ) {
      return getEventDefinition(element) ? 'event' : 'basic';
    }
    return 'basic';
  }
  if (is(element, 'bpmn:UserTask') || is(element, 'bpmn:Task')) return 'node';
  if (is(element, 'bpmn:SequenceFlow')) return 'transition';
  if (
    is(element, 'bpmn:InclusiveGateway') ||
    is(element, 'bpmn:ExclusiveGateway') ||
    is(element, 'bpmn:ParallelGateway')
  )
    return 'gateway';
  if (is(element, 'bpmn:ServiceTask')) return 'service';
  if (is(element, 'bpmn:ScriptTask')) return 'script';
  if (is(element, 'bpmn:BusinessRuleTask')) return 'rule';
  if (is(element, 'bpmn:SubProcess') || is(element, 'bpmn:CallActivity')) return 'subprocess';
  if (
    is(element, 'bpmn:StartEvent') ||
    is(element, 'bpmn:EndEvent') ||
    is(element, 'bpmn:IntermediateCatchEvent') ||
    is(element, 'bpmn:IntermediateThrowEvent') ||
    is(element, 'bpmn:BoundaryEvent')
  ) {
    const ed = getEventDefinition(element);
    return ed ? 'event' : null;
  }
  return null;
}

