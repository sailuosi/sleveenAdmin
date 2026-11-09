import type { DiagramSnapshot } from './types.js';

/**
 * Switch diagram root into a collapsed subprocess (C07-007).
 * Stores previous root on the subprocess businessObject for restore.
 */
export function drilldown(state: DiagramSnapshot, subprocessId: string): DiagramSnapshot | null {
  const el = state.elements[subprocessId];
  if (!el) return null;
  if (
    el.type !== 'bpmn:SubProcess' &&
    el.type !== 'bpmn:Transaction' &&
    el.type !== 'bpmn:AdHocSubProcess'
  ) {
    return null;
  }
  return {
    ...state,
    rootId: subprocessId,
    elements: {
      ...state.elements,
      [subprocessId]: {
        ...el,
        di: { ...el.di, isExpanded: true },
        businessObject: {
          ...el.businessObject,
          __previousRoot: state.rootId,
        },
      },
    },
    selection: [subprocessId],
  };
}

export function drillup(state: DiagramSnapshot): DiagramSnapshot | null {
  const root = state.rootId ? state.elements[state.rootId] : undefined;
  if (!root) return null;
  const previous = root.businessObject.__previousRoot as string | undefined | null;
  if (!previous) return null;
  const { __previousRoot: _, ...bo } = root.businessObject;
  return {
    ...state,
    rootId: previous,
    elements: {
      ...state.elements,
      [root.id]: {
        ...root,
        di: { ...root.di, isExpanded: false },
        businessObject: bo,
      },
    },
  };
}
