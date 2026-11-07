<template>
  <svg class="eb-spark" :viewBox="`0 0 ${w} ${h}`" preserveAspectRatio="none">
    <polyline
      fill="none"
      :stroke="color"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      :points="points"
    />
  </svg>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  values: { type: Array, default: () => [] },
  color: { type: String, default: '#4A90E2' },
  w: { type: Number, default: 80 },
  h: { type: Number, default: 28 }
});

const points = computed(() => {
  const vals = (props.values || []).map((v) => Number(v) || 0);
  if (!vals.length) return `0,${props.h} ${props.w},${props.h}`;
  const max = Math.max(...vals, 1);
  const min = Math.min(...vals, 0);
  const span = Math.max(max - min, 1);
  const step = vals.length > 1 ? props.w / (vals.length - 1) : props.w;
  return vals
    .map((v, i) => {
      const x = i * step;
      const y = props.h - ((v - min) / span) * (props.h - 4) - 2;
      return `${x},${y}`;
    })
    .join(' ');
});
</script>

<style scoped>
.eb-spark {
  width: 80px;
  height: 28px;
  display: block;
}
</style>
