import type { DiagramElement, DiagramSnapshot, StoreListener } from './types.js';
import { CommandStack } from './commandStack.js';

const DEFAULT_VIEWBOX = { x: 0, y: 0, width: 1000, height: 600, scale: 1 };

function emptyState(): DiagramSnapshot {
  return {
    rootId: null,
    definitionsId: null,
    elements: {},
    selection: [],
    activeTool: null,
    viewbox: { ...DEFAULT_VIEWBOX },
  };
}

function cloneState(state: DiagramSnapshot): DiagramSnapshot {
  return structuredClone(state);
}

/**
 * Single source of truth for diagram UI + model (D14).
 * Vue layers should subscribe; they must not keep parallel element maps.
 */
export class DiagramStore {
  private state: DiagramSnapshot = emptyState();
  private listeners = new Set<StoreListener>();
  readonly commandStack = new CommandStack();

  constructor() {
    this.commandStack.onChange(() => this.emit());
  }

  getState(): DiagramSnapshot {
    return this.state;
  }

  /** Immutable snapshot for comparisons / tests. */
  snapshot(): DiagramSnapshot {
    return cloneState(this.state);
  }

  subscribe(listener: StoreListener): () => void {
    this.listeners.add(listener);
    listener(this.state);
    return () => this.listeners.delete(listener);
  }

  replaceState(next: DiagramSnapshot): void {
    this.state = next;
    this.emit();
  }

  patchState(patch: Partial<DiagramSnapshot>): void {
    this.state = { ...this.state, ...patch };
    this.emit();
  }

  reset(): void {
    this.commandStack.clear();
    this.state = emptyState();
    this.emit();
  }

  setSelection(ids: string[]): void {
    const unique = [...new Set(ids)].filter((id) => id in this.state.elements || id === this.state.rootId);
    this.patchState({ selection: unique });
  }

  clearSelection(): void {
    this.patchState({ selection: [] });
  }

  setActiveTool(tool: string | null): void {
    this.patchState({ activeTool: tool });
  }

  getElement(id: string): DiagramElement | undefined {
    return this.state.elements[id];
  }

  listElements(): DiagramElement[] {
    return Object.values(this.state.elements);
  }

  /**
   * Direct store mutation for bootstrapping / import.
   * Interactive edits should prefer commandStack.execute.
   */
  loadElements(input: {
    rootId: string;
    definitionsId?: string | null;
    definitionsAttrs?: Record<string, string>;
    elements: Record<string, DiagramElement>;
  }): void {
    this.commandStack.clear();
    this.replaceState({
      ...emptyState(),
      rootId: input.rootId,
      definitionsId: input.definitionsId ?? null,
      definitionsAttrs: input.definitionsAttrs,
      elements: input.elements,
    });
  }

  updateElement(id: string, patch: Partial<DiagramElement>): void {
    const current = this.state.elements[id];
    if (!current) return;
    const next = {
      ...this.state.elements,
      [id]: { ...current, ...patch, businessObject: { ...current.businessObject, ...(patch.businessObject ?? {}) } },
    };
    this.patchState({ elements: next });
  }

  private emit() {
    for (const listener of this.listeners) listener(this.state);
  }
}

export function createDiagramStore(): DiagramStore {
  return new DiagramStore();
}
