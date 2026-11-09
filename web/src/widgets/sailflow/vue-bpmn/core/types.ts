export type BpmnType = `bpmn:${string}`;

export interface Point {
  x: number;
  y: number;
}

export interface Bounds {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface LabelInfo {
  kind: 'internal' | 'external' | 'none';
  bounds?: Bounds;
  text?: string;
}

export interface DiagramElement {
  id: string;
  type: BpmnType | string;
  parentId: string | null;
  name?: string;
  businessObject: Record<string, unknown>;
  di: Bounds & { isExpanded?: boolean | null };
  hostId?: string | null;
  incoming: string[];
  outgoing: string[];
  waypoints?: Point[] | null;
  label?: LabelInfo;
  children?: string[];
  /** Non-core attributes preserved across import/export (e.g. sf:code) */
  opaqueAttrs?: Record<string, string>;
  /** Child XML not modeled structurally (extensionElements, documentation, …) */
  opaqueInnerXml?: string;
}

export interface DiagramSnapshot {
  rootId: string | null;
  definitionsId: string | null;
  /** Extra definitions attrs (xmlns:sf, targetNamespace, …) */
  definitionsAttrs?: Record<string, string>;
  elements: Record<string, DiagramElement>;
  selection: string[];
  activeTool: string | null;
  viewbox: { x: number; y: number; width: number; height: number; scale: number };
}

export interface Command {
  name: string;
  execute: (ctx: CommandContext) => void;
  revert: (ctx: CommandContext) => void;
}

export interface CommandContext {
  store: {
    getState: () => DiagramSnapshot;
    replaceState: (next: DiagramSnapshot) => void;
    patchState: (patch: Partial<DiagramSnapshot>) => void;
  };
  payload?: unknown;
}

export type StoreListener = (state: DiagramSnapshot) => void;
