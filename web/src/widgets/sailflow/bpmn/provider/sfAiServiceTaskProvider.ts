import { is } from 'bpmn-js/lib/util/ModelUtil';
import { createElement, type ComponentChildren } from 'preact';
import { makeSelectOnNested, makeTextOnNested } from '../utils/panelEntries';
import { getBo, getExtension } from '../utils/sfExtension';

const LOW_PRIORITY = 500;

function makeAxConfigOpenEntry() {
  function Field(props: { element: { id?: string }; id: string }) {
    const { element } = props;
    const open = (event: Event) => {
      event.preventDefault();
      event.stopPropagation();
      import('../ui/designerEvents').then(({ designerEvents }) => {
        designerEvents.emit('axconfig:open', {
          activityId: String(element.id || ''),
          serviceType: getAiType(element) || 'LLM'
        });
      });
    };
    return createElement(
      'div',
      { class: 'sf-axconfig-entry' },
      createElement(
        'button',
        { type: 'button', class: 'sf-condition-entry__btn', onClick: open },
        '编辑活动配置 (AxConfig)'
      )
    ) as ComponentChildren;
  }
  return (element: unknown) => ({
    id: 'sf-axconfig-open',
    element,
    component: Field
  });
}

const axConfigOpenEntry = makeAxConfigOpenEntry();

const typeEntry = makeSelectOnNested({
  id: 'sf-ai-type',
  label: 'AI 类型',
  collectionType: 'sf:AiServices',
  listProp: 'aiServices',
  itemType: 'sf:AiService',
  prop: 'type',
  options: [
    { label: 'LLM', value: '1', name: 'LLM' },
    { label: 'RAG', value: '2', name: 'RAG' },
    { label: 'Agent', value: '3', name: 'Agent' }
  ]
});

const configUuidEntry = makeTextOnNested({
  id: 'sf-ai-config-uuid',
  label: 'Config UUID',
  description: '对应 ai_activity_config / 模型配置',
  collectionType: 'sf:AiServices',
  listProp: 'aiServices',
  itemType: 'sf:AiService',
  prop: 'configUUID'
});

const toolSetEntry = makeTextOnNested({
  id: 'sf-ai-toolset',
  label: 'ToolSet 名称',
  collectionType: 'sf:AiServices',
  listProp: 'aiServices',
  itemType: 'sf:AiService',
  prop: 'toolSetName'
});

function getAiType(element: unknown): string {
  const bo = getBo(element) as any;
  const sfType = (element as any)?.sfType || bo?.sfType;
  if (sfType) return String(sfType);
  const collection = getExtension(bo, 'sf:AiServices');
  const list =
    (collection?.get?.('aiServices') as any[] | undefined) ??
    (collection?.aiServices as any[] | undefined);
  return list?.[0]?.type ? String(list[0].type) : '';
}

function SfAiServiceTaskPropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      // 对所有 ServiceTask 暴露 AI 组，便于把普通服务任务标成 LLM/RAG/Agent
      if (!is(element, 'bpmn:ServiceTask')) return groups;
      const entries: object[] = [
        axConfigOpenEntry(element),
        typeEntry(element),
        configUuidEntry(element)
      ];
      if (getAiType(element) === 'Agent') entries.push(toolSetEntry(element));
      groups.push({
        id: 'sf-aiservicetask',
        label: 'AI 服务',
        entries
      });
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfAiServiceTaskPropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfAiServiceTaskPropertiesProviderModule = {
  __init__: ['sfAiServiceTaskPropertiesProvider'],
  sfAiServiceTaskPropertiesProvider: ['type', SfAiServiceTaskPropertiesProvider]
};
