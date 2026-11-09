import type { Command, CommandContext, DiagramElement, DiagramSnapshot } from '../types.js';
import { nextId } from '../ids.js';

export type LaneAction =
  | 'lane-insert-above'
  | 'lane-insert-below'
  | 'lane-divide-two'
  | 'lane-divide-three';

export interface LaneCommandPayload {
  laneId: string;
  action: LaneAction;
}

function siblingLanes(state: DiagramSnapshot, lane: DiagramElement): DiagramElement[] {
  const parentId = lane.parentId;
  return Object.values(state.elements)
    .filter((e) => e.type === 'bpmn:Lane' && e.parentId === parentId)
    .sort((a, b) => a.di.y - b.di.y);
}

function blankLane(
  id: string,
  parentId: string | null,
  bounds: { x: number; y: number; width: number; height: number },
  name: string,
): DiagramElement {
  return {
    id,
    type: 'bpmn:Lane',
    parentId,
    name,
    businessObject: { name },
    di: { ...bounds, isExpanded: null },
    hostId: null,
    incoming: [],
    outgoing: [],
    waypoints: null,
    label: { kind: 'none' },
    children: [],
  };
}

export function buildLaneAction(
  state: DiagramSnapshot,
  payload: LaneCommandPayload,
): DiagramSnapshot | null {
  const lane = state.elements[payload.laneId];
  if (!lane || lane.type !== 'bpmn:Lane') return null;

  const elements = { ...state.elements };
  const lanes = siblingLanes(state, lane);

  if (payload.action === 'lane-insert-below' || payload.action === 'lane-insert-above') {
    const height = Math.max(60, Math.floor(lane.di.height));
    const insertBelow = payload.action === 'lane-insert-below';
    const y = insertBelow ? lane.di.y + lane.di.height : lane.di.y;
    const id = nextId(state, 'Lane');
    const created = blankLane(
      id,
      lane.parentId,
      { x: lane.di.x, y, width: lane.di.width, height },
      id,
    );

    // Shift lanes at/after insertion point
    for (const sibling of lanes) {
      if (insertBelow ? sibling.di.y > lane.di.y : sibling.di.y >= lane.di.y) {
        if (sibling.id === lane.id && insertBelow) continue;
        if (sibling.id === lane.id && !insertBelow) {
          elements[sibling.id] = {
            ...sibling,
            di: { ...sibling.di, y: sibling.di.y + height },
          };
          continue;
        }
        if (insertBelow && sibling.di.y >= lane.di.y + lane.di.height) {
          elements[sibling.id] = {
            ...sibling,
            di: { ...sibling.di, y: sibling.di.y + height },
          };
        } else if (!insertBelow && sibling.id !== lane.id && sibling.di.y >= lane.di.y) {
          elements[sibling.id] = {
            ...sibling,
            di: { ...sibling.di, y: sibling.di.y + height },
          };
        }
      }
    }
    if (!insertBelow) {
      elements[lane.id] = {
        ...lane,
        di: { ...lane.di, y: lane.di.y + height },
      };
    }
    elements[id] = created;

    // Grow participant/process height if present
    const parent = lane.parentId ? elements[lane.parentId] : undefined;
    if (parent && (parent.type === 'bpmn:Participant' || parent.type === 'bpmn:Process')) {
      elements[parent.id] = {
        ...parent,
        di: { ...parent.di, height: parent.di.height + height },
      };
    }

    return { ...state, elements, selection: [id] };
  }

  const parts = payload.action === 'lane-divide-three' ? 3 : 2;
  const totalHeight = lane.di.height;
  const partHeight = Math.max(40, Math.floor(totalHeight / parts));
  const ids: string[] = [lane.id];

  elements[lane.id] = {
    ...lane,
    di: { ...lane.di, height: partHeight },
  };

  for (let i = 1; i < parts; i += 1) {
    const id = nextId({ ...state, elements }, 'Lane');
    elements[id] = blankLane(
      id,
      lane.parentId,
      {
        x: lane.di.x,
        y: lane.di.y + partHeight * i,
        width: lane.di.width,
        height: i === parts - 1 ? totalHeight - partHeight * (parts - 1) : partHeight,
      },
      id,
    );
    ids.push(id);
  }

  return { ...state, elements, selection: [ids[ids.length - 1]!] };
}

export function createLaneCommand(payload: LaneCommandPayload): Command {
  let before: DiagramSnapshot | null = null;
  let after: DiagramSnapshot | null = null;

  return {
    name: 'lane.action',
    execute(ctx: CommandContext) {
      if (!after) {
        before = structuredClone(ctx.store.getState());
        after = buildLaneAction(before, payload);
      }
      if (after) ctx.store.replaceState(structuredClone(after));
    },
    revert(ctx: CommandContext) {
      if (before) ctx.store.replaceState(structuredClone(before));
    },
  };
}
