import type { AlignMode } from '@vue-bpmn/core';

export type DesignerAlignDir = 'left' | 'right' | 'top' | 'bottom' | 'center' | 'middle';

export type ModelingBridge = {
  updateProperties: (el: unknown, props: object) => void;
  updateModdleProperties?: (el: unknown, moddleEl: object, props: object) => void;
};

export type ModdleBridge = {
  create: (type: string, props?: object) => any;
};

/** Unified surface for SailDesignerToolbar + page shell (bpmn-js or vue-bpmn). */
export type ProcessDesignerEngine = {
  kind: 'bpmn-js' | 'vue-bpmn';
  importXML: (xml: string) => Promise<{ warnings?: Array<{ message?: string }> | string[] }>;
  exportXML: () => Promise<{ xml: string }>;
  exportSVG: () => Promise<{ svg: string }>;
  undo: () => void;
  redo: () => void;
  canUndo: () => boolean;
  canRedo: () => boolean;
  zoom: (level: number | 'fit-viewport') => void;
  getZoom: () => number;
  align: (dir: DesignerAlignDir) => void;
  destroy: () => void;
  /** Token simulation — only bpmn-js */
  toggleSimulation?: () => boolean;
  supportsSimulation: boolean;
  getModelingBridge: () => ModelingBridge;
  getModdleBridge: () => ModdleBridge;
  /** Open Sail full-config dialogs for current selection */
  openNodeConfig?: () => void;
  openTransitionConfig?: () => void;
  /** Subscribe to command stack / selection changes; returns unsubscribe */
  onChange: (fn: () => void) => () => void;
  /** bpmn-js raw modeler when kind === bpmn-js (toolbar legacy) */
  raw?: unknown;
};

export function alignDirToMode(dir: DesignerAlignDir): AlignMode {
  if (dir === 'center') return 'center';
  if (dir === 'middle') return 'middle';
  return dir;
}
