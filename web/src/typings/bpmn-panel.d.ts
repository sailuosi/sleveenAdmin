declare module 'bpmn-js-properties-panel' {
  export const BpmnPropertiesPanelModule: object;
  export const BpmnPropertiesProviderModule: object;
  export function useService<T = any>(name: string): T;
}

declare module '@bpmn-io/properties-panel' {
  export const TextFieldEntry: any;
  export const TextAreaEntry: any;
  export const SelectEntry: any;
  export const CheckboxEntry: any;
  export const ListGroup: any;
  export function isTextFieldEntryEdited(...args: any[]): boolean;
}
