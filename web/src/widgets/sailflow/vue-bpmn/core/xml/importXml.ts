import { DOMParser } from '@xmldom/xmldom';
import type { DiagramElement, Point } from '../types.js';
import type { ImportResult } from './types.js';
import { collectOpaqueAttrs, collectOpaqueInnerXml } from './opaqueXml.js';

function localName(node: Element): string {
  return node.localName || node.nodeName.replace(/^.*:/, '');
}

function attr(el: Element, name: string): string | null {
  return el.getAttribute(name) || el.getAttribute(`bpmn:${name}`);
}

function children(el: Element, name: string): Element[] {
  return Array.from(el.childNodes).filter(
    (n): n is Element => n.nodeType === 1 && localName(n as Element) === name,
  );
}

function firstChild(el: Element, name: string): Element | undefined {
  return children(el, name)[0];
}

function collectByLocalName(root: Element, name: string): Element[] {
  const out: Element[] = [];
  const walk = (node: Element) => {
    if (localName(node) === name) out.push(node);
    Array.from(node.childNodes).forEach((c) => {
      if (c.nodeType === 1) walk(c as Element);
    });
  };
  walk(root);
  return out;
}

function parseBounds(shape: Element): { x: number; y: number; width: number; height: number } | null {
  const bounds = firstChild(shape, 'Bounds');
  if (!bounds) return null;
  return {
    x: Number(bounds.getAttribute('x') ?? 0),
    y: Number(bounds.getAttribute('y') ?? 0),
    width: Number(bounds.getAttribute('width') ?? 0),
    height: Number(bounds.getAttribute('height') ?? 0),
  };
}

function parseWaypoints(edge: Element): Point[] {
  return children(edge, 'waypoint').map((wp) => ({
    x: Number(wp.getAttribute('x') ?? 0),
    y: Number(wp.getAttribute('y') ?? 0),
  }));
}

const FLOW_TYPES = new Set([
  'sequenceFlow',
  'messageFlow',
  'association',
  'dataInputAssociation',
  'dataOutputAssociation',
]);

/**
 * Minimal BPMN 2.0 XML → diagram element map.
 * Intentionally independent of bpmn-js / bpmn-moddle; expand toward D01 parity over time.
 */
export function importXml(xml: string): {
  result: ImportResult;
  elements: Record<string, DiagramElement>;
} {
  const warnings: string[] = [];
  const doc = new DOMParser().parseFromString(xml, 'text/xml');
  const parseError = doc.getElementsByTagName('parsererror')[0];
  if (parseError) {
    throw new Error(`Invalid XML: ${parseError.textContent ?? 'parse error'}`);
  }

  const documentElement = doc.documentElement as Element | null;
  if (!documentElement) {
    throw new Error('Invalid XML: missing document element');
  }
  const rootEl: Element = documentElement;

  const definitions = collectByLocalName(rootEl, 'definitions')[0] ?? rootEl;
  const definitionsId = attr(definitions, 'id') ?? 'Definitions_1';
  const definitionsAttrs = collectOpaqueAttrs(definitions, new Set(['id']));

  const collaboration = collectByLocalName(definitions, 'collaboration')[0];
  const processes = collectByLocalName(definitions, 'process');
  const rootBo = collaboration ?? processes[0];
  if (!rootBo) {
    throw new Error('No bpmn:process or bpmn:collaboration found');
  }
  const rootId = attr(rootBo, 'id') ?? 'Root_1';

  const elements: Record<string, DiagramElement> = {};

  const applyOpaque = (node: DiagramElement, el: Element) => {
    const opaqueAttrs = collectOpaqueAttrs(el);
    if (Object.keys(opaqueAttrs).length) node.opaqueAttrs = opaqueAttrs;
    const opaqueInnerXml = collectOpaqueInnerXml(el);
    if (opaqueInnerXml) node.opaqueInnerXml = opaqueInnerXml;
  };

  const ensure = (id: string, typeLocal: string, parentId: string | null): DiagramElement => {
    const mapped = mapBpmnType(typeLocal);
    if (!elements[id]) {
      elements[id] = {
        id,
        type: mapped,
        parentId,
        name: undefined,
        businessObject: {},
        di: { x: 0, y: 0, width: 0, height: 0, isExpanded: null },
        hostId: null,
        incoming: [],
        outgoing: [],
        waypoints: null,
        label: { kind: 'none' },
        children: [],
      };
    }
    return elements[id]!;
  };

  const registerFlowNode = (el: Element, parentId: string) => {
    const id = attr(el, 'id');
    if (!id) return;
    const typeLocal = localName(el);
    if (typeLocal === 'laneSet') return;
    const node = ensure(id, typeLocal, parentId);
    node.name = attr(el, 'name') ?? undefined;
    node.businessObject = {
      ...node.businessObject,
      name: node.name,
      triggeredByEvent: attr(el, 'triggeredByEvent') === 'true' ? true : undefined,
      isForCompensation: attr(el, 'isForCompensation') === 'true' ? true : undefined,
      cancelActivity: attr(el, 'cancelActivity'),
      attachedToRef: attr(el, 'attachedToRef'),
      isExecutable: attr(el, 'isExecutable'),
      processRef: attr(el, 'processRef') ?? undefined,
      default: attr(el, 'default') ?? undefined,
    };
    const eventDefs = Array.from(el.childNodes)
      .filter((n): n is Element => n.nodeType === 1)
      .map((n) => localName(n))
      .filter((n) => n.endsWith('EventDefinition'))
      .map((n) => `bpmn:${n.charAt(0).toUpperCase()}${n.slice(1)}`);
    if (eventDefs.length) {
      node.businessObject.eventDefinitions = eventDefs;
    }
    if (attr(el, 'attachedToRef')) {
      node.hostId = attr(el, 'attachedToRef');
    }
    applyOpaque(node, el);

    // Nested data associations on activities
    for (const child of Array.from(el.childNodes)) {
      if (child.nodeType !== 1) continue;
      const c = child as Element;
      const cn = localName(c);
      if (cn === 'dataInputAssociation' || cn === 'dataOutputAssociation') {
        registerDataAssociation(c, parentId, id);
      }
    }

    // Nest subprocess children
    if (['subProcess', 'transaction', 'adHocSubProcess', 'SubProcess', 'Transaction', 'AdHocSubProcess'].includes(typeLocal)) {
      Array.from(el.childNodes).forEach((child) => {
        if (child.nodeType !== 1) return;
        const c = child as Element;
        const cn = localName(c);
        if (FLOW_TYPES.has(cn) || ['startEvent', 'endEvent', 'task', 'userTask', 'serviceTask', 'scriptTask', 'businessRuleTask', 'sendTask', 'receiveTask', 'manualTask', 'callActivity', 'exclusiveGateway', 'inclusiveGateway', 'parallelGateway', 'eventBasedGateway', 'complexGateway', 'intermediateCatchEvent', 'intermediateThrowEvent', 'boundaryEvent', 'subProcess', 'transaction', 'adHocSubProcess', 'dataObjectReference', 'dataStoreReference', 'textAnnotation', 'group'].includes(cn)) {
          registerFlowNode(c, id);
        }
        if (FLOW_TYPES.has(cn)) {
          registerConnection(c, id);
        }
      });
    }
  };

  const childText = (el: Element, name: string): string | undefined => {
    const c = firstChild(el, name);
    if (!c) return undefined;
    return (c.textContent ?? '').trim() || undefined;
  };

  const registerConnection = (el: Element, parentId: string) => {
    const id = attr(el, 'id');
    if (!id) return;
    const typeLocal = localName(el);
    const conn = ensure(id, typeLocal, parentId);
    const sourceRef = attr(el, 'sourceRef') ?? childText(el, 'sourceRef');
    const targetRef = attr(el, 'targetRef') ?? childText(el, 'targetRef');
    const conditionExpression = childText(el, 'conditionExpression');
    conn.businessObject = {
      ...conn.businessObject,
      sourceRef,
      targetRef,
      ...(conditionExpression ? { conditionExpression } : {}),
    };
    applyOpaque(conn, el);
    if (sourceRef && elements[sourceRef]) {
      elements[sourceRef]!.outgoing = [...new Set([...elements[sourceRef]!.outgoing, id])];
    }
    if (targetRef && elements[targetRef]) {
      elements[targetRef]!.incoming = [...new Set([...elements[targetRef]!.incoming, id])];
    }
  };

  const registerDataAssociation = (el: Element, parentId: string, hostId?: string) => {
    const id = attr(el, 'id');
    if (!id) return;
    const typeLocal = localName(el);
    const conn = ensure(id, typeLocal, parentId);
    let sourceRef = attr(el, 'sourceRef') ?? childText(el, 'sourceRef');
    let targetRef = attr(el, 'targetRef') ?? childText(el, 'targetRef');
    if (typeLocal === 'dataInputAssociation' && !targetRef && hostId) targetRef = hostId;
    if (typeLocal === 'dataOutputAssociation' && !sourceRef && hostId) sourceRef = hostId;
    conn.businessObject = { ...conn.businessObject, sourceRef, targetRef };
  };

  // Register collaboration participants
  if (collaboration) {
    for (const p of children(collaboration, 'participant')) {
      registerFlowNode(p, rootId);
    }
    for (const mf of children(collaboration, 'messageFlow')) {
      registerConnection(mf, rootId);
    }
  }

  for (const process of processes) {
    const processId = attr(process, 'id') ?? rootId;
    ensure(processId, 'process', null);
    const processEl = elements[processId];
    if (processEl) {
      processEl.businessObject = {
        ...processEl.businessObject,
        isExecutable: attr(process, 'isExecutable') !== 'false',
      };
      applyOpaque(processEl, process);
    }

    const parentForNodes = collaboration ? processId : rootId;

    // lanes
    for (const laneSet of children(process, 'laneSet')) {
      for (const lane of children(laneSet, 'lane')) {
        registerFlowNode(lane, collaboration ? (attr(children(collaboration, 'participant').find((p) => attr(p, 'processRef') === processId) ?? process, 'id') ?? parentForNodes) : parentForNodes);
      }
    }

    Array.from(process.childNodes).forEach((child) => {
      if (child.nodeType !== 1) return;
      const c = child as Element;
      const cn = localName(c);
      if (cn === 'laneSet' || cn === 'sequenceFlow' || cn === 'association' || cn === 'dataObject') return;
      if (cn === 'dataInputAssociation' || cn === 'dataOutputAssociation') return;
      registerFlowNode(c, parentForNodes);
    });

    for (const flow of collectByLocalName(process, 'sequenceFlow')) {
      registerConnection(flow, parentForNodes);
    }
    for (const flow of collectByLocalName(process, 'association')) {
      registerConnection(flow, parentForNodes);
    }

    // Mark default flows from gateway/activity default= attribute
    for (const el of Object.values(elements)) {
      const defId = el.businessObject.default as string | undefined;
      if (defId && elements[defId]) {
        elements[defId] = {
          ...elements[defId]!,
          businessObject: {
            ...elements[defId]!.businessObject,
            isDefault: true,
          },
        };
      }
    }
  }

  // Wire outgoing/incoming after all nodes exist
  for (const el of Object.values(elements)) {
    const isConnection =
      el.type.includes('Flow') ||
      el.type.includes('Association');
    if (!isConnection) continue;
    const sourceRef = el.businessObject.sourceRef as string | undefined;
    const targetRef = el.businessObject.targetRef as string | undefined;
    if (sourceRef && elements[sourceRef]) {
      elements[sourceRef]!.outgoing = [...new Set([...elements[sourceRef]!.outgoing, el.id])];
    }
    if (targetRef && elements[targetRef]) {
      elements[targetRef]!.incoming = [...new Set([...elements[targetRef]!.incoming, el.id])];
    }
  }

  // DI
  for (const shape of collectByLocalName(definitions, 'BPMNShape')) {
    const bpmnElement = shape.getAttribute('bpmnElement');
    if (!bpmnElement || !elements[bpmnElement]) continue;
    const bounds = parseBounds(shape);
    if (bounds) {
      elements[bpmnElement]!.di = {
        ...bounds,
        isExpanded: shape.getAttribute('isExpanded') == null ? null : shape.getAttribute('isExpanded') === 'true',
      };
    }
    const label = firstChild(shape, 'BPMNLabel');
    if (label) {
      const lb = parseBounds(label);
      elements[bpmnElement]!.label = {
        kind: 'external',
        bounds: lb ?? undefined,
        text: elements[bpmnElement]!.name,
      };
    } else if (elements[bpmnElement]!.name) {
      elements[bpmnElement]!.label = { kind: 'internal', text: elements[bpmnElement]!.name };
    }
  }

  for (const edge of collectByLocalName(definitions, 'BPMNEdge')) {
    const bpmnElement = edge.getAttribute('bpmnElement');
    if (!bpmnElement || !elements[bpmnElement]) continue;
    const wps = parseWaypoints(edge);
    elements[bpmnElement]!.waypoints = wps;
    if (wps.length) {
      elements[bpmnElement]!.di = {
        x: wps[0]!.x,
        y: wps[0]!.y,
        width: 0,
        height: 0,
        isExpanded: null,
      };
    }
  }

  if (!Object.keys(elements).length) {
    warnings.push('No diagram elements parsed');
  }

  return {
    result: { warnings, rootId, definitionsId, definitionsAttrs },
    elements,
  };
}

function mapBpmnType(local: string): string {
  const special: Record<string, string> = {
    sequenceFlow: 'bpmn:SequenceFlow',
    messageFlow: 'bpmn:MessageFlow',
    association: 'bpmn:Association',
    dataInputAssociation: 'bpmn:DataInputAssociation',
    dataOutputAssociation: 'bpmn:DataOutputAssociation',
    startEvent: 'bpmn:StartEvent',
    endEvent: 'bpmn:EndEvent',
    intermediateCatchEvent: 'bpmn:IntermediateCatchEvent',
    intermediateThrowEvent: 'bpmn:IntermediateThrowEvent',
    boundaryEvent: 'bpmn:BoundaryEvent',
    exclusiveGateway: 'bpmn:ExclusiveGateway',
    inclusiveGateway: 'bpmn:InclusiveGateway',
    parallelGateway: 'bpmn:ParallelGateway',
    eventBasedGateway: 'bpmn:EventBasedGateway',
    complexGateway: 'bpmn:ComplexGateway',
    userTask: 'bpmn:UserTask',
    serviceTask: 'bpmn:ServiceTask',
    scriptTask: 'bpmn:ScriptTask',
    businessRuleTask: 'bpmn:BusinessRuleTask',
    sendTask: 'bpmn:SendTask',
    receiveTask: 'bpmn:ReceiveTask',
    manualTask: 'bpmn:ManualTask',
    callActivity: 'bpmn:CallActivity',
    subProcess: 'bpmn:SubProcess',
    adHocSubProcess: 'bpmn:AdHocSubProcess',
    transaction: 'bpmn:Transaction',
    textAnnotation: 'bpmn:TextAnnotation',
    dataObjectReference: 'bpmn:DataObjectReference',
    dataStoreReference: 'bpmn:DataStoreReference',
    dataObject: 'bpmn:DataObject',
    participant: 'bpmn:Participant',
    lane: 'bpmn:Lane',
    process: 'bpmn:Process',
    group: 'bpmn:Group',
    task: 'bpmn:Task',
  };
  if (special[local]) return special[local]!;
  return `bpmn:${local.charAt(0).toUpperCase()}${local.slice(1)}`;
}