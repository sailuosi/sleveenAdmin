import { ListGroup } from '@bpmn-io/properties-panel';
import { is, getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';
import { without } from 'min-dash';
import { designerEvents } from '../ui/designerEvents';
import {
  createElement as createModdleEl,
  createNotifications,
  getNotifications,
  getNotificationsExtension
} from '../utils/notificationsUtil';

const LOW_PRIORITY = 500;

function notificationsProps(element: unknown, injector: { get: (n: string) => any }) {
  const notifications = getNotifications(element) || [];
  const bpmnFactory = injector.get('bpmnFactory');
  const commandStack = injector.get('commandStack');

  const items = notifications.map((notification: any, index: number) => {
    const id = `${(element as any).id}-notification-${index}`;
    return {
      id,
      label: notification.get?.('name') || notification.name || '',
      autoFocusEntry: `${id}-name`,
      remove: (event: Event) => {
        event.stopPropagation();
        const extension = getNotificationsExtension(element);
        if (!extension) return;
        const next = without(extension.get('values'), notification);
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
      designerEvents.emit('notification:open', {
        element,
        onConfirm: draft => {
          const existing = getNotifications(element) || [];
          if (existing.some((n: any) => n.outerId === draft.outerId)) return;

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

          let extension = getNotificationsExtension(element);
          if (!extension) {
            extension = createNotifications({ values: [] }, extensionElements, bpmnFactory);
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

          const newNotification = createModdleEl(
            'sf:Notification',
            {
              name: draft.name,
              outerId: draft.outerId,
              outerCode: draft.outerCode,
              outerType: draft.outerType || 'User'
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
                values: [...extension.get('values'), newNotification]
              }
            }
          });

          commandStack.execute('properties-panel.multi-command-executor', commands);
        }
      });
    }
  };
}

function SfNotificationsPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask')) {
        groups.push({
          id: 'sf-notifications',
          label: '通知',
          component: ListGroup,
          ...notificationsProps(element, injector)
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfNotificationsPropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfNotificationsPropertiesProviderModule = {
  __init__: ['sfNotificationsPropertiesProvider'],
  sfNotificationsPropertiesProvider: ['type', SfNotificationsPropertiesProvider]
};
