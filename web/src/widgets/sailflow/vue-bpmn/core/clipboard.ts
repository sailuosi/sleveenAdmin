import type { DiagramElement, DiagramSnapshot, Point } from './types.js';
import { nextId } from './ids.js';
import { idPrefixFor } from './factories/shapeDefaults.js';
import { isConnection } from './xml/elementKind.js';
import { applyDelete } from './commands/deleteElements.js';

export interface ClipboardPayload {
  elements: Record<string, DiagramElement>;
  rootIds: string[];
}

let clipboard: ClipboardPayload | null = null;

export function getClipboard(): ClipboardPayload | null {
  return clipboard ? structuredClone(clipboard) : null;
}

export function setClipboard(payload: ClipboardPayload | null) {
  clipboard = payload ? structuredClone(payload) : null;
}

export function copySelection(state: DiagramSnapshot): ClipboardPayload | null {
  const ids = state.selection.filter((id) => state.elements[id]);
  if (!ids.length) return null;
  const selected = new Set(ids);
  for (const el of Object.values(state.elements)) {
    if (el.parentId && selected.has(el.parentId)) selected.add(el.id);
  }
  for (const el of Object.values(state.elements)) {
    if (!isConnection(el)) continue;
    const s = el.businessObject.sourceRef as string | undefined;
    const t = el.businessObject.targetRef as string | undefined;
    if (s && t && selected.has(s) && selected.has(t)) selected.add(el.id);
  }
  const elements: Record<string, DiagramElement> = {};
  for (const id of selected) {
    elements[id] = structuredClone(state.elements[id]!);
  }
  const payload = { elements, rootIds: ids };
  setClipboard(payload);
  return payload;
}

function allocateIds(
  state: DiagramSnapshot,
  payload: ClipboardPayload,
): Map<string, string> {
  const idMap = new Map<string, string>();
  const occupied = new Set(Object.keys(state.elements));
  let counter = 0;
  for (const [oldId, el] of Object.entries(payload.elements)) {
    const prefix = idPrefixFor(el.type);
    let candidate = nextId(state, prefix);
    while (occupied.has(candidate) || [...idMap.values()].includes(candidate)) {
      counter += 1;
      candidate = `${prefix}_${Object.keys(state.elements).length + counter + 1}`;
    }
    if (candidate === oldId) {
      counter += 1;
      candidate = `${prefix}_copy_${counter}`;
    }
    occupied.add(candidate);
    idMap.set(oldId, candidate);
  }
  return idMap;
}

export function buildPaste(
  state: DiagramSnapshot,
  payload: ClipboardPayload,
  offset: Point = { x: 40, y: 40 },
): { state: DiagramSnapshot; newIds: string[] } {
  const idMap = allocateIds(state, payload);
  const elements = { ...state.elements };

  for (const [oldId, el] of Object.entries(payload.elements)) {
    const newId = idMap.get(oldId)!;
    const parentId =
      el.parentId && idMap.has(el.parentId)
        ? idMap.get(el.parentId)!
        : el.parentId && state.elements[el.parentId]
          ? el.parentId
          : state.rootId;

    const cloned: DiagramElement = {
      ...structuredClone(el),
      id: newId,
      parentId,
      hostId: el.hostId && idMap.has(el.hostId) ? idMap.get(el.hostId)! : el.hostId,
      incoming: [],
      outgoing: [],
      di: {
        ...el.di,
        x: el.di.x + offset.x,
        y: el.di.y + offset.y,
      },
      waypoints: el.waypoints
        ? el.waypoints.map((p) => ({ x: p.x + offset.x, y: p.y + offset.y }))
        : el.waypoints,
      label:
        el.label?.kind === 'external' && el.label.bounds
          ? {
              ...el.label,
              bounds: {
                ...el.label.bounds,
                x: el.label.bounds.x + offset.x,
                y: el.label.bounds.y + offset.y,
              },
            }
          : structuredClone(el.label),
      businessObject: { ...el.businessObject },
    };

    if (cloned.businessObject.sourceRef && idMap.has(String(cloned.businessObject.sourceRef))) {
      cloned.businessObject.sourceRef = idMap.get(String(cloned.businessObject.sourceRef));
    }
    if (cloned.businessObject.targetRef && idMap.has(String(cloned.businessObject.targetRef))) {
      cloned.businessObject.targetRef = idMap.get(String(cloned.businessObject.targetRef));
    }

    elements[newId] = cloned;
  }

  for (const el of Object.values(elements)) {
    if (!isConnection(el)) continue;
    const s = el.businessObject.sourceRef as string | undefined;
    const t = el.businessObject.targetRef as string | undefined;
    if (s && elements[s] && !elements[s]!.outgoing.includes(el.id)) {
      elements[s] = {
        ...elements[s]!,
        outgoing: [...elements[s]!.outgoing, el.id],
      };
    }
    if (t && elements[t] && !elements[t]!.incoming.includes(el.id)) {
      elements[t] = {
        ...elements[t]!,
        incoming: [...elements[t]!.incoming, el.id],
      };
    }
  }

  const rootNewIds = payload.rootIds
    .map((id) => idMap.get(id))
    .filter((id): id is string => Boolean(id));

  return {
    state: {
      ...state,
      elements,
      selection: rootNewIds,
    },
    newIds: rootNewIds,
  };
}

export function buildCut(state: DiagramSnapshot): {
  clipboard: ClipboardPayload | null;
  state: DiagramSnapshot;
} {
  const payload = copySelection(state);
  if (!payload) return { clipboard: null, state };
  return {
    clipboard: payload,
    state: applyDelete(state, payload.rootIds),
  };
}

export function buildDuplicate(
  state: DiagramSnapshot,
  offset: Point = { x: 40, y: 40 },
): DiagramSnapshot | null {
  const payload = copySelection(state);
  if (!payload) return null;
  return buildPaste(state, payload, offset).state;
}
