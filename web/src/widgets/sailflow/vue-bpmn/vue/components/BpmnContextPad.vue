<script setup lang="ts">
import { computed } from 'vue';
import {
  getContextPadEntries,
  getReplaceOptions,
  isConnection,
  type DiagramSnapshot,
  type Point,
} from '@vue-bpmn/core';

const props = defineProps<{
  state: DiagramSnapshot;
  replaceOpen?: boolean;
  /** Convert diagram coords → host CSS pixels (required for correct overlay). */
  mapPoint?: (x: number, y: number) => Point;
}>();

const emit = defineEmits<{
  action: [key: string, type?: string];
  replace: [target: string];
  toggleReplace: [];
}>();

const selected = computed(() => {
  if (props.state.selection.length !== 1) return null;
  return props.state.elements[props.state.selection[0]!] ?? null;
});

const entries = computed(() =>
  selected.value ? getContextPadEntries(props.state, selected.value.id) : [],
);

const replaceOptions = computed(() =>
  selected.value ? getReplaceOptions(selected.value) : [],
);

function map(x: number, y: number): Point {
  return props.mapPoint?.(x, y) ?? { x, y };
}

const style = computed(() => {
  const el = selected.value;
  if (!el) return { display: 'none' as const };

  let anchorX: number;
  let anchorY: number;
  if (isConnection(el)) {
    const wp = el.waypoints?.[Math.floor((el.waypoints.length - 1) / 2)];
    anchorX = (wp?.x ?? 0) + 10;
    anchorY = (wp?.y ?? 0) - 10;
  } else {
    // Farther right so the pad is less likely to cover the next node.
    anchorX = el.di.x + el.di.width + 16;
    anchorY = el.di.y;
  }
  const host = map(anchorX, anchorY);
  return {
    left: `${host.x}px`,
    top: `${host.y}px`,
  };
});

function onClick(entry: { key: string; action: string; type?: string }) {
  if (entry.action === 'replace') {
    emit('toggleReplace');
    return;
  }
  emit('action', entry.key, entry.type);
}

function shortLabel(label: string): string {
  if (label.length <= 4) return label;
  return label.replace(/任务$/u, '').replace(/^追加\s*/u, '');
}
</script>

<template>
  <div v-if="selected && entries.length" class="vb-context-pad" :style="style">
    <button
      v-for="entry in entries"
      :key="entry.key"
      type="button"
      class="vb-context-pad-entry"
      :class="{ 'is-active': entry.action === 'replace' && replaceOpen }"
      :title="entry.label"
      :data-pad-key="entry.key"
      @click.stop="onClick(entry)"
    >
      {{ shortLabel(entry.label) }}
    </button>
    <div v-if="replaceOpen && replaceOptions.length" class="vb-replace-menu">
      <button
        v-for="opt in replaceOptions"
        :key="opt.key"
        type="button"
        class="vb-replace-option"
        :data-replace-target="opt.target"
        @click.stop="emit('replace', opt.target)"
      >
        {{ opt.label }}
      </button>
    </div>
  </div>
</template>
