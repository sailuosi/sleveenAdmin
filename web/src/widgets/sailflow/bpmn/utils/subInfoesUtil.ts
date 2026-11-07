import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';

export function getSubInfoes(element: unknown) {
  const ext = getSubInfoesExtension(element);
  return ext && ext.get('values');
}

export function getSubInfoesExtension(element: unknown) {
  const businessObject = getBusinessObject(element as never);
  return getExtension(businessObject, 'sf:SubInfoes');
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

export function createSubInfoes(properties: object, parent: any, bpmnFactory: any) {
  return createElement('sf:SubInfoes', properties, parent, bpmnFactory);
}
