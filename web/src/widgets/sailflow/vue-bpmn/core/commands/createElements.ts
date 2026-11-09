import type { Command, CommandContext, DiagramElement, DiagramSnapshot, Point } from '../types.js';
import { nextId } from '../ids.js';
import { defaultBoundsFor, idPrefixFor } from '../factories/shapeDefaults.js';

export interface CreateElementsPayload {
  type: string;
  position: Point;
  parentId?: string | null;
}

function blankElement(
  id: string,
  type: string,
  parentId: string | null,
  bounds: { x: number; y: number; width: number; height: number },
  options: {
    name?: string;
    businessObject?: Record<string, unknown>;
    isExpanded?: boolean | null;
  } = {},
): DiagramElement {
  return {
    id,
    type,
    parentId,
    name: options.name,
    businessObject: { ...(options.businessObject ?? {}) },
    di: {
      ...bounds,
      isExpanded:
        options.isExpanded !== undefined
          ? options.isExpanded
          : type === 'bpmn:SubProcess'
            ? true
            : null,
    },
    hostId: null,
    incoming: [],
    outgoing: [],
    waypoints: null,
    label: { kind: 'none' },
    children: [],
  };
}

function resolveParent(state: DiagramSnapshot, explicit?: string | null): string {
  if (explicit) return explicit;
  const root = state.rootId ?? 'Process_1';
  const rootEl = state.elements[root];
  if (rootEl?.type === 'bpmn:Collaboration') {
    // place into first process referenced by a participant
    const participant = Object.values(state.elements).find((e) => e.type === 'bpmn:Participant');
    const processRef = participant?.businessObject.processRef as string | undefined;
    if (processRef) return processRef;
  }
  return root;
}

/**
 * Create one palette shape (and nested start for expanded subprocess).
 * Participant creation upgrades a process root to collaboration when needed.
 */
export function buildCreateElements(
  state: DiagramSnapshot,
  payload: CreateElementsPayload,
): { elements: Record<string, DiagramElement>; rootId: string; selection: string[] } {
  const elements: Record<string, DiagramElement> = {};
  for (const [id, el] of Object.entries(state.elements)) {
    elements[id] = { ...el, businessObject: { ...el.businessObject } };
  }
  let rootId = state.rootId ?? 'Process_1';
  const position = payload.position;

  if (payload.type === 'bpmn:Participant') {
    const rootEl = elements[rootId];
    const isCollab = rootEl?.type === 'bpmn:Collaboration';

    if (!isCollab) {
      const processId = rootId;
      if (!elements[processId] || elements[processId]?.type !== 'bpmn:Process') {
        elements[processId] = blankElement(processId, 'bpmn:Process', null, {
          x: 0,
          y: 0,
          width: 0,
          height: 0,
        });
      } else {
        elements[processId] = { ...elements[processId]!, parentId: null };
      }

      const collabId = nextId({ ...state, elements }, 'Collaboration');
      const participantId = nextId({ ...state, elements }, 'Participant');
      elements[collabId] = blankElement(collabId, 'bpmn:Collaboration', null, {
        x: 0,
        y: 0,
        width: 0,
        height: 0,
      });
      const bounds = defaultBoundsFor('bpmn:Participant', position);
      elements[participantId] = blankElement(participantId, 'bpmn:Participant', collabId, bounds, {
        name: '池',
        businessObject: { processRef: processId },
      });
      rootId = collabId;
      return { elements, rootId, selection: [participantId] };
    }

    const processId = nextId({ ...state, elements }, 'Process');
    const participantId = nextId({ ...state, elements }, 'Participant');
    elements[processId] = blankElement(processId, 'bpmn:Process', null, {
      x: 0,
      y: 0,
      width: 0,
      height: 0,
    });
    const bounds = defaultBoundsFor('bpmn:Participant', position);
    elements[participantId] = blankElement(participantId, 'bpmn:Participant', rootId, bounds, {
      name: '池',
      businessObject: { processRef: processId },
    });
    return { elements, rootId, selection: [participantId] };
  }

  const parentId = resolveParent(state, payload.parentId);
  const id = nextId({ ...state, elements }, idPrefixFor(payload.type));
  const bounds = defaultBoundsFor(payload.type, position);
  elements[id] = blankElement(id, payload.type, parentId, bounds);

  if (payload.type === 'bpmn:SubProcess') {
    const startId = nextId({ ...state, elements }, 'StartEvent');
    const startBounds = defaultBoundsFor('bpmn:StartEvent', {
      x: bounds.x + 40,
      y: bounds.y + bounds.height / 2,
    });
    elements[startId] = blankElement(startId, 'bpmn:StartEvent', id, startBounds);
  }

  return { elements, rootId, selection: [id] };
}

export function createCreateElementsCommand(payload: CreateElementsPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'elements.create',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        const built = buildCreateElements(before, payload);
        after = {
          ...before,
          rootId: built.rootId,
          elements: built.elements,
          selection: built.selection,
          activeTool: null,
        };
      }
      ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
