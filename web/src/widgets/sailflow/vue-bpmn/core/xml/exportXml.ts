import type { DiagramElement, DiagramSnapshot } from '../types.js';
import type { ExportResult } from './types.js';
import { isConnection, isContainerActivity, localBpmnTag } from './elementKind.js';
import {
  DEFAULT_DEFINITIONS_ATTRS,
  escapeAttr,
  formatOpaqueAttrs,
  indentOpaqueInner,
} from './opaqueXml.js';

function esc(value: string): string {
  return escapeAttr(value);
}

function indent(level: number): string {
  return '  '.repeat(level);
}

function childrenOf(
  elements: Record<string, DiagramElement>,
  parentId: string,
): DiagramElement[] {
  return Object.values(elements).filter((el) => el.parentId === parentId);
}

function appendOpaqueBody(el: DiagramElement, level: number, bodyParts: string[]) {
  if (!el.opaqueInnerXml) return;
  // Strip children that we also emit from businessObject to avoid duplicates
  // sanitize：清理历史 xmldom ns0 脏前缀
  let xml = el.opaqueInnerXml;
  if (el.businessObject.documentation != null && String(el.businessObject.documentation) !== '') {
    xml = xml
      .replace(/<\w*:?documentation\b[^>]*\/>/gi, '')
      .replace(/<\w*:?documentation\b[\s\S]*?<\/\w*:?documentation>/gi, '');
  }
  if (el.businessObject.loopCharacteristics) {
    xml = xml
      .replace(/<\w*:?multiInstanceLoopCharacteristics\b[^>]*\/>/gi, '')
      .replace(
        /<\w*:?multiInstanceLoopCharacteristics\b[\s\S]*?<\/\w*:?multiInstanceLoopCharacteristics>/gi,
        '',
      );
  }
  const trimmed = xml.trim();
  if (!trimmed) return;
  bodyParts.push(indentOpaqueInner(trimmed, level + 1));
}

/** Emit documentation / eventDefinitions / multi-instance from flattened BO fields. */
function appendStructuralBoChildren(
  el: DiagramElement,
  level: number,
  bodyParts: string[],
) {
  const pad = indent(level + 1);
  const bo = el.businessObject;
  const doc = bo.documentation != null ? String(bo.documentation) : '';
  if (doc) {
    bodyParts.push(`${pad}<bpmn:documentation>${esc(doc)}</bpmn:documentation>`);
  }

  const defs = (bo.eventDefinitions as string[] | undefined) ?? [];
  for (const def of defs) {
    const local = def.replace(/^bpmn:/, '');
    const tag = local.charAt(0).toLowerCase() + local.slice(1);
    if (local.includes('Timer')) {
      const kind = String(bo.timerKind || 'timeDuration');
      const body = esc(String(bo.timeDuration ?? ''));
      const childTag =
        kind === 'timeDate' ? 'timeDate' : kind === 'timeCycle' ? 'timeCycle' : 'timeDuration';
      bodyParts.push(
        `${pad}<bpmn:${tag}>\n${pad}  <bpmn:${childTag} xsi:type="bpmn:tFormalExpression">${body}</bpmn:${childTag}>\n${pad}</bpmn:${tag}>`,
      );
    } else if (local.includes('Conditional')) {
      const body = esc(String(bo.condition ?? ''));
      bodyParts.push(
        `${pad}<bpmn:${tag}>\n${pad}  <bpmn:condition xsi:type="bpmn:tFormalExpression">${body}</bpmn:condition>\n${pad}</bpmn:${tag}>`,
      );
    } else if (local.includes('Message')) {
      const name = String(bo.messageName ?? '');
      const attr = name ? ` messageRef="${esc(name)}"` : '';
      bodyParts.push(`${pad}<bpmn:${tag}${attr} />`);
    } else if (local.includes('Signal')) {
      const name = String(bo.signalName ?? '');
      const attr = name ? ` signalRef="${esc(name)}"` : '';
      bodyParts.push(`${pad}<bpmn:${tag}${attr} />`);
    } else if (local.includes('Error')) {
      const name = String(bo.errorName ?? '');
      const attr = name ? ` errorRef="${esc(name)}"` : '';
      bodyParts.push(`${pad}<bpmn:${tag}${attr} />`);
    } else if (local.includes('Escalation')) {
      const name = String(bo.escalationName ?? '');
      const attr = name ? ` escalationRef="${esc(name)}"` : '';
      bodyParts.push(`${pad}<bpmn:${tag}${attr} />`);
    } else if (local.includes('Link')) {
      const name = String(bo.linkName ?? '');
      const attr = name ? ` name="${esc(name)}"` : '';
      bodyParts.push(`${pad}<bpmn:${tag}${attr} />`);
    } else {
      bodyParts.push(`${pad}<bpmn:${tag} />`);
    }
  }

  const loop = String(bo.loopCharacteristics ?? '');
  if (loop === 'parallel' || loop === 'sequential') {
    const seq = loop === 'sequential' || bo.isSequential === true;
    const card = String(bo.loopCardinality ?? '').trim();
    if (card) {
      bodyParts.push(
        `${pad}<bpmn:multiInstanceLoopCharacteristics isSequential="${seq ? 'true' : 'false'}">\n${pad}  <bpmn:loopCardinality>${esc(card)}</bpmn:loopCardinality>\n${pad}</bpmn:multiInstanceLoopCharacteristics>`,
      );
    } else {
      bodyParts.push(
        `${pad}<bpmn:multiInstanceLoopCharacteristics isSequential="${seq ? 'true' : 'false'}" />`,
      );
    }
  }
}

function serializeNode(
  el: DiagramElement,
  elements: Record<string, DiagramElement>,
  level: number,
): string {
  const tag = localBpmnTag(el.type);
  const pad = indent(level);
  const nameAttr = el.name ? ` name="${esc(el.name)}"` : '';
  const extraAttrs: string[] = [];
  if (el.businessObject.triggeredByEvent === true) extraAttrs.push(' triggeredByEvent="true"');
  if (el.businessObject.isForCompensation === true) extraAttrs.push(' isForCompensation="true"');
  if (el.businessObject.cancelActivity != null) {
    extraAttrs.push(` cancelActivity="${esc(String(el.businessObject.cancelActivity))}"`);
  }
  if (el.hostId) extraAttrs.push(` attachedToRef="${esc(el.hostId)}"`);
  if (el.type === 'bpmn:Participant' && el.businessObject.processRef) {
    extraAttrs.push(` processRef="${esc(String(el.businessObject.processRef))}"`);
  }
  if (el.type === 'bpmn:Process' && el.businessObject.isExecutable === false) {
    extraAttrs.push(' isExecutable="false"');
  } else if (el.type === 'bpmn:Process') {
    // default true is written by process wrapper; skip here when used as nested
  }
  extraAttrs.push(formatOpaqueAttrs(el.opaqueAttrs));

  const nested = childrenOf(elements, el.id);
  const nestedNodes = nested.filter((c) => !isConnection(c));
  const nestedFlows = nested.filter((c) => isConnection(c));

  const incoming = el.incoming
    .map((id) => `${pad}  <bpmn:incoming>${esc(id)}</bpmn:incoming>`)
    .join('\n');
  const outgoing = el.outgoing
    .map((id) => `${pad}  <bpmn:outgoing>${esc(id)}</bpmn:outgoing>`)
    .join('\n');

  const bodyParts = [incoming, outgoing].filter(Boolean);
  appendStructuralBoChildren(el, level, bodyParts);
  appendOpaqueBody(el, level, bodyParts);

  if (isContainerActivity(el) || el.type === 'bpmn:Process') {
    const inner = [
      ...nestedNodes.map((c) => serializeNode(c, elements, level + 1)),
      ...nestedFlows.map((c) => serializeConnection(c, level + 1)),
    ]
      .filter(Boolean)
      .join('\n');
    if (inner) bodyParts.push(inner);
  }

  const attrs = `${nameAttr}${extraAttrs.join('')}`;
  const body = bodyParts.filter(Boolean).join('\n');
  if (!body) return `${pad}<bpmn:${tag} id="${esc(el.id)}"${attrs} />`;
  return `${pad}<bpmn:${tag} id="${esc(el.id)}"${attrs}>\n${body}\n${pad}</bpmn:${tag}>`;
}

function serializeConnection(el: DiagramElement, level: number): string {
  const tag = localBpmnTag(el.type);
  const pad = indent(level);
  const source = String(el.businessObject.sourceRef ?? '');
  const target = String(el.businessObject.targetRef ?? '');
  const nameAttr = el.name ? ` name="${esc(el.name)}"` : '';
  const opaque = formatOpaqueAttrs(el.opaqueAttrs);
  const bodyParts: string[] = [];
  const hasCondOpaque = Boolean(el.opaqueInnerXml?.includes('conditionExpression'));
  if (!hasCondOpaque && el.businessObject.conditionExpression) {
    const expr = esc(String(el.businessObject.conditionExpression));
    bodyParts.push(
      `${pad}  <bpmn:conditionExpression xsi:type="bpmn:tFormalExpression">${expr}</bpmn:conditionExpression>`,
    );
  }
  appendOpaqueBody(el, level, bodyParts);
  const body = bodyParts.filter(Boolean).join('\n');
  if (!body) {
    return `${pad}<bpmn:${tag} id="${esc(el.id)}"${nameAttr}${opaque} sourceRef="${esc(source)}" targetRef="${esc(target)}" />`;
  }
  return `${pad}<bpmn:${tag} id="${esc(el.id)}"${nameAttr}${opaque} sourceRef="${esc(source)}" targetRef="${esc(target)}">\n${body}\n${pad}</bpmn:${tag}>`;
}

function serializeLabel(el: DiagramElement, pad: string): string {
  if (el.label?.kind !== 'external' || !el.label.bounds) return '';
  const b = el.label.bounds;
  return `
${pad}  <bpmndi:BPMNLabel>
${pad}    <dc:Bounds x="${b.x}" y="${b.y}" width="${b.width}" height="${b.height}" />
${pad}  </bpmndi:BPMNLabel>`;
}

function serializeDi(elements: DiagramElement[]): { shapes: string; edges: string } {
  const shapes = elements
    .filter((el) => !isConnection(el))
    .map((el) => {
      const expanded =
        el.di.isExpanded == null ? '' : ` isExpanded="${el.di.isExpanded ? 'true' : 'false'}"`;
      const label = serializeLabel(el, '      ');
      return `      <bpmndi:BPMNShape id="${esc(el.id)}_di" bpmnElement="${esc(el.id)}"${expanded}>
        <dc:Bounds x="${el.di.x}" y="${el.di.y}" width="${el.di.width}" height="${el.di.height}" />${label}
      </bpmndi:BPMNShape>`;
    })
    .join('\n');

  const edges = elements
    .filter((el) => isConnection(el))
    .map((el) => {
      const wps = (el.waypoints ?? [])
        .map((p) => `        <di:waypoint x="${p.x}" y="${p.y}" />`)
        .join('\n');
      const label = serializeLabel(el, '      ');
      return `      <bpmndi:BPMNEdge id="${esc(el.id)}_di" bpmnElement="${esc(el.id)}">
${wps}${label}
      </bpmndi:BPMNEdge>`;
    })
    .join('\n');

  return { shapes, edges };
}

function formatDefinitionsOpen(
  definitionsId: string,
  definitionsAttrs: Record<string, string> | undefined,
): string {
  const merged: Record<string, string> = {
    ...DEFAULT_DEFINITIONS_ATTRS,
    ...(definitionsAttrs ?? {}),
  };
  // Always emit core BPMN namespaces first
  const fixed = [
    'xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"',
    'xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"',
    'xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"',
    'xmlns:di="http://www.omg.org/spec/DD/20100524/DI"',
  ];
  const skip = new Set(['xmlns:bpmn', 'xmlns:bpmndi', 'xmlns:dc', 'xmlns:di', 'id']);
  const extras = Object.entries(merged)
    .filter(([k]) => !skip.has(k))
    .map(([k, v]) => `${k}="${esc(v)}"`);
  return `<bpmn:definitions ${[...fixed, ...extras].join('\n                  ')}
                  id="${esc(definitionsId)}">`;
}

/**
 * Export diagram snapshot to BPMN 2.0 XML.
 * Preserves opaqueAttrs / opaqueInnerXml (sf: extensions, extensionElements, …).
 */
export function exportXml(state: DiagramSnapshot): ExportResult {
  const rootId = state.rootId ?? 'Process_1';
  const definitionsId = state.definitionsId ?? 'Definitions_1';
  const elements = state.elements;
  const all = Object.values(elements);
  const rootType = elements[rootId]?.type;
  const isCollab =
    rootType === 'bpmn:Collaboration' ||
    all.some((el) => el.type === 'bpmn:Participant' || el.type === 'bpmn:MessageFlow');

  let semanticXml: string;

  if (isCollab) {
    const collaborationId =
      all.find((el) => el.type === 'bpmn:Collaboration')?.id ??
      (rootType === 'bpmn:Collaboration' ? rootId : 'Collaboration_1');
    const participants = all.filter((el) => el.type === 'bpmn:Participant');
    const messageFlows = all.filter((el) => el.type === 'bpmn:MessageFlow');
    const processIds = new Set(
      participants
        .map((p) => p.businessObject.processRef as string | undefined)
        .filter((id): id is string => Boolean(id)),
    );
    for (const el of all) {
      if (el.type === 'bpmn:Process') processIds.add(el.id);
    }

    const collabBody = [
      ...participants.map((p) => serializeNode(p, elements, 2)),
      ...messageFlows.map((f) => serializeConnection(f, 2)),
    ].join('\n');

    const processesXml = [...processIds]
      .map((processId) => {
        const processEl = elements[processId];
        const direct = childrenOf(elements, processId).filter(
          (el) => el.type !== 'bpmn:Process' && el.type !== 'bpmn:Participant',
        );
        const nodes = direct.filter((el) => !isConnection(el));
        const flows = direct.filter((el) => isConnection(el));
        const bodyParts = [
          ...nodes.map((n) => serializeNode(n, elements, 2)),
          ...flows.map((f) => serializeConnection(f, 2)),
        ];
        if (processEl?.opaqueInnerXml) {
          bodyParts.unshift(indentOpaqueInner(processEl.opaqueInnerXml, 2));
        }
        const body = bodyParts.join('\n');
        const opaque = formatOpaqueAttrs(processEl?.opaqueAttrs);
        const exec =
          processEl?.businessObject.isExecutable === false ? 'false' : 'true';
        return `  <bpmn:process id="${esc(processId)}" isExecutable="${exec}"${opaque}>
${body}
  </bpmn:process>`;
      })
      .join('\n');

    semanticXml = `  <bpmn:collaboration id="${esc(collaborationId)}">
${collabBody}
  </bpmn:collaboration>
${processesXml}`;
  } else {
    const processId = rootId;
    const processEl = elements[processId];
    const direct = childrenOf(elements, processId);
    const topNodes = direct.filter((el) => !isConnection(el));
    const topFlows = direct.filter((el) => isConnection(el));
    const bodyParts = [
      ...topNodes.map((n) => serializeNode(n, elements, 2)),
      ...topFlows.map((f) => serializeConnection(f, 2)),
    ];
    if (processEl?.opaqueInnerXml) {
      bodyParts.unshift(indentOpaqueInner(processEl.opaqueInnerXml, 2));
    }
    const body = bodyParts.join('\n');
    const opaque = formatOpaqueAttrs(processEl?.opaqueAttrs);
    const exec = processEl?.businessObject.isExecutable === false ? 'false' : 'true';
    semanticXml = `  <bpmn:process id="${esc(processId)}" isExecutable="${exec}"${opaque}>
${body}
  </bpmn:process>`;
  }

  const planeRef = rootId;
  const { shapes, edges } = serializeDi(
    all.filter((el) => el.type !== 'bpmn:Process' && el.type !== 'bpmn:Collaboration'),
  );

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
${formatDefinitionsOpen(definitionsId, state.definitionsAttrs)}
${semanticXml}
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="${esc(planeRef)}">
${shapes}
${edges}
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>
`;

  return { xml };
}
