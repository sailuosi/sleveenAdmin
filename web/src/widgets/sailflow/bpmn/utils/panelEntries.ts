import {
  CheckboxEntry,
  isTextFieldEntryEdited,
  SelectEntry,
  TextAreaEntry,
  TextFieldEntry
} from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';
import {
  ensureChildExtension,
  ensureNestedListItem,
  getBo,
  getExtension,
  optionName,
  optionValue,
  type SelectOption
} from './sfExtension';

type Modeling = { updateProperties: (el: unknown, props: object) => void };
type Moddle = { create: (type: string, props?: object) => any };

export function makeSelectOnExtension(cfg: {
  id: string;
  label: string;
  extensionType: string;
  prop: string;
  options: SelectOption[];
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const ext = bo.extensionElements
        ? (bo.extensionElements.values || []).find((e: any) => e.$instanceOf?.(cfg.extensionType))
        : undefined;
      return optionValue(cfg.options, String(ext?.[cfg.prop] ?? ''));
    };

    const setValue = (value: string) => {
      const bo = getBo(element);
      const ext = ensureChildExtension(bo, cfg.extensionType, moddle);
      ext[cfg.prop] = optionName(cfg.options, value);
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    return createElement(SelectEntry as never, {
      id,
      element,
      label: cfg.label,
      getValue,
      setValue,
      getOptions: () => cfg.options,
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

export function makeTextOnExtension(cfg: {
  id: string;
  label: string;
  description?: string;
  extensionType: string;
  prop: string;
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const ext = (bo.extensionElements?.values || []).find((e: any) =>
        e.$instanceOf?.(cfg.extensionType)
      );
      return String(ext?.[cfg.prop] ?? '');
    };

    const setValue = (value: string) => {
      const bo = getBo(element);
      const ext = ensureChildExtension(bo, cfg.extensionType, moddle);
      ext[cfg.prop] = value;
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    return createElement(TextFieldEntry as never, {
      id,
      element,
      label: cfg.label,
      description: cfg.description,
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

export function makeTextAreaOnExtension(cfg: {
  id: string;
  label: string;
  description?: string;
  extensionType: string;
  prop: string;
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const ext = (bo.extensionElements?.values || []).find((e: any) =>
        e.$instanceOf?.(cfg.extensionType)
      );
      return String(ext?.[cfg.prop] ?? '');
    };

    const setValue = (value: string) => {
      const bo = getBo(element);
      const ext = ensureChildExtension(bo, cfg.extensionType, moddle);
      ext[cfg.prop] = value;
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    return createElement(TextAreaEntry as never, {
      id,
      element,
      label: cfg.label,
      description: cfg.description,
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

export function makeCheckboxOnExtension(cfg: {
  id: string;
  label: string;
  description?: string;
  extensionType: string;
  prop: string;
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const ext = (bo.extensionElements?.values || []).find((e: any) =>
        e.$instanceOf?.(cfg.extensionType)
      );
      const v = ext?.[cfg.prop];
      return v === true || v === 'true';
    };

    const setValue = (value: boolean) => {
      const bo = getBo(element);
      const ext = ensureChildExtension(bo, cfg.extensionType, moddle);
      ext[cfg.prop] = String(value);
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    return createElement(CheckboxEntry as never, {
      id,
      element,
      label: cfg.label,
      description: cfg.description,
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

/** 嵌套列表首项上的 Select（sf:Scripts/scripts/sf:Script 等） */
export function makeSelectOnNested(cfg: {
  id: string;
  label: string;
  collectionType: string;
  listProp: string;
  itemType: string;
  prop: string;
  options: SelectOption[];
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const collection = getExtension(bo, cfg.collectionType);
      const list =
        (collection?.get?.(cfg.listProp) as unknown[] | undefined) ??
        (collection?.[cfg.listProp] as unknown[] | undefined);
      const item = Array.isArray(list) ? (list[0] as Record<string, unknown> | undefined) : undefined;
      return optionValue(cfg.options, String(item?.[cfg.prop] ?? ''));
    };

    const setValue = (value: string) => {
      const bo = getBo(element);
      const item = ensureNestedListItem(bo, cfg.collectionType, cfg.listProp, cfg.itemType, moddle);
      item[cfg.prop] = optionName(cfg.options, value);
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    return createElement(SelectEntry as never, {
      id,
      element,
      label: cfg.label,
      getValue,
      setValue,
      getOptions: () => cfg.options,
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

export function makeTextOnNested(cfg: {
  id: string;
  label: string;
  description?: string;
  collectionType: string;
  listProp: string;
  itemType: string;
  prop: string;
  rows?: number;
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const moddle = useService('moddle') as Moddle;
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element);
      const collection = getExtension(bo, cfg.collectionType);
      const list =
        (collection?.get?.(cfg.listProp) as unknown[] | undefined) ??
        (collection?.[cfg.listProp] as unknown[] | undefined);
      const item = Array.isArray(list) ? (list[0] as Record<string, unknown> | undefined) : undefined;
      return String(item?.[cfg.prop] ?? '');
    };

    const setValue = (value: string) => {
      const bo = getBo(element);
      const item = ensureNestedListItem(bo, cfg.collectionType, cfg.listProp, cfg.itemType, moddle);
      item[cfg.prop] = value;
      return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
    };

    const Entry = cfg.rows && cfg.rows > 1 ? TextAreaEntry : TextFieldEntry;
    return createElement(Entry as never, {
      id,
      element,
      label: cfg.label,
      description: cfg.description,
      getValue,
      setValue,
      debounce,
      ...(cfg.rows && cfg.rows > 1 ? { rows: cfg.rows } : {})
    }) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: cfg.id,
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}

export function makeConditionExpressionEntry() {
  function Field(props: { element: unknown; id: string }) {
    const { element, id } = props;
    const modeling = useService('modeling') as Modeling;
    const bpmnFactory = useService('bpmnFactory') as { create: (t: string, p?: object) => any };
    const debounce = useService('debounceInput');

    const getValue = () => {
      const bo = getBo(element) as any;
      return bo.conditionExpression?.body ?? '';
    };

    const setValue = (value: string) => {
      const expression = bpmnFactory.create('bpmn:Expression', { body: value });
      return modeling.updateProperties(element, { conditionExpression: expression });
    };

    const openEditor = (event: Event) => {
      event.preventDefault();
      event.stopPropagation();
      // 延迟 import 避免 panelEntries ↔ designerEvents 循环依赖时初始化问题
      import('../ui/designerEvents').then(({ designerEvents }) => {
        designerEvents.emit('expression:open', {
          value: getValue() || '',
          onConfirm: (expr: string) => setValue(expr)
        });
      });
    };

    return createElement(
      'div',
      { class: 'sf-condition-entry' },
      createElement(TextAreaEntry as never, {
        id,
        element,
        label: '条件表达式',
        getValue,
        setValue,
        debounce,
        rows: 3
      }),
      createElement(
        'button',
        {
          type: 'button',
          class: 'sf-condition-entry__btn',
          onClick: openEditor
        },
        '可视化编辑'
      )
    ) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: 'sf-condition',
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}
