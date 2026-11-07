import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import {
  SelectEntry,
  TextFieldEntry,
  isSelectEntryEdited,
  isTextFieldEntryEdited
} from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';

const LOW_PRIORITY = 500;

function randomId(prefix: string) {
  return `${prefix}_${Math.random().toString(36).slice(2, 8)}`;
}

function getEventDefinition(element: unknown): any | null {
  const bo = getBusinessObject(element as never) as any;
  return bo?.eventDefinitions?.[0] || null;
}

type TimerKind = 'timeDate' | 'timeDuration' | 'timeCycle';

function readTimerKind(ed: any): TimerKind {
  if (ed?.timeDate) return 'timeDate';
  if (ed?.timeCycle) return 'timeCycle';
  return 'timeDuration';
}

function readTimerBody(ed: any): string {
  const kind = readTimerKind(ed);
  const expr = ed?.get?.(kind) || ed?.[kind];
  return expr?.body ?? '';
}

function makeTimerKindEntry() {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const bpmnFactory = useService('bpmnFactory') as { create: (t: string, p?: object) => any };
    const modeling = useService('modeling') as {
      updateModdleProperties: (el: unknown, moddle: unknown, props: object) => void;
    };
    const debounce = useService('debounceInput');

    const getValue = () => {
      const ed = getEventDefinition(element);
      if (!ed || ed.$type !== 'bpmn:TimerEventDefinition') return 'timeDuration';
      return readTimerKind(ed);
    };

    const setValue = (value: TimerKind) => {
      const ed = getEventDefinition(element);
      if (!ed) return;
      const prev = readTimerBody(ed);
      const formal = bpmnFactory.create('bpmn:FormalExpression', { body: prev || '' });
      modeling.updateModdleProperties(element, ed, {
        timeDate: value === 'timeDate' ? formal : undefined,
        timeDuration: value === 'timeDuration' ? formal : undefined,
        timeCycle: value === 'timeCycle' ? formal : undefined
      });
    };

    return createElement(SelectEntry as never, {
      id,
      element,
      label: '定时类型',
      getValue,
      setValue,
      getOptions: () => [
        { label: '时间点 (timeDate)', value: 'timeDate' },
        { label: '持续 (timeDuration)', value: 'timeDuration' },
        { label: '循环 (timeCycle)', value: 'timeCycle' }
      ],
      debounce
    }) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: 'sf-trigger-timer-kind',
    element,
    component: Field,
    isEdited: isSelectEntryEdited
  });
}

function makeTimerBodyEntry() {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const bpmnFactory = useService('bpmnFactory') as { create: (t: string, p?: object) => any };
    const modeling = useService('modeling') as {
      updateModdleProperties: (el: unknown, moddle: unknown, props: object) => void;
    };
    const debounce = useService('debounceInput');

    const getValue = () => {
      const ed = getEventDefinition(element);
      if (!ed) return '';
      return readTimerBody(ed);
    };

    const setValue = (value: string) => {
      const ed = getEventDefinition(element);
      if (!ed) return;
      const kind = readTimerKind(ed);
      const formal = bpmnFactory.create('bpmn:FormalExpression', { body: value });
      modeling.updateModdleProperties(element, ed, {
        timeDate: kind === 'timeDate' ? formal : undefined,
        timeDuration: kind === 'timeDuration' ? formal : undefined,
        timeCycle: kind === 'timeCycle' ? formal : undefined
      });
    };

    return createElement(TextFieldEntry as never, {
      id,
      element,
      label: '定时表达式',
      description: 'timeDate: ISO 日期；timeDuration: PT10M；timeCycle: R3/PT10M',
      getValue,
      setValue,
      debounce
    }) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: 'sf-trigger-timer-body',
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}

function makeFormalExprEntry(cfg: {
  id: string;
  label: string;
  prop: 'condition';
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const bpmnFactory = useService('bpmnFactory') as { create: (t: string, p?: object) => any };
    const debounce = useService('debounceInput');

    const getValue = () => {
      const ed = getEventDefinition(element);
      const expr = ed?.get?.(cfg.prop) || ed?.[cfg.prop];
      return expr?.body ?? '';
    };

    const setValue = (value: string) => {
      const ed = getEventDefinition(element);
      if (!ed) return;
      const formalExpression = bpmnFactory.create('bpmn:FormalExpression', { body: value });
      ed.set?.(cfg.prop, formalExpression);
    };

    return createElement(TextFieldEntry as never, {
      id,
      element,
      label: cfg.label,
      getValue,
      setValue,
      debounce
    }) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: cfg.id,
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}

function makeRootRefEntry(cfg: {
  id: string;
  label: string;
  refProp: 'messageRef' | 'signalRef';
  createType: 'bpmn:Message' | 'bpmn:Signal';
  idPrefix: string;
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const bpmnFactory = useService('bpmnFactory') as { create: (t: string, p?: object) => any };
    const debounce = useService('debounceInput');

    const getValue = () => {
      const ed = getEventDefinition(element);
      const ref = ed?.[cfg.refProp];
      return ref?.name ?? '';
    };

    const setValue = (value: string) => {
      const bo = getBusinessObject(element as never) as any;
      const ed = getEventDefinition(element);
      if (!ed) return;
      const definitions = bo.$parent?.$parent;
      if (!definitions?.rootElements) return;

      let ref = ed[cfg.refProp];
      if (ref) {
        const found = definitions.rootElements.find((r: any) => r.id === ref.id);
        if (found) found.name = value;
        else ref.name = value;
      } else {
        ref = bpmnFactory.create(cfg.createType, {
          id: randomId(cfg.idPrefix),
          name: value
        });
        definitions.rootElements.push(ref);
        ed[cfg.refProp] = ref;
      }
    };

    return createElement(TextFieldEntry as never, {
      id,
      element,
      label: cfg.label,
      getValue,
      setValue,
      debounce
    }) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: cfg.id,
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}

const timerKindEntry = makeTimerKindEntry();
const timerBodyEntry = makeTimerBodyEntry();

const conditionEntry = makeFormalExprEntry({
  id: 'sf-trigger-condition',
  label: '条件表达式',
  prop: 'condition'
});

const messageEntry = makeRootRefEntry({
  id: 'sf-trigger-message',
  label: '消息名',
  refProp: 'messageRef',
  createType: 'bpmn:Message',
  idPrefix: 'Message'
});

const signalEntry = makeRootRefEntry({
  id: 'sf-trigger-signal',
  label: '信号名',
  refProp: 'signalRef',
  createType: 'bpmn:Signal',
  idPrefix: 'Signal'
});

function isTriggerHost(element: unknown) {
  return (
    is(element, 'bpmn:StartEvent') ||
    is(element, 'bpmn:IntermediateCatchEvent') ||
    is(element, 'bpmn:IntermediateThrowEvent') ||
    is(element, 'bpmn:EndEvent')
  );
}

function SfTriggerPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!isTriggerHost(element)) return groups;
      const ed = getEventDefinition(element);
      if (!ed) return groups;

      const entries: object[] = [];
      if (ed.$type === 'bpmn:ConditionalEventDefinition') entries.push(conditionEntry(element));
      else if (ed.$type === 'bpmn:TimerEventDefinition') {
        entries.push(timerKindEntry(element), timerBodyEntry(element));
      } else if (ed.$type === 'bpmn:MessageEventDefinition') entries.push(messageEntry(element));
      else if (ed.$type === 'bpmn:SignalEventDefinition') entries.push(signalEntry(element));

      if (!entries.length) return groups;
      groups.push({ id: 'sf-trigger', label: '触发', entries });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfTriggerPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfTriggerPropertiesProviderModule = {
  __init__: ['sfTriggerPropertiesProvider'],
  sfTriggerPropertiesProvider: ['type', SfTriggerPropertiesProvider]
};
