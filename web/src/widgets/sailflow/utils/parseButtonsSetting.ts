import {
  defaultButtonsSetting,
  mergeButtonsSetting,
  type SailButtonSetting
} from '@sail/bpmn/buttonsCatalog';

/**
 * 从流程 XmlContent 解析指定活动的 sf:ButtonsSetting / sf:buttonsSetting。
 * 兼容 attribute json= 与子文本；活动未配置时返回默认码表。
 * 注意：text/xml 下 querySelector 大小写敏感；bpmn-js 常写出 camelCase。
 */
export function parseButtonsSettingForActivity(
  xml: string | null | undefined,
  activityId: string | null | undefined
): SailButtonSetting[] {
  if (!xml || !activityId) return defaultButtonsSetting();

  try {
    const doc = new DOMParser().parseFromString(xml, 'text/xml');
    if (doc.querySelector('parsererror')) {
      return parseButtonsSettingRegex(xml, activityId);
    }

    const nodes = Array.from(
      doc.querySelectorAll('userTask, task, *[id]')
    ) as Element[];
    const activity = nodes.find(n => n.getAttribute('id') === activityId);
    if (!activity) return parseButtonsSettingRegex(xml, activityId);

    const ext = findButtonsSettingEl(activity);
    if (!ext) return parseButtonsSettingRegex(xml, activityId);

    const json =
      ext.getAttribute('json') ||
      ext.getAttributeNS('*', 'json') ||
      ext.textContent?.trim() ||
      '';
    if (!json) return parseButtonsSettingRegex(xml, activityId);
    const arr = JSON.parse(json) as SailButtonSetting[];
    return mergeButtonsSetting(Array.isArray(arr) ? arr : null);
  } catch {
    return parseButtonsSettingRegex(xml, activityId);
  }
}

function findButtonsSettingEl(activity: Element): Element | null {
  const kids = Array.from(activity.getElementsByTagName('*')) as Element[];
  return (
    kids.find(el => {
      const local = el.localName || el.tagName?.split(':').pop() || '';
      return local.toLowerCase() === 'buttonssetting';
    }) || null
  );
}

function parseButtonsSettingRegex(xml: string, activityId: string): SailButtonSetting[] {
  // 粗匹配：activityId 附近的 ButtonsSetting json="..."
  const idEsc = activityId.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const re = new RegExp(
    `id=["']${idEsc}["'][\\s\\S]{0,4000}?ButtonsSetting[^>]*json=["']([^"']+)["']`,
    'i'
  );
  const m = xml.match(re);
  if (!m?.[1]) return defaultButtonsSetting();
  try {
    const decoded = m[1].replace(/&quot;/g, '"').replace(/&amp;/g, '&');
    const arr = JSON.parse(decoded) as SailButtonSetting[];
    return mergeButtonsSetting(Array.isArray(arr) ? arr : null);
  } catch {
    return defaultButtonsSetting();
  }
}
