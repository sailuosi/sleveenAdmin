import type { Command, CommandContext, DiagramElement, DiagramSnapshot } from '../types.js';

export interface ReplaceElementPayload {
  id: string;
  target: string;
}

function parseTarget(target: string): {
  type: string;
  eventDefinition?: string;
  flowKind?: 'normal' | 'default' | 'conditional';
} {
  if (target === 'bpmn:SequenceFlow.conditional') {
    return { type: 'bpmn:SequenceFlow', flowKind: 'conditional' };
  }
  if (target === 'bpmn:SequenceFlow.default') {
    return { type: 'bpmn:SequenceFlow', flowKind: 'default' };
  }
  if (target === 'bpmn:SequenceFlow') {
    return { type: 'bpmn:SequenceFlow', flowKind: 'normal' };
  }
  const m = /^(bpmn:\w+)\.(\w+)$/.exec(target);
  if (m) {
    const defMap: Record<string, string> = {
      message: 'bpmn:MessageEventDefinition',
      timer: 'bpmn:TimerEventDefinition',
      signal: 'bpmn:SignalEventDefinition',
      conditional: 'bpmn:ConditionalEventDefinition',
      error: 'bpmn:ErrorEventDefinition',
      terminate: 'bpmn:TerminateEventDefinition',
      escalation: 'bpmn:EscalationEventDefinition',
      compensate: 'bpmn:CompensateEventDefinition',
      link: 'bpmn:LinkEventDefinition',
      cancel: 'bpmn:CancelEventDefinition',
    };
    return { type: m[1]!, eventDefinition: defMap[m[2]!] };
  }
  return { type: target };
}

function applyReplace(el: DiagramElement, target: string): DiagramElement {
  const parsed = parseTarget(target);
  const businessObject = { ...el.businessObject };

  if (parsed.flowKind === 'conditional') {
    businessObject.conditionExpression = businessObject.conditionExpression ?? '${true}';
    delete businessObject.isDefault;
  } else if (parsed.flowKind === 'default') {
    businessObject.isDefault = true;
    delete businessObject.conditionExpression;
  } else if (parsed.flowKind === 'normal') {
    delete businessObject.conditionExpression;
    delete businessObject.isDefault;
  }

  if (parsed.eventDefinition) {
    businessObject.eventDefinitions = [parsed.eventDefinition];
  } else if (parsed.type.includes('Event') && !target.includes('.')) {
    businessObject.eventDefinitions = [];
  }

  return {
    ...el,
    type: parsed.type,
    businessObject,
  };
}

export function buildReplace(
  state: DiagramSnapshot,
  payload: ReplaceElementPayload,
): DiagramSnapshot | null {
  const el = state.elements[payload.id];
  if (!el) return null;
  const next = applyReplace(el, payload.target);
  return {
    ...state,
    elements: {
      ...state.elements,
      [payload.id]: next,
    },
    selection: [payload.id],
  };
}

export function createReplaceElementCommand(payload: ReplaceElementPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'element.replace',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        after = buildReplace(before, payload);
      }
      if (after) ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
