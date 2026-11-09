<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue';
import type { DiagramElement, Point } from '@vue-bpmn/core';

const props = defineProps<{
  element: DiagramElement | null;
  active: boolean;
  mapPoint?: (x: number, y: number) => Point;
}>();

const emit = defineEmits<{
  commit: [text: string];
  cancel: [];
}>();

const input = ref<HTMLInputElement | null>(null);
const text = ref('');

function map(x: number, y: number): Point {
  return props.mapPoint?.(x, y) ?? { x, y };
}

const style = computed(() => {
  const el = props.element;
  if (!el) return { display: 'none' };
  if (el.label?.kind === 'external' && el.label.bounds) {
    const b = el.label.bounds;
    const p = map(b.x, b.y);
    const q = map(b.x + Math.max(80, b.width), b.y);
    return {
      left: `${p.x}px`,
      top: `${p.y}px`,
      width: `${Math.max(80, q.x - p.x)}px`,
    };
  }
  const p = map(el.di.x, el.di.y + el.di.height / 2 - 12);
  const q = map(el.di.x + Math.max(80, el.di.width), el.di.y);
  return {
    left: `${p.x}px`,
    top: `${p.y}px`,
    width: `${Math.max(80, q.x - p.x)}px`,
  };
});

watch(
  () => [props.active, props.element?.id] as const,
  async ([active]) => {
    if (!active || !props.element) return;
    text.value = props.element.name ?? '';
    await nextTick();
    input.value?.focus();
    input.value?.select();
  },
);

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    event.preventDefault();
    emit('commit', text.value);
  } else if (event.key === 'Escape') {
    event.preventDefault();
    emit('cancel');
  }
}
</script>

<template>
  <input
    v-if="active && element"
    ref="input"
    class="vb-direct-editing"
    :style="style"
    :value="text"
    @input="text = ($event.target as HTMLInputElement).value"
    @keydown="onKeydown"
    @blur="emit('commit', text)"
  />
</template>
