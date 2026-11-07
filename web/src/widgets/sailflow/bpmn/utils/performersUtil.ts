import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';

export function getPerformers(element: unknown) {
  const performers = getPerformersExtension(element);
  return performers && performers.get('values');
}

export function getPerformersExtension(element: unknown) {
  const businessObject = getBusinessObject(element as never);
  return getExtension(businessObject, 'sf:Performers');
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

export function createPerformers(properties: object, parent: any, bpmnFactory: any) {
  return createElement('sf:Performers', properties, parent, bpmnFactory);
}
