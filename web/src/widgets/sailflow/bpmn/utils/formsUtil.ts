import { getBusinessObject } from 'bpmn-js/lib/util/ModelUtil';

export function getForms(element: unknown) {
  const forms = getFormsExtension(element);
  return forms && forms.get('values');
}

export function getFormsExtension(element: unknown) {
  const businessObject = getBusinessObject(element as never);
  return getExtension(businessObject, 'sf:Forms');
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

export function createForms(properties: object, parent: any, bpmnFactory: any) {
  return createElement('sf:Forms', properties, parent, bpmnFactory);
}
