import { ListGroup } from '@bpmn-io/properties-panel';
import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { without } from 'min-dash';
import { designerEvents } from '../ui/designerEvents';
import {
  createElement as createModdleEl,
  createPerformers,
  getPerformers,
  getPerformersExtension
} from '../utils/performersUtil';

const LOW_PRIORITY = 500;

function performersProps(element: unknown, injector: { get: (n: string) => any }) {
  const performers = getPerformers(element) || [];
  const bpmnFactory = injector.get('bpmnFactory');
  const commandStack = injector.get('commandStack');

  const items = performers.map((performer: any, index: number) => {
    const id = `${(element as any).id}-performer-${index}`;
    return {
      id,
      label: performer.get?.('name') || performer.name || '',
      autoFocusEntry: `${id}-name`,
      remove: (event: Event) => {
        event.stopPropagation();
        const extension = getPerformersExtension(element);
        if (!extension) return;
        const next = without(extension.get('values'), performer);
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
      designerEvents.emit('performer:open', {
        element,
        onConfirm: draft => {
          const existing = getPerformers(element) || [];
          if (existing.some((p: any) => p.outerId === draft.outerId)) return;

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

          let extension = getPerformersExtension(element);
          if (!extension) {
            extension = createPerformers({ values: [] }, extensionElements, bpmnFactory);
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

          const newPerformer = createModdleEl(
            'sf:Performer',
            {
              name: draft.name,
              outerId: draft.outerId,
              outerCode: draft.outerCode,
              outerType: draft.outerType
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
                values: [...extension.get('values'), newPerformer]
              }
            }
          });

          commandStack.execute('properties-panel.multi-command-executor', commands);
        }
      });
    }
  };
}

function SfPerformersPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask')) {
        groups.push({
          id: 'sf-performers',
          label: '参与者',
          component: ListGroup,
          ...performersProps(element, injector)
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfPerformersPropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfPerformersPropertiesProviderModule = {
  __init__: ['sfPerformersPropertiesProvider'],
  sfPerformersPropertiesProvider: ['type', SfPerformersPropertiesProvider]
};
