import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';

export function getNotifications(element: unknown) {
  const notifications = getNotificationsExtension(element);
  return notifications && notifications.get('values');
}

export function getNotificationsExtension(element: unknown) {
  const businessObject = getBusinessObject(element as never);
  return getExtension(businessObject, 'sf:Notifications');
}

export function getExtension(element: any, type: string) {
  if (!element.extensionElements) return null;
  return element.extensionElements.values.filter((e: any) => e.$instanceOf(type))[0];
}

export function createElement(elementType: string, properties: object, parent: any, factory: any) {
  const element = factory.create(elementType, properties);
  if (parent) element.$parent = parent;
  return element;
}

export function createNotifications(properties: object, parent: any, bpmnFactory: any) {
  return createElement('sf:Notifications', properties, parent, bpmnFactory);
}
