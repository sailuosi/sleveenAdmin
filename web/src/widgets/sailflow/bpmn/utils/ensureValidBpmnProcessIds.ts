/**
 * BPMN/XML NCName：首字符须为字母或 `_`。
 * 新建流程若把 Guid（数字开头）直接写入 process@id，bpmn-js 会报
 * 「no process or collaboration to display」。
 * 打开已有脏数据时，把非法 process id（及平面 bpmnElement）改成合法值。
 */
export function isValidBpmnNcName(id: string): boolean {
  if (!id) return false;
  return /^[A-Za-z_][\w.-]*$/.test(id);
}

export function toBpmnElementId(processId: string): string {
  const id = String(processId || '').trim() || 'Process_1';
  if (isValidBpmnNcName(id)) return id;
  return `Process_${id}`;
}

export function ensureValidBpmnProcessIds(xml: string): {
  xml: string;
  changed: boolean;
  renames: Array<{ from: string; to: string }>;
} {
  const renames: Array<{ from: string; to: string }> = [];
  if (!xml || typeof xml !== 'string') {
    return { xml: xml || '', changed: false, renames };
  }

  let doc: Document;
  try {
    doc = new DOMParser().parseFromString(xml, 'application/xml');
  } catch {
    return { xml, changed: false, renames };
  }
  if (doc.getElementsByTagName('parsererror')[0]) {
    return { xml, changed: false, renames };
  }

  const all = doc.getElementsByTagName('*');
  const map = new Map<string, string>();

  for (let i = 0; i < all.length; i++) {
    const el = all[i];
    const local = el.localName || el.tagName;
    if (!/^process$/i.test(local)) continue;
    const oldId = el.getAttribute('id') || '';
    if (!oldId || isValidBpmnNcName(oldId)) continue;
    const next = toBpmnElementId(oldId);
    if (next === oldId) continue;
    map.set(oldId, next);
    el.setAttribute('id', next);
    renames.push({ from: oldId, to: next });
  }

  if (map.size === 0) {
    return { xml, changed: false, renames };
  }

  for (let i = 0; i < all.length; i++) {
    const el = all[i];
    const ref = el.getAttribute('bpmnElement');
    if (ref && map.has(ref)) {
      el.setAttribute('bpmnElement', map.get(ref)!);
    }
  }

  return {
    xml: new XMLSerializer().serializeToString(doc),
    changed: true,
    renames,
  };
}
