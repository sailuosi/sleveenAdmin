import { XMLSerializer } from '@xmldom/xmldom';

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

function localName(node: Element): string {
  return node.localName || node.nodeName.replace(/^.*:/, '');
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
    const name = a.name;
    const local = a.localName || name.replace(/^.*:/, '');
    if (known.has(name) || known.has(local)) continue;
    out[name] = a.value;
  }
  return out;
}

export function collectOpaqueInnerXml(
  el: Element,
  structural: Set<string> = STRUCTURAL_CHILD_LOCAL,
): string | undefined {
  const serializer = new XMLSerializer();
  const parts: string[] = [];
  for (const child of Array.from(el.childNodes)) {
    if (child.nodeType !== 1) continue;
    const c = child as Element;
    const ln = localName(c);
    if (structural.has(ln)) continue;
    if (ln.endsWith('EventDefinition')) continue;
    parts.push(serializer.serializeToString(c as never));
  }
  if (!parts.length) return undefined;
  return parts.join('\n');
}

export function formatOpaqueAttrs(opaque?: Record<string, string>): string {
  if (!opaque) return '';
  return Object.entries(opaque)
    .map(([k, v]) => ` ${k}="${escapeAttr(v)}"`)
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
  return xml
    .split(/\r?\n/)
    .map((line) => (line.trim() ? `${pad}${line.trim()}` : ''))
    .filter(Boolean)
    .join('\n');
}

export const SF_NS = 'http://www.saiflow.com/schema/sf';
export const DEFAULT_DEFINITIONS_ATTRS: Record<string, string> = {
  'xmlns:xsi': 'http://www.w3.org/2001/XMLSchema-instance',
  'xmlns:sf': SF_NS,
  targetNamespace: 'http://bpmn.io/schema/bpmn',
};
