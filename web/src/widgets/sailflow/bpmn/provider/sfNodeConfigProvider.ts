import { is } from 'bpmn-js/lib/util/ModelUtil';
import { isTextFieldEntryEdited } from '@bpmn-io/properties-panel';
import { useService } from 'bpmn-js-properties-panel';
import { createElement, type ComponentChildren } from 'preact';
import { designerEvents } from '../ui/designerEvents';

const LOW_PRIORITY = 450;

type Modeling = { updateProperties: (el: unknown, props: object) => void };
type Moddle = { create: (type: string, props?: object) => any };

function OpenNodeConfigEntry(props: { element: unknown; id: string }) {
  const { element, id } = props;
  const modeling = useService('modeling') as Modeling;
  const moddle = useService('moddle') as Moddle;

  const open = (event: Event) => {
    event.preventDefault();
    event.stopPropagation();
    designerEvents.emit('nodeconfig:open', { element, modeling, moddle });
  };

  return createElement(
    'div',
    { class: 'sf-node-config-entry', 'data-entry-id': id },
    createElement(
      'button',
      {
        type: 'button',
        class: 'sf-node-config-entry__btn',
        onClick: open
      },
      '打开节点配置…'
    ),
    createElement(
      'div',
      { class: 'sf-node-config-entry__hint' },
      '待办人、办理模式、按钮、表单、策略、会签、通知、动作等'
    )
  ) as ComponentChildren;
}

function SfNodeConfigPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask')) {
        groups.unshift({
          id: 'sf-node-config-open',
          label: '节点配置',
          entries: [
            {
              id: 'sf-node-config-open-btn',
              element,
              component: OpenNodeConfigEntry,
              isEdited: isTextFieldEntryEdited
            }
          ]
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfNodeConfigPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfNodeConfigPropertiesProviderModule = {
  __init__: ['sfNodeConfigPropertiesProvider'],
  sfNodeConfigPropertiesProvider: ['type', SfNodeConfigPropertiesProvider]
};
