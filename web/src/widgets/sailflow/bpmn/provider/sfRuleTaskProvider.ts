import { ListGroup } from '@bpmn-io/properties-panel';
import { is } from 'bpmn-js/lib/util/ModelUtil';
import { designerEvents } from '../ui/designerEvents';
import { ensureNestedListItem, getBo, getExtension } from '../utils/sfExtension';

const LOW_PRIORITY = 500;

function getRuleSetCode(element: unknown): string {
  const bo = getBo(element);
  const collection = getExtension(bo, 'sf:RuleConfigs');
  const list =
    (collection?.get?.('ruleConfigs') as any[] | undefined) ??
    (collection?.ruleConfigs as any[] | undefined);
  return String(list?.[0]?.ruleSetCode ?? '');
}

function ruleConfigProps(element: unknown, injector: { get: (n: string) => any }) {
  const code = getRuleSetCode(element);
  const modeling = injector.get('modeling');
  const moddle = injector.get('moddle');
  const commandStack = injector.get('commandStack');

  return {
    items: [
      {
        id: `${(element as any).id}-ruleconfig`,
        label: code ? `RuleSet: ${code}` : 'RuleSet: (未设置)',
        remove: (event: Event) => {
          event.stopPropagation();
          const bo = getBo(element) as any;
          const ee = bo.extensionElements;
          if (!ee) return;
          const values = (ee.get?.('values') || ee.values || []).filter(
            (v: any) => !v.$instanceOf?.('sf:RuleConfigs')
          );
          commandStack.execute('element.updateModdleProperties', {
            element,
            moddleElement: ee,
            properties: { values }
          });
        }
      }
    ],
    add: (event: Event) => {
      event.stopPropagation();
      designerEvents.emit('ruleset:pick', {
        currentCode: getRuleSetCode(element),
        onConfirm: (ruleSetCode: string) => {
          const bo = getBo(element);
          const item = ensureNestedListItem(
            bo,
            'sf:RuleConfigs',
            'ruleConfigs',
            'sf:RuleConfig',
            moddle
          );
          item.ruleSetCode = ruleSetCode;
          if (Object.prototype.hasOwnProperty.call(item, 'bindingsJson')) {
            delete (item as any).bindingsJson;
          }
          modeling.updateProperties(element, { extensionElements: bo.extensionElements });
        }
      });
    }
  };
}

function SfRuleTaskPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function },
  injector: { get: (n: string) => any }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (!is(element, 'bpmn:BusinessRuleTask')) return groups;
      groups.push({
        id: 'sf-ruletask',
        label: '规则集',
        component: ListGroup,
        ...ruleConfigProps(element, injector)
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfRuleTaskPropertiesProvider.$inject = ['propertiesPanel', 'injector', 'translate'];

export const SfRuleTaskPropertiesProviderModule = {
  __init__: ['sfRuleTaskPropertiesProvider'],
  sfRuleTaskPropertiesProvider: ['type', SfRuleTaskPropertiesProvider]
};
