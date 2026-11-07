import { ListGroup } from '@bpmn-io/properties-panel';
import { getBusinessObject, is } from 'bpmn-js/lib/util/ModelUtil';
import { without } from 'min-dash';
import { designerEvents } from '../ui/designerEvents';
import { createElement as createModdleEl, createForms, getForms, getFormsExtension } from '../utils/formsUtil';

const LOW_PRIORITY = 500;

function formsProps(element: unknown, injector: { get: (n: string) => any }) {
  const forms = getForms(element) || [];
  const bpmnFactory = injector.get('bpmnFactory');
  const commandStack = injector.get('commandStack');

  const items = forms.map((form: any, index: number) => {
    const id = `${(element as any).id}-form-${index}`;
    return {
      id,
      label: form.get?.('name') || form.name || '',
      autoFocusEntry: `${id}-name`,
      remove: (event: Event) => {
        event.stopPropagation();
        const extension = getFormsExtension(element);
        if (!extension) return;
        commandStack.execute('element.updateModdleProperties', {
          element,
          moddleElement: extension,
          properties: { values: without(extension.get('values'), form) }
        });
      }
    };
  });

  return {
    items,
    add: (event: Event) => {
      event.stopPropagation();
      designerEvents.emit('form:open', {
        element,
        onConfirm: draft => {
          const existing = getForms(element) || [];
          if (existing.some((f: any) => f.outerId === draft.outerId)) return;

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

          let extension = getFormsExtension(element);
          if (!extension) {
            extension = createForms({ values: [] }, extensionElements, bpmnFactory);
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

          const newForm = createModdleEl(
            'sf:Form',
            {
              name: draft.name,
              outerId: draft.outerId,
              outerCode: draft.outerCode
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
                values: [...extension.get('values'), newForm]
              }
            }
          });

          commandStack.execute('properties-panel.multi-command-executor', commands);
        }
      });
    }
  };
}

function SfFormsPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      // Process / UserTask / StartEvent 均可绑 sf:Forms（Host 读扩展；引擎 ConvertForms 仍空）
      if (
        is(element, 'bpmn:Process') ||
        is(element, 'bpmn:UserTask') ||
        is(element, 'bpmn:Task') ||
        is(element, 'bpmn:StartEvent')
      ) {
        groups.splice(1, 0, {
          id: 'sf-forms',
          label: '表单',
          component: ListGroup,
          ...formsProps(element, injector)
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfFormsPropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfFormsPropertiesProviderModule = {
  __init__: ['sfFormsPropertiesProvider'],
  sfFormsPropertiesProvider: ['type', SfFormsPropertiesProvider]
};
