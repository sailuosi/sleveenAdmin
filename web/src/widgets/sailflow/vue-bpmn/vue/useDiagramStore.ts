import { computed, onUnmounted, shallowRef } from 'vue';
import {
  createDiagramStore,
  type DiagramSnapshot,
  type DiagramStore,
} from '@vue-bpmn/core';

/**
 * Vue binding for DiagramStore — UI always derives from store snapshots (D14).
 */
export function useDiagramStore(existing?: DiagramStore) {
  const store = existing ?? createDiagramStore();
  const state = shallowRef<DiagramSnapshot>(store.getState());

  const stop = store.subscribe((next) => {
    state.value = next;
  });

  onUnmounted(() => stop());

  return {
    store,
    state,
    selection: computed(() => state.value.selection),
    elements: computed(() => Object.values(state.value.elements)),
    activeTool: computed(() => state.value.activeTool),
    rootId: computed(() => state.value.rootId),
  };
}
