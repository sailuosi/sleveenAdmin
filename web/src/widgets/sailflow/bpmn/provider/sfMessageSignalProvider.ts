import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { ListGroup, TextFieldEntry, isTextFieldEntryEdited } from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';
import { without } from 'min-dash';

const LOW_PRIORITY = 500;

function getDefinitions(element: unknown): any | null {
  let bo: any = getBusinessObject(element as never);
  while (bo && !bo.rootElements) {
    bo = bo.$parent;
  }
  return bo?.rootElements ? bo : null;
}

function listOfType(definitions: any, type: string) {
  return (definitions.rootElements || []).filter((e: any) => e.$instanceOf?.(type) || e.$type === type);
}

function makeNameEntry(cfg: { id: string; element: unknown; moddleEl: any }) {
  function Field(props: { element: unknown; id: string }) {
    const { id } = props;
    const modeling = useService('modeling') as {
      updateModdleProperties: (el: unknown, m: unknown, p: object) => void;
    };
    const debounce = useService('debounceInput');
    return createElement(TextFieldEntry as never, {
      id,
      element: props.element,
      label: '名称',
      getValue: () => String(cfg.moddleEl.name ?? ''),
      setValue: (v: string) =>
        modeling.updateModdleProperties(cfg.element, cfg.moddleEl, { name: v }),
      debounce
    }) as ComponentChildren;
  }
  return {
    id: cfg.id,
    element: cfg.element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  };
}

function makeListProps(
  element: unknown,
  injector: { get: (n: string) => any },
  type: 'bpmn:Message' | 'bpmn:Signal',
  idPrefix: string
) {
  const bpmnFactory = injector.get('bpmnFactory');
  const commandStack = injector.get('commandStack');
  const definitions = getDefinitions(element);
  if (!definitions) {
    return { items: [], add: () => undefined };
  }

  const items = listOfType(definitions, type).map((el: any, index: number) => {
    const id = `${idPrefix}-${el.id || index}`;
    return {
      id,
      label: el.name || el.id || type,
      entries: [makeNameEntry({ id: `${id}-name`, element, moddleEl: el })],
      remove: (event: Event) => {
        event.stopPropagation();
        commandStack.execute('element.updateModdleProperties', {
          element,
          moddleElement: definitions,
          properties: {
            rootElements: without(definitions.rootElements || [], el)
          }
        });
      }
    };
  });

  return {
    items,
    add: (event: Event) => {
      event.stopPropagation();
      const created = bpmnFactory.create(type, {
        id: `${idPrefix}_${Math.random().toString(36).slice(2, 8)}`,
        name: type === 'bpmn:Message' ? '新消息' : '新信号'
      });
      commandStack.execute('element.updateModdleProperties', {
        element,
        moddleElement: definitions,
        properties: {
          rootElements: [...(definitions.rootElements || []), created]
        }
      });
    }
  };
}

function SfMessageSignalPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!is(element, 'bpmn:Process')) return groups;
      groups.push({
        id: 'sf-messages',
        label: '消息',
        component: ListGroup,
        ...makeListProps(element, injector, 'bpmn:Message', 'Message')
      });
      groups.push({
        id: 'sf-signals',
        label: '信号',
        component: ListGroup,
        ...makeListProps(element, injector, 'bpmn:Signal', 'Signal')
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfMessageSignalPropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfMessageSignalPropertiesProviderModule = {
  __init__: ['sfMessageSignalPropertiesProvider'],
  sfMessageSignalPropertiesProvider: ['type', SfMessageSignalPropertiesProvider]
};
