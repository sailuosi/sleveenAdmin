<script setup lang="ts">
import { computed } from 'vue';
import { getPaletteEntries } from '@vue-bpmn/core';

const props = defineProps<{
  activeTool?: string | null;
  pendingCreate?: string | null;
}>();

const emit = defineEmits<{
  tool: [tool: string];
  create: [type: string];
}>();

const entries = getPaletteEntries();
const tools = computed(() => entries.filter((e) => e.group === 'tools'));
const creates = computed(() => entries.filter((e) => e.group === 'create'));
</script>

<template>
  <aside class="vb-palette" aria-label="工具面板">
    <div class="vb-palette-group">
      <button
        v-for="item in tools"
        :key="item.key"
        type="button"
        class="vb-palette-entry"
        :class="{ 'is-active': activeTool === item.tool }"
        :title="item.label"
        :data-palette-key="item.key"
        @click="emit('tool', item.tool!)"
      >
        {{ item.label }}
      </button>
    </div>
    <hr />
    <div class="vb-palette-group">
      <button
        v-for="item in creates"
        :key="item.key"
        type="button"
        class="vb-palette-entry"
        :class="{ 'is-active': pendingCreate === item.type }"
        :title="item.label"
        :data-palette-key="item.key"
        @click="emit('create', item.type!)"
      >
        {{ item.label }}
      </button>
    </div>
  </aside>
</template>
