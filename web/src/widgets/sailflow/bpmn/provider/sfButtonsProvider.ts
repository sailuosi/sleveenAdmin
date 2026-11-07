import { is } from 'bpmn-js/lib/util/ModelUtil';
import { isTextFieldEntryEdited } from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';
import { useState } from '@bpmn-io/properties-panel/preact/hooks';
import {
  SAIL_BUTTON_CATALOG,
  parseButtonsJson,
  type SailButtonSetting
} from '../buttonsCatalog';
import { ensureChildExtension, getBo } from '../utils/sfExtension';

const EXT = 'sf:ButtonsSetting';
const LOW_PRIORITY = 470;

type Modeling = { updateProperties: (el: unknown, props: object) => void };
type Moddle = { create: (type: string, props?: object) => any };

function readSettings(element: unknown): SailButtonSetting[] {
  const bo = getBo(element);
  const ext = (bo.extensionElements?.values || []).find((e: any) => e.$instanceOf?.(EXT));
  return parseButtonsJson(ext?.json != null ? String(ext.json) : '');
}

function writeSettings(
  element: unknown,
  modeling: Modeling,
  moddle: Moddle,
  next: SailButtonSetting[]
) {
  const bo = getBo(element);
  const ext = ensureChildExtension(bo, EXT, moddle);
  ext.json = JSON.stringify(next);
  return modeling.updateProperties(element, { extensionElements: bo.extensionElements });
}

function catalogName(id: string) {
  return SAIL_BUTTON_CATALOG.find(b => b.id === id)?.displayName || id;
}

/** 对齐 n_flowable 列表：操作按钮 | 显示名称 | 启用 */
function ButtonsSettingList(props: { element: unknown; id: string }) {
  const { element, id } = props;
  const modeling = useService('modeling') as Modeling;
  const moddle = useService('moddle') as Moddle;
  const [editIndex, setEditIndex] = useState<number | null>(null);
  const [, bump] = useState(0);

  const rows = readSettings(element);

  const persist = (next: SailButtonSetting[]) => {
    writeSettings(element, modeling, moddle, next);
    bump(n => n + 1);
  };

  const onToggle = (index: number, enable: boolean) => {
    persist(rows.map((r, i) => (i === index ? { ...r, enable } : r)));
  };

  const onNameChange = (index: number, displayName: string) => {
    persist(
      rows.map((r, i) =>
        i === index ? { ...r, displayName: displayName || catalogName(r.id) } : r
      )
    );
  };

  return createElement(
    'div',
    { class: 'sf-button-setting-pane', 'data-entry-id': id },
    createElement('div', { class: 'sf-button-setting-desc' }, '操作按钮'),
    createElement(
      'div',
      { class: 'sf-button-setting-title' },
      createElement('div', { class: 'sf-button-title-label' }, '操作按钮'),
      createElement('div', { class: 'sf-button-title-label' }, '显示名称'),
      createElement('div', { class: 'sf-button-title-label sf-button-title-enable' }, '启用')
    ),
    ...rows.map((item, index) =>
      createElement(
        'div',
        { class: 'sf-button-setting-item', key: item.id },
        createElement(
          'div',
          { class: 'sf-button-setting-item-label', title: catalogName(item.id) },
          catalogName(item.id)
        ),
        createElement(
          'div',
          { class: 'sf-button-setting-item-label' },
          editIndex === index
            ? createElement('input', {
                type: 'text',
                class: 'sf-editable-title-input',
                value: item.displayName,
                placeholder: catalogName(item.id),
                autofocus: true,
                onInput: (e: Event) =>
                  onNameChange(index, (e.target as HTMLInputElement).value),
                onBlur: () => setEditIndex(null),
                onKeyDown: (e: KeyboardEvent) => {
                  if (e.key === 'Enter') setEditIndex(null);
                }
              })
            : createElement(
                'button',
                {
                  type: 'button',
                  class: 'sf-button-name-btn',
                  onClick: () => setEditIndex(index)
                },
                (item.displayName || catalogName(item.id)) + ' ✎'
              )
        ),
        createElement(
          'div',
          { class: 'sf-button-setting-item-label sf-button-enable-cell' },
          createElement('input', {
            type: 'checkbox',
            class: 'sf-button-enable-switch',
            checked: item.enable !== false,
            onChange: (e: Event) =>
              onToggle(index, (e.target as HTMLInputElement).checked)
          })
        )
      )
    )
  ) as ComponentChildren;
}

function makeButtonsListEntry() {
  return (element: unknown) => ({
    id: 'sf-buttons-setting-list',
    element,
    component: ButtonsSettingList,
    isEdited: isTextFieldEntryEdited
  });
}

function SfButtonsPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask')) {
        groups.push({
          id: 'sf-buttons-setting',
          label: '办理按钮设置',
          entries: [makeButtonsListEntry()(element)]
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfButtonsPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfButtonsPropertiesProviderModule = {
  __init__: ['sfButtonsPropertiesProvider'],
  sfButtonsPropertiesProvider: ['type', SfButtonsPropertiesProvider]
};
