import { ListGroup } from '@bpmn-io/properties-panel';
import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { without } from 'min-dash';
import { designerEvents } from '../ui/designerEvents';
import {
  createElement as createModdleEl,
  createSubInfoes,
  getSubInfoes,
  getSubInfoesExtension
} from '../utils/subInfoesUtil';

const LOW_PRIORITY = 500;

function subInfoesProps(element: unknown, injector: { get: (n: string) => any }) {
  const subInfoes = getSubInfoes(element) || [];
  const bpmnFactory = injector.get('bpmnFactory');
  const commandStack = injector.get('commandStack');

  const items = subInfoes.map((subInfo: any, index: number) => {
    const id = `${(element as any).id}-subinfo-${index}`;
    return {
      id,
      label: subInfo.get?.('subProcessName') || subInfo.subProcessName || '',
      remove: (event: Event) => {
        event.stopPropagation();
        const extension = getSubInfoesExtension(element);
        if (!extension) return;
        const next = without(extension.get('values'), subInfo);
        commandStack.execute('element.updateModdleProperties', {
          element,
          moddleElement: extension,
          properties: { values: next }
        });
      }
    };
  });

  return {
    items,
    add: (event: Event) => {
      event.stopPropagation();
      designerEvents.emit('subprocess:pick', {
        onConfirm: draft => {
          const existing = getSubInfoes(element) || [];
          if (existing.some((s: any) => String(s.subProcessId) === draft.subProcessId)) return;

          const commands: object[] = [];
          const businessObject = getBusinessObject(element as never);
          let extensionElements = businessObject.get('extensionElements');

          if (!extensionElements) {
            extensionElements = createModdleEl(
              'bpmn:ExtensionElements',
              { values: [] },
              businessObject,
              bpmnFactory
            );
            commands.push({
              cmd: 'element.updateModdleProperties',
              context: {
                element,
                moddleElement: businessObject,
                properties: { extensionElements }
              }
            });
          }

          let extension = getSubInfoesExtension(element);
          if (!extension) {
            extension = createSubInfoes({ values: [] }, extensionElements, bpmnFactory);
            commands.push({
              cmd: 'element.updateModdleProperties',
              context: {
                element,
                moddleElement: extensionElements,
                properties: {
                  values: [...extensionElements.get('values'), extension]
                }
              }
            });
          }

          const newSubInfo = createModdleEl(
            'sf:SubInfo',
            {
              subId: draft.subId,
              subProcessId: draft.subProcessId,
              subProcessName: draft.subProcessName,
              subType: '',
              subVar: ''
            },
            extension,
            bpmnFactory
          );

          commands.push({
            cmd: 'element.updateModdleProperties',
            context: {
              element,
              moddleElement: extension,
              properties: {
                values: [...extension.get('values'), newSubInfo]
              }
            }
          });

          commandStack.execute('properties-panel.multi-command-executor', commands);
        }
      });
    }
  };
}

function SfSubInfoesPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!is(element, 'bpmn:SubProcess')) return groups;
      groups.push({
        id: 'sf-subinfoes',
        label: '子流程',
        component: ListGroup,
        ...subInfoesProps(element, injector)
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfSubInfoesPropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfSubInfoesPropertiesProviderModule = {
  __init__: ['sfSubInfoesPropertiesProvider'],
  sfSubInfoesPropertiesProvider: ['type', SfSubInfoesPropertiesProvider]
};
