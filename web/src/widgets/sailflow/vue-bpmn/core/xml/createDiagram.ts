import type { DiagramElement, DiagramSnapshot } from '../types.js';

/** Empty modeler diagram: one process + none start event (C01-004). */
export function createEmptyDiagram(): DiagramSnapshot {
  const start: DiagramElement = {
    id: 'StartEvent_1',
    type: 'bpmn:StartEvent',
    parentId: 'Process_1',
    name: undefined,
    businessObject: {},
    di: { x: 180, y: 160, width: 36, height: 36, isExpanded: null },
    hostId: null,
    incoming: [],
    outgoing: [],
    waypoints: null,
    label: { kind: 'none' },
    children: [],
  };

  return {
    rootId: 'Process_1',
    definitionsId: 'Definitions_1',
    elements: { [start.id]: start },
    selection: [],
    activeTool: null,
    viewbox: { x: 0, y: 0, width: 1000, height: 600, scale: 1 },
  };
}
