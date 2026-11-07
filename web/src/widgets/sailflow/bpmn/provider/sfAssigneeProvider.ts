import { is } from 'bpmn-js/lib/util/ModelUtil';
import { createElement, type ComponentChildren } from 'preact';
import { useService } from 'bpmn-js-properties-panel';
import {
  makeSelectOnExtension,
  makeTextAreaOnExtension,
  makeTextOnExtension
} from '../utils/panelEntries';
import { ensureChildExtension, getBo, getExtension } from '../utils/sfExtension';
import { LEAVE_TYPE_OPTIONS, RESOLVE_TYPE_OPTIONS } from '../assignee/types';

const resolveTypeOptions = [
  { label: '（清空）', value: '', name: '' },
  ...RESOLVE_TYPE_OPTIONS.map(o => ({ ...o, name: o.value }))
];

const leaveTypeOptions = [
  { label: '（清空）', value: '', name: '' },
  ...LEAVE_TYPE_OPTIONS.map(o => ({ ...o, name: o.value }))
];

const resolveTypeQuick = makeSelectOnExtension({
  id: 'sf-assignee-resolve-type-quick',
  label: '定范围 Type（快捷）',
  extensionType: 'sf:AssigneeResolve',
  prop: 'type',
  options: resolveTypeOptions
});

const resolveTypeEntry = makeTextOnExtension({
  id: 'sf-assignee-resolve-type',
  label: '定范围 Type（string）',
  description: '内置或业务注册码；后注册可覆盖同名 Type',
  extensionType: 'sf:AssigneeResolve',
  prop: 'type'
});

const resolveParamsEntry = makeTextAreaOnExtension({
  id: 'sf-assignee-resolve-params',
  label: '定范围 Params (JSON)',
  description: '建议用下方「结构化编辑」；高级可手改 JSON',
  extensionType: 'sf:AssigneeResolve',
  prop: 'params'
});

const leaveTypeQuick = makeSelectOnExtension({
  id: 'sf-leave-judge-type-quick',
  label: '办理模式 Type（快捷）',
  extensionType: 'sf:LeaveJudge',
  prop: 'type',
  options: leaveTypeOptions
});

const leaveTypeEntry = makeTextOnExtension({
  id: 'sf-leave-judge-type',
  label: '办理模式 Type（string）',
  description: 'Race / Collaborate / GroupRace / GroupCollaborate / Captain / 业务码',
  extensionType: 'sf:LeaveJudge',
  prop: 'type'
});

const leaveParamsEntry = makeTextAreaOnExtension({
  id: 'sf-leave-judge-params',
  label: '办理模式 Params (JSON)',
  description: '建议用下方「结构化编辑」',
  extensionType: 'sf:LeaveJudge',
  prop: 'params'
});

function makeParamsEditorButton(cfg: {
  id: string;
  kind: 'resolve' | 'leave';
  extensionType: string;
}) {
  function Field(props: { element: unknown; id: string }) {
    const { element } = props;
    const modeling = useService('modeling') as {
      updateProperties: (el: unknown, props: object) => void;
    };
    const moddle = useService('moddle') as { create: (t: string, p?: object) => any };

    const open = (event: Event) => {
      event.preventDefault();
      event.stopPropagation();
      const bo = getBo(element);
      const ext = getExtension(bo, cfg.extensionType);
      const type = String(ext?.type ?? '');
      const params = String(ext?.params ?? '');
      import('../ui/designerEvents').then(({ designerEvents }) => {
        designerEvents.emit('assignee-params:open', {
          kind: cfg.kind,
          type,
          params,
          onConfirm: (paramsJson: string) => {
            const e = ensureChildExtension(bo, cfg.extensionType, moddle);
            e.params = paramsJson;
            modeling.updateProperties(element, { extensionElements: bo.extensionElements });
          }
        });
      });
    };

    return createElement(
      'button',
      {
        type: 'button',
        class: 'sf-assignee-params-btn',
        onClick: open
      },
      '结构化编辑 Params…'
    ) as ComponentChildren;
  }

  return (element: unknown) => ({
    id: cfg.id,
    element,
    component: Field
  });
}

const resolveParamsBtn = makeParamsEditorButton({
  id: 'sf-assignee-resolve-params-btn',
  kind: 'resolve',
  extensionType: 'sf:AssigneeResolve'
});

const leaveParamsBtn = makeParamsEditorButton({
  id: 'sf-leave-judge-params-btn',
  kind: 'leave',
  extensionType: 'sf:LeaveJudge'
});

const LOW_PRIORITY = 480;

function SfAssigneePropertiesProvider(
  this: { getGroups: Function },
  propertiesPanel: { registerProvider: Function }
) {
  this.getGroups = function (element: unknown) {
    return function (groups: object[]) {
      if (is(element, 'bpmn:Task') || is(element, 'bpmn:UserTask')) {
        groups.push({
          id: 'sf-assignee-resolve',
          label: '待办人 · 定范围',
          entries: [
            resolveTypeQuick(element),
            resolveTypeEntry(element),
            resolveParamsEntry(element),
            resolveParamsBtn(element)
          ]
        });
        groups.push({
          id: 'sf-leave-judge',
          label: '待办人 · 办理模式',
          entries: [
            leaveTypeQuick(element),
            leaveTypeEntry(element),
            leaveParamsEntry(element),
            leaveParamsBtn(element)
          ]
        });
      }
      return groups;
    };
  };
  propertiesPanel.registerProvider(LOW_PRIORITY, this);
}

SfAssigneePropertiesProvider.$inject = ['propertiesPanel', 'translate'];

export const SfAssigneePropertiesProviderModule = {
  __init__: ['sfAssigneePropertiesProvider'],
  sfAssigneePropertiesProvider: ['type', SfAssigneePropertiesProvider]
};
