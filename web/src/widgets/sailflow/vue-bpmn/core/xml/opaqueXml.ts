/** Attributes already mapped into DiagramElement / businessObject */
export const KNOWN_ELEMENT_ATTRS = new Set([
  'id',
  'name',
  'isExecutable',
  'triggeredByEvent',
  'isForCompensation',
  'cancelActivity',
  'attachedToRef',
  'processRef',
  'default',
  'sourceRef',
  'targetRef',
]);

/** Child localNames handled structurally (not kept as opaque inner XML) */
export const STRUCTURAL_CHILD_LOCAL = new Set([
  'incoming',
  'outgoing',
  'laneSet',
  'sequenceFlow',
  'messageFlow',
  'association',
  'dataInputAssociation',
  'dataOutputAssociation',
  'dataObject',
  'startEvent',
  'endEvent',
  'intermediateCatchEvent',
  'intermediateThrowEvent',
  'boundaryEvent',
  'exclusiveGateway',
  'inclusiveGateway',
  'parallelGateway',
  'eventBasedGateway',
  'complexGateway',
  'userTask',
  'serviceTask',
  'scriptTask',
  'businessRuleTask',
  'sendTask',
  'receiveTask',
  'manualTask',
  'callActivity',
  'subProcess',
  'adHocSubProcess',
  'transaction',
  'textAnnotation',
  'dataObjectReference',
  'dataStoreReference',
  'participant',
  'lane',
  'process',
  'group',
  'task',
]);

export const SF_NS = 'http://www.saiflow.com/schema/sf';
export const SLICK_SF_NS = 'http://www.slickflow.com/schema/sf';

/** 稳定前缀：避免 xmldom XMLSerializer 产出 ns0/ns1 且无 xmlns 声明 */
const NS_TO_PREFIX: Record<string, string> = {
  'http://www.omg.org/spec/BPMN/20100524/MODEL': 'bpmn',
  'http://www.omg.org/spec/BPMN/20100524/DI': 'bpmndi',
  'http://www.omg.org/spec/DD/20100524/DC': 'dc',
  'http://www.omg.org/spec/DD/20100524/DI': 'di',
  'http://www.w3.org/2001/XMLSchema-instance': 'xsi',
  [SF_NS]: 'sf',
  [SLICK_SF_NS]: 'sf',
};

export const DEFAULT_DEFINITIONS_ATTRS: Record<string, string> = {
  'xmlns:xsi': 'http://www.w3.org/2001/XMLSchema-instance',
  'xmlns:sf': SF_NS,
  targetNamespace: 'http://bpmn.io/schema/bpmn',
};

function localName(node: Element): string {
  return node.localName || node.nodeName.replace(/^.*:/, '');
}

function isGeneratedNsPrefix(prefix: string | null | undefined): boolean {
  return !!prefix && /^ns\d+$/i.test(prefix);
}

function prefixForUri(uri: string | null | undefined): string | null {
  if (!uri) return null;
  return NS_TO_PREFIX[uri] ?? null;
}

function qualifyName(uri: string | null, local: string, fallbackPrefix: string | null): string {
  const mapped = prefixForUri(uri);
  if (mapped) return `${mapped}:${local}`;
  if (fallbackPrefix && !isGeneratedNsPrefix(fallbackPrefix)) {
    return `${fallbackPrefix}:${local}`;
  }
  return local;
}

function serializeAttrName(a: Attr): string | null {
  const name = a.name || '';
  // 丢弃自动生成的 xmlns:ns0，避免脏前缀进导出文档
  if (/^xmlns:ns\d+$/i.test(name)) return null;
  if (name === 'xmlns' || name.startsWith('xmlns:')) {
    // 保留真实业务命名空间声明；把 slickflow 归一到 sf
    if (a.value === SLICK_SF_NS) return 'xmlns:sf';
    return name;
  }
  const local = a.localName || name.replace(/^.*:/, '');
  const uri = a.namespaceURI || '';
  const mapped = prefixForUri(uri);
  if (mapped) return `${mapped}:${local}`;
  const prefix = a.prefix || (name.includes(':') ? name.split(':')[0] : null);
  if (isGeneratedNsPrefix(prefix)) {
    // 无 URI 的 ns0:xxx 无法可靠还原，跳过以免写坏 XML
    if (!uri) return null;
    if (mapped) return `${mapped}:${local}`;
    return local;
  }
  return name;
}

/** 手写序列化，强制 bpmn/sf/xsi 等稳定前缀，杜绝 ns0 */
export function serializeElementStable(el: Element): string {
  const local = localName(el);
  const tag = qualifyName(el.namespaceURI, local, el.prefix);
  const attrParts: string[] = [];
  const attrs = el.attributes;
  if (attrs) {
    for (let i = 0; i < attrs.length; i++) {
      const a = attrs.item(i);
      if (!a) continue;
      const an = serializeAttrName(a);
      if (!an) continue;
      let val = a.value;
      if (an === 'xmlns:sf' || (an.startsWith('xmlns:') && val === SLICK_SF_NS)) {
        val = SF_NS;
      }
      attrParts.push(` ${an}="${escapeAttr(val)}"`);
    }
  }

  const childParts: string[] = [];
  for (const child of Array.from(el.childNodes)) {
    if (child.nodeType === 1) {
      childParts.push(serializeElementStable(child as Element));
    } else if (child.nodeType === 3) {
      const t = child.textContent ?? '';
      if (t.trim()) childParts.push(escapeText(t));
    }
  }

  const open = `<${tag}${attrParts.join('')}`;
  if (!childParts.length) return `${open} />`;
  return `${open}>${childParts.join('')}</${tag}>`;
}

/**
 * 清洗已落入 store 的 opaque 片段（历史 ns0 / slickflow NS）。
 * 在导出前再跑一遍，兼容旧会话数据。
 */
export function sanitizeOpaqueXml(xml: string): string {
  if (!xml) return xml;
  let s = xml.replaceAll(SLICK_SF_NS, SF_NS);

  // xmlns:ns0="URI" → 稳定前缀；标签/属性上的 ns0: 同步替换
  for (let n = 0; n < 32; n++) {
    const xmlnsRe = new RegExp(`xmlns:ns${n}="([^"]*)"`, 'gi');
    const matches = [...s.matchAll(xmlnsRe)];
    for (const m of matches) {
      const uri = m[1] || '';
      const pref =
        prefixForUri(uri) ||
        (uri.includes('saiflow') || uri.includes('slickflow') ? 'sf' : 'bpmn');
      const finalUri = uri === SLICK_SF_NS ? SF_NS : uri;
      s = s.replace(
        new RegExp(`xmlns:ns${n}="${escapeRegExp(uri)}"`, 'gi'),
        `xmlns:${pref}="${finalUri}"`,
      );
      s = s.replace(new RegExp(`([\\s</])ns${n}:`, 'gi'), `$1${pref}:`);
    }
    // 仍残留且无 xmlns 的 nsN:（extensionElements 等）→ bpmn
    s = s.replace(new RegExp(`([\\s</])ns${n}:`, 'gi'), '$1bpmn:');
    s = s.replace(new RegExp(`xmlns:ns${n}="[^"]*"`, 'gi'), '');
  }
  return s;
}

function escapeRegExp(s: string): string {
  return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function escapeText(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

export function collectOpaqueAttrs(
  el: Element,
  known: Set<string> = KNOWN_ELEMENT_ATTRS,
): Record<string, string> {
  const out: Record<string, string> = {};
  const attrs = el.attributes;
  if (!attrs) return out;
  for (let i = 0; i < attrs.length; i++) {
    const a = attrs.item(i);
    if (!a) continue;
    const rawName = a.name;
    const local = a.localName || rawName.replace(/^.*:/, '');
    if (known.has(rawName) || known.has(local)) continue;
    if (/^xmlns:ns\d+$/i.test(rawName)) continue;
    if (isGeneratedNsPrefix(a.prefix)) {
      const mapped = prefixForUri(a.namespaceURI);
      if (!mapped && !a.namespaceURI) continue;
      const key = `${mapped ?? 'sf'}:${local}`;
      out[key] = a.value === SLICK_SF_NS ? SF_NS : a.value;
      continue;
    }
    let name = rawName;
    if (name === 'xmlns:sf' || (name.startsWith('xmlns:') && a.value === SLICK_SF_NS)) {
      name = 'xmlns:sf';
    }
    out[name] = a.value === SLICK_SF_NS ? SF_NS : a.value;
  }
  return out;
}

export function collectOpaqueInnerXml(
  el: Element,
  structural: Set<string> = STRUCTURAL_CHILD_LOCAL,
): string | undefined {
  const parts: string[] = [];
  for (const child of Array.from(el.childNodes)) {
    if (child.nodeType !== 1) continue;
    const c = child as Element;
    const ln = localName(c);
    if (structural.has(ln)) continue;
    if (ln.endsWith('EventDefinition')) continue;
    parts.push(serializeElementStable(c));
  }
  if (!parts.length) return undefined;
  return parts.join('\n');
}

export function formatOpaqueAttrs(opaque?: Record<string, string>): string {
  if (!opaque) return '';
  return Object.entries(opaque)
    .filter(([k]) => !/^xmlns:ns\d+$/i.test(k) && !/^ns\d+:/i.test(k))
    .map(([k, v]) => ` ${k}="${escapeAttr(v === SLICK_SF_NS ? SF_NS : v)}"`)
    .join('');
}

export function escapeAttr(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

/** Indent each line of opaque inner XML to match surrounding export. */
export function indentOpaqueInner(xml: string, level: number): string {
  const pad = '  '.repeat(level);
  const cleaned = sanitizeOpaqueXml(xml);
  return cleaned
    .split(/\r?\n/)
    .map((line) => (line.trim() ? `${pad}${line.trim()}` : ''))
    .filter(Boolean)
    .join('\n');
}
