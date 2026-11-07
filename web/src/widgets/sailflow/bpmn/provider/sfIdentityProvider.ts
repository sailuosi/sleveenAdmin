/**
 * T3-2a：无 jQuery 的 Identity 扩展（code / url）
 * 对照 ClientApp identity provider；文案先写死中文（i18n 后置）
 */
import { isTextFieldEntryEdited, TextFieldEntry } from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';

type DiagramElement = {
  businessObject: Record<string, unknown> & { code?: string; url?: string };
};

function textEntry(
  id: string,
  label: string,
  getValue: (el: DiagramElement) => string,
  setValue: (el: DiagramElement, modeling: { updateProperties: (el: unknown, props: object) => void }, value: string) => void
) {
  function Field(props: { element: DiagramElement; id: string }) {
    const { element, id: entryId } = props;
    const modeling = useService('modeling');
    const debounce = useService('debounceInput');

    return createElement(TextFieldEntry as never, {
      id: entryId,
      element,
      label,
      getValue: () => getValue(element),
      setValue: (value: string) => setValue(element, modeling, value),
      debounce
    }) as ComponentChildren;
  }

  return (element: DiagramElement) => ({
    id,
    element,
    component: Field,
    isEdited: isTextFieldEntryEdited
  });
}

const codeEntry = textEntry(
  'sf-code',
  '编码 (sf:code)',
  el => String(el.businessObject.code ?? ''),
  (el, modeling, value) => modeling.updateProperties(el, { code: value })
);

const urlEntry = textEntry(
  'sf-url',
  'URL (sf:url)',
  el => String(el.businessObject.url ?? ''),
  (el, modeling, value) => modeling.updateProperties(el, { url: value })
);

const LOW_PRIORITY = 500;

function SfIdentityPropertiesProvider(this: { getGroups: Function }, propertiesPanel: { registerProvider: Function }) {
  this.getGroups = function (_element: DiagramElement) {
    return function (groups: Array<{ id: string }>) {
      groups.push({
        id: 'sf-identity',
        // @ts-expect-error bpmn-js group shape
        label: 'Sailflow',
        entries: [codeEntry(_element), urlEntry(_element)]
      });
      return groups;
    };
  };

  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfIdentityPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfIdentityPropertiesProviderModule = {
  __init__: ['sfIdentityPropertiesProvider'],
  sfIdentityPropertiesProvider: ['type', SfIdentityPropertiesProvider]
};
