/**
 * bpmn-js / properties-panel 中文：覆盖默认 translate 服务。
 * 词条来自 bpmn-js-i18n-zh。
 */
import bpmnZh from 'bpmn-js-i18n-zh/lib/bpmn-js';
import propertiesZh from 'bpmn-js-i18n-zh/lib/properties-panel';

const zhCN: Record<string, string> = {
  ...bpmnZh,
  ...propertiesZh,
  // 标题里常见的元素类型名（面板 header）
  'Start Event': '开始事件',
  'End Event': '结束事件',
  'Intermediate Throw Event': '中间抛出事件',
  'Intermediate Catch Event': '中间捕获事件',
  'Boundary Event': '边界事件',
  'Task': '任务',
  'User Task': '用户任务',
  'Service Task': '服务任务',
  'Script Task': '脚本任务',
  'Business Rule Task': '业务规则任务',
  'Receive Task': '接收任务',
  'Send Task': '发送任务',
  'Manual Task': '手工任务',
  'Call Activity': '调用活动',
  'Sub Process': '子流程',
  'Transaction': '事务',
  'Exclusive Gateway': '排他网关',
  'Parallel Gateway': '并行网关',
  'Inclusive Gateway': '包容网关',
  'Event Based Gateway': '事件网关',
  'Complex Gateway': '复杂网关',
  'Sequence Flow': '顺序流',
  'Message Flow': '消息流',
  'Participant': '参与者',
  'Lane': '泳道',
  'Process': '流程',
  'Collaboration': '协作',
  'Data Object Reference': '数据对象引用',
  'Data Store Reference': '数据存储引用',
  'Group': '分组',
  'Text Annotation': '文本注释',
  ID: 'ID'
};

export function zhTranslate(template: string, replacements?: Record<string, string>): string {
  const dict = replacements || {};
  let text = zhCN[template] || template;

  return text.replace(/{([^}]+)}/g, (_, key: string) => {
    const raw = dict[key];
    if (raw == null) return `{${key}}`;
    return zhCN[raw] || raw;
  });
}

/** bpmn-js additionalModules 条目 */
export const ZhTranslateModule = {
  translate: ['value', zhTranslate]
};
