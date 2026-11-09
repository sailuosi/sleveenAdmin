/**
 * Lightweight moddle-like objects backed by vue-bpmn DiagramElement opaque XML.
 * Enough for existing Sail loadSave / sfExtension helpers without bpmn-js.
 */
import { DOMParser } from '@xmldom/xmldom';
import type { DiagramElement, DiagramStore } from '@vue-bpmn/core';

export type VueModdleEl = {
  $type: string;
  $instanceOf: (type: string) => boolean;
  $parent?: VueModdleEl;
  get: (name: string) => unknown;
  set: (name: string, value: unknown) => void;
  extensionElements?: VueModdleEl;
  values?: VueModdleEl[];
  [key: string]: unknown;
};

const SF_NS = 'http://www.saiflow.com/schema/sf';
const BPMN_NS = 'http://www.omg.org/spec/BPMN/20100524/MODEL';

function localOf(name: string): string {
  return name.includes(':') ? name.split(':')[1]! : name;
}

function qName(type: string): { prefix: string; local: string; ns: string } {
  if (type.startsWith('sf:')) {
    return { prefix: 'sf', local: type.slice(3), ns: SF_NS };
  }
  if (type.startsWith('bpmn:')) {
    return { prefix: 'bpmn', local: type.slice(5), ns: BPMN_NS };
  }
  return { prefix: '', local: type, ns: '' };
}

export function createVueModdleEl(type: string, props: Record<string, unknown> = {}): VueModdleEl {
  const data: Record<string, unknown> = { ...props };
  const el: VueModdleEl = {
    $type: type,
    $instanceOf(t: string) {
      return el.$type === t || localOf(el.$type) === localOf(t);
    },
    get(name: string) {
      return data[name];
    },
    set(name: string, value: unknown) {
      data[name] = value;
      (el as any)[name] = value;
    },
    ...props
  };
  return el;
}

function nodeToModdle(node: Element): VueModdleEl {
  const prefix = node.prefix || (node.namespaceURI === SF_NS ? 'sf' : '');
  const local = node.localName || node.nodeName.replace(/^.*:/, '');
  const type = prefix ? `${prefix}:${local}` : local.startsWith('sf') ? `sf:${local}` : `bpmn:${local}`;
  const props: Record<string, unknown> = {};
  const attrs = node.attributes;
  if (attrs) {
    for (let i = 0; i < attrs.length; i++) {
      const a = attrs.item(i)!;
      const an = a.localName || a.name;
      // Prefer camelCase keys used by Sail (Type → type via lowercasing first letter of BPMN attrs)
      const key = an.charAt(0).toLowerCase() + an.slice(1);
      props[key] = a.value;
      props[an] = a.value;
    }
  }
  const childEls = Array.from(node.childNodes)
    .filter(n => n.nodeType === 1)
    .map(n => n as unknown as Element);
  const values = childEls.map(nodeToModdle);
  if (values.length) {
    props.values = values;
    // Sail Actions uses `actions` list prop
    if (type === 'sf:Actions') props.actions = values;
  }
  const el = createVueModdleEl(type, props);
  for (const c of values) c.$parent = el;
  return el;
}

function parseExtensionElements(xmlFragment: string | undefined): VueModdleEl | undefined {
  if (!xmlFragment?.includes('extensionElements')) return undefined;
  const wrapped = `<root xmlns:bpmn="${BPMN_NS}" xmlns:sf="${SF_NS}">${xmlFragment}</root>`;
  const doc = new DOMParser().parseFromString(wrapped, 'text/xml');
  const root = doc.documentElement;
  if (!root) return undefined;
  let eeNode: Element | null = null;
  for (const c of Array.from(root.childNodes)) {
    if (c.nodeType !== 1) continue;
    const el = c as unknown as Element;
    const ln = el.localName || el.nodeName.replace(/^.*:/, '');
    if (ln === 'extensionElements') {
      eeNode = el;
      break;
    }
  }
  if (!eeNode) return undefined;
  const values = Array.from(eeNode.childNodes)
    .filter(n => n.nodeType === 1)
    .map(n => nodeToModdle(n as unknown as Element));
  const ee = createVueModdleEl('bpmn:ExtensionElements', { values });
  for (const v of values) v.$parent = ee;
  return ee;
}

function serializeModdle(el: VueModdleEl): string {
  const { prefix, local } = qName(el.$type);
  const tag = prefix ? `${prefix}:${local}` : local;
  const skip = new Set([
    '$type',
    '$instanceOf',
    '$parent',
    'get',
    'set',
    'extensionElements',
    'values',
    'actions'
  ]);
  const attrParts: string[] = [];
  for (const [k, v] of Object.entries(el)) {
    if (skip.has(k) || v == null || typeof v === 'object' || typeof v === 'function') continue;
    // Prefer PascalCase for sf attrs as in existing BPMN (Type, Params)
    const attrName = k === 'type' ? 'Type' : k === 'params' ? 'Params' : k === 'json' ? 'json' : k;
    attrParts.push(` ${attrName}="${escapeXml(String(v))}"`);
  }
  const list =
    (el.values as VueModdleEl[] | undefined) ||
    (el.actions as VueModdleEl[] | undefined) ||
    [];
  if (!list.length) {
    return `<${tag}${attrParts.join('')} />`;
  }
  const inner = list.map(serializeModdle).join('');
  return `<${tag}${attrParts.join('')}>${inner}</${tag}>`;
}

function escapeXml(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function replaceExtensionInOpaque(opaque: string | undefined, eeXml: string): string {
  const block = `<bpmn:extensionElements>${eeXml}</bpmn:extensionElements>`;
  if (!opaque?.trim()) return block;
  if (/<\w*:?extensionElements[\s\S]*?<\/\w*:?extensionElements>/i.test(opaque)) {
    return opaque.replace(/<\w*:?extensionElements[\s\S]*?<\/\w*:?extensionElements>/i, block);
  }
  return `${opaque}\n${block}`;
}

export type VueDiagramElementView = {
  __sailVue: true;
  id: string;
  type: string;
  businessObject: VueModdleEl;
  diagramId: string;
  incoming?: string[];
  outgoing?: string[];
  source?: VueDiagramElementView | null;
  target?: VueDiagramElementView | null;
};

export function wrapDiagramElement(
  store: DiagramStore,
  id: string,
  opts?: { withFlowEnds?: boolean },
): VueDiagramElementView | null {
  const de = store.getElement(id);
  if (!de) return null;
  const view = materializeView(de);
  if (opts?.withFlowEnds && String(de.type).includes('Flow')) {
    const srcId = de.businessObject.sourceRef as string | undefined;
    const tgtId = de.businessObject.targetRef as string | undefined;
    const src = srcId ? store.getElement(srcId) : undefined;
    const tgt = tgtId ? store.getElement(tgtId) : undefined;
    view.source = src ? materializeView(src) : null;
    view.target = tgt ? materializeView(tgt) : null;
  }
  return view;
}

const BO_COPY_KEYS = [
  'documentation',
  'isExecutable',
  'processRef',
  'loopCharacteristics',
  'isSequential',
  'loopCardinality',
  'eventDefinitions',
  'timerKind',
  'timeDuration',
  'condition',
  'messageName',
  'signalName',
  'errorName',
  'escalationName',
  'linkName',
  'completionCondition',
] as const;

function materializeView(de: DiagramElement): VueDiagramElementView {
  const bo = createVueModdleEl(String(de.type), {
    id: de.id,
    name: de.name,
    ...(de.opaqueAttrs
      ? Object.fromEntries(
          Object.entries(de.opaqueAttrs).map(([k, v]) => {
            const local = k.includes(':') ? k.split(':')[1]! : k;
            const key = local.charAt(0).toLowerCase() + local.slice(1);
            return [key, v];
          }),
        )
      : {}),
  });
  for (const k of BO_COPY_KEYS) {
    if (de.businessObject[k] !== undefined) {
      bo[k] = de.businessObject[k];
      bo.set(k, de.businessObject[k]);
    }
  }
  // conditionExpression may live in businessObject from import
  if (de.businessObject.conditionExpression != null) {
    bo.conditionExpression = { body: String(de.businessObject.conditionExpression) };
    bo.set('conditionExpression', bo.conditionExpression);
  }
  const ee = parseExtensionElements(de.opaqueInnerXml);
  if (ee) {
    bo.extensionElements = ee;
    bo.set('extensionElements', ee);
  }
  return {
    __sailVue: true,
    id: de.id,
    type: String(de.type),
    businessObject: bo,
    diagramId: de.id,
    incoming: [...de.incoming],
    outgoing: [...de.outgoing],
  };
}

export function createVueModdleBridge() {
  return {
    create(type: string, props?: object) {
      return createVueModdleEl(type, (props || {}) as Record<string, unknown>);
    },
  };
}

/** Keys that live on DiagramElement.businessObject (not sf: opaque attrs). */
const BO_PROP_KEYS = new Set([
  'documentation',
  'isExecutable',
  'loopCharacteristics',
  'isSequential',
  'loopCardinality',
  'eventDefinitions',
  'timerKind',
  'timeDuration',
  'condition',
  'messageName',
  'signalName',
  'errorName',
  'escalationName',
  'linkName',
  'completionCondition',
  'conditionExpression',
]);

export function createVueModelingBridge(store: DiagramStore) {
  return {
    updateProperties(el: unknown, props: object) {
      const view = el as VueDiagramElementView;
      if (!view?.__sailVue || !view.diagramId) return;
      const de = store.getElement(view.diagramId);
      if (!de) return;

      const next = { ...de };
      const p = props as Record<string, unknown>;
      const nextBo: Record<string, unknown> = { ...next.businessObject };

      if (typeof p.name === 'string') {
        next.name = p.name;
        nextBo.name = p.name;
        view.businessObject.name = p.name;
        view.businessObject.set('name', p.name);
      }

      const opaqueAttrs = { ...(next.opaqueAttrs || {}) };
      for (const [k, v] of Object.entries(p)) {
        if (k === 'name' || k === 'extensionElements') continue;
        if (BO_PROP_KEYS.has(k)) {
          if (v === undefined || v === null || v === '') {
            delete nextBo[k];
            delete view.businessObject[k];
          } else {
            nextBo[k] = v;
            view.businessObject[k] = v;
            view.businessObject.set(k, v);
          }
          continue;
        }
        if (v === undefined) {
          delete opaqueAttrs[`sf:${k}`];
          delete opaqueAttrs[k];
          continue;
        }
        if (typeof v === 'string' || typeof v === 'number' || typeof v === 'boolean') {
          const attrKey = k === 'code' || k === 'url' || k === 'version' ? `sf:${k}` : k;
          opaqueAttrs[attrKey] = String(v);
          view.businessObject[k] = v;
          view.businessObject.set(k, v);
        }
      }

      if (p.extensionElements) {
        const ee = p.extensionElements as VueModdleEl;
        view.businessObject.extensionElements = ee;
        view.businessObject.set('extensionElements', ee);
        const values = (ee.values || []) as VueModdleEl[];
        const inner = values.map(serializeModdle).join('');
        next.opaqueInnerXml = replaceExtensionInOpaque(next.opaqueInnerXml, inner);
      } else if (view.businessObject.extensionElements) {
        const ee = view.businessObject.extensionElements;
        const values = (ee.values || []) as VueModdleEl[];
        const inner = values.map(serializeModdle).join('');
        next.opaqueInnerXml = replaceExtensionInOpaque(next.opaqueInnerXml, inner);
      }

      // Keep opaque XML free of fields we emit from businessObject
      if ('documentation' in p || 'loopCharacteristics' in p || 'loopCardinality' in p) {
        let xml = next.opaqueInnerXml || '';
        if ('documentation' in p) {
          xml = xml
            .replace(/<\w*:?documentation\b[^>]*\/>/gi, '')
            .replace(/<\w*:?documentation\b[\s\S]*?<\/\w*:?documentation>/gi, '');
        }
        if ('loopCharacteristics' in p || 'loopCardinality' in p) {
          xml = xml
            .replace(/<\w*:?multiInstanceLoopCharacteristics\b[^>]*\/>/gi, '')
            .replace(
              /<\w*:?multiInstanceLoopCharacteristics\b[\s\S]*?<\/\w*:?multiInstanceLoopCharacteristics>/gi,
              '',
            );
        }
        next.opaqueInnerXml = xml.trim() || undefined;
      }

      next.opaqueAttrs = opaqueAttrs;
      next.businessObject = { ...nextBo, name: next.name };
      store.updateElement(view.diagramId, next);
    },
    updateModdleProperties(el: unknown, _moddleEl: object, props: object) {
      // Vue path: flatten into element businessObject via updateProperties
      this.updateProperties(el, props);
    },
  };
}

/** Strip sf:Variables from opaque extension XML before save. */
export function stripSfVariablesFromStore(store: DiagramStore) {
  const state = store.getState();
  for (const el of Object.values(state.elements) as DiagramElement[]) {
    if (!el.opaqueInnerXml?.includes('Variables')) continue;
    const cleaned = el.opaqueInnerXml
      .replace(/<\w*:?Variables\b[^>]*\/>/gi, '')
      .replace(/<\w*:?Variables\b[\s\S]*?<\/\w*:?Variables>/gi, '');
    if (cleaned !== el.opaqueInnerXml) {
      store.updateElement(el.id, { opaqueInnerXml: cleaned });
    }
  }
}
