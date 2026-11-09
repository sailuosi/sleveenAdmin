export interface PaletteEntry {
  key: string;
  group: 'tools' | 'create';
  label: string;
  tool?: string;
  type?: string;
}

/** Official-like palette keys for C03-001 (14 visible entries). */
export const PALETTE_ENTRIES: PaletteEntry[] = [
  { key: 'hand-tool', group: 'tools', label: '抓手', tool: 'hand' },
  { key: 'lasso-tool', group: 'tools', label: '套索', tool: 'lasso' },
  { key: 'space-tool', group: 'tools', label: '间距', tool: 'space' },
  { key: 'global-connect-tool', group: 'tools', label: '连线', tool: 'global-connect' },
  { key: 'create.start-event', group: 'create', label: '开始事件', type: 'bpmn:StartEvent' },
  {
    key: 'create.intermediate-event',
    group: 'create',
    label: '中间事件',
    type: 'bpmn:IntermediateThrowEvent',
  },
  { key: 'create.end-event', group: 'create', label: '结束事件', type: 'bpmn:EndEvent' },
  {
    key: 'create.exclusive-gateway',
    group: 'create',
    label: '排他网关',
    type: 'bpmn:ExclusiveGateway',
  },
  { key: 'create.task', group: 'create', label: '任务', type: 'bpmn:Task' },
  {
    key: 'create.subprocess-expanded',
    group: 'create',
    label: '展开子流程',
    type: 'bpmn:SubProcess',
  },
  {
    key: 'create.data-object',
    group: 'create',
    label: '数据对象',
    type: 'bpmn:DataObjectReference',
  },
  {
    key: 'create.data-store',
    group: 'create',
    label: '数据存储',
    type: 'bpmn:DataStoreReference',
  },
  {
    key: 'create.participant-expanded',
    group: 'create',
    label: '池/参与者',
    type: 'bpmn:Participant',
  },
  { key: 'create.group', group: 'create', label: '分组', type: 'bpmn:Group' },
];

export function getPaletteEntries(): PaletteEntry[] {
  return [...PALETTE_ENTRIES];
}

export function paletteEntryByKey(key: string): PaletteEntry | undefined {
  return PALETTE_ENTRIES.find((e) => e.key === key);
}
