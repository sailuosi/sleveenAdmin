/**
 * 保存前清理 BPMN：去掉无对应流程元素的 DI 形状/连线，避免重开后「幽灵节点」或 import 异常。
 * 不补全缺失的 sourceRef/targetRef（那是画布数据问题，应在设计器内修）。
 */
export function sanitizeBpmnXml(xml: string): { xml: string; removedDi: string[] } {
  const removedDi: string[] = [];
  if (!xml || typeof xml !== 'string') return { xml: xml || '', removedDi };

  let doc: Document;
  try {
    doc = new DOMParser().parseFromString(xml, 'application/xml');
  } catch {
    return { xml, removedDi };
  }
  const parseErr = doc.getElementsByTagName('parsererror')[0];
  if (parseErr) return { xml, removedDi };

  const known = new Set<string>();
  const all = doc.getElementsByTagName('*');
  for (let i = 0; i < all.length; i++) {
    const el = all[i];
    const local = el.localName || el.tagName;
    // 跳过 DI / 定义根
    if (/^(BPMNDiagram|BPMNPlane|BPMNShape|BPMNEdge|BPMNLabel|Bounds|waypoint|definitions)$/i.test(local)) {
      continue;
    }
    const id = el.getAttribute('id');
    if (id) known.add(id);
  }

  // 残缺 sequenceFlow（缺 sourceRef/targetRef）会导致引擎 ToActivity NRE
  const brokenFlows: Element[] = [];
  for (let i = 0; i < all.length; i++) {
    const el = all[i];
    const local = el.localName || el.tagName;
    if (!/^sequenceFlow$/i.test(local)) continue;
    const src = el.getAttribute('sourceRef') || '';
    const tgt = el.getAttribute('targetRef') || '';
    if (!src || !tgt || !known.has(src) || !known.has(tgt)) {
      brokenFlows.push(el);
      const id = el.getAttribute('id');
      if (id) known.delete(id);
    }
  }
  brokenFlows.forEach(el => el.parentNode?.removeChild(el));

  const toRemove: Element[] = [];
  for (let i = 0; i < all.length; i++) {
    const el = all[i];
    const local = el.localName || el.tagName;
    if (!/^(BPMNShape|BPMNEdge)$/i.test(local)) continue;
    const ref = el.getAttribute('bpmnElement') || '';
    if (!ref || known.has(ref)) continue;
    removedDi.push(ref);
    toRemove.push(el);
  }
  toRemove.forEach(el => el.parentNode?.removeChild(el));

  // 去掉空 Bounds 的残留（无坐标的 shape 往往是删除半截）
  const bounds = doc.getElementsByTagName('*');
  const emptyShapes: Element[] = [];
  for (let i = 0; i < bounds.length; i++) {
    const el = bounds[i];
    const local = el.localName || el.tagName;
    if (!/^BPMNShape$/i.test(local)) continue;
    const b = Array.from(el.children).find(c => /Bounds$/i.test(c.localName || c.tagName));
    if (!b) continue;
    const hasBox =
      b.getAttribute('x') != null &&
      b.getAttribute('y') != null &&
      b.getAttribute('width') != null &&
      b.getAttribute('height') != null;
    if (!hasBox) emptyShapes.push(el);
  }
  emptyShapes.forEach(el => {
    const ref = el.getAttribute('bpmnElement') || '?';
    if (!removedDi.includes(ref)) removedDi.push(ref);
    el.parentNode?.removeChild(el);
  });

  const serialized = new XMLSerializer().serializeToString(doc);
  return { xml: serialized, removedDi };
}

/** 统计 process 内任务类节点数量（用于保存前粗检） */
export function countProcessTaskElements(xml: string): number {
  if (!xml) return 0;
  const doc = new DOMParser().parseFromString(xml, 'application/xml');
  if (doc.getElementsByTagName('parsererror')[0]) return 0;
  let n = 0;
  const all = doc.getElementsByTagName('*');
  for (let i = 0; i < all.length; i++) {
    const local = all[i].localName || all[i].tagName;
    if (/^(userTask|task|serviceTask|scriptTask|businessRuleTask|manualTask|sendTask|receiveTask|callActivity|subProcess)$/i.test(local)) {
      n += 1;
    }
  }
  return n;
}
