import {
  defaultButtonsSetting,
  mergeButtonsSetting,
  type SailButtonSetting
} from '@sail/bpmn/buttonsCatalog';

/**
 * 从流程 XmlContent 解析指定活动的 sf:ButtonsSetting。
 * 兼容 attribute json= 与子文本；活动未配置时返回默认码表。
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
    if (!activity) return defaultButtonsSetting();

    const ext =
      activity.querySelector('ButtonsSetting') ||
      activity.getElementsByTagNameNS('*', 'ButtonsSetting')[0] ||
      null;
    if (!ext) return defaultButtonsSetting();

    const json =
      ext.getAttribute('json') ||
      ext.getAttributeNS('*', 'json') ||
      ext.textContent?.trim() ||
      '';
    if (!json) return defaultButtonsSetting();
    const arr = JSON.parse(json) as SailButtonSetting[];
    return mergeButtonsSetting(Array.isArray(arr) ? arr : null);
  } catch {
    return parseButtonsSettingRegex(xml, activityId);
  }
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
