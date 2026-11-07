<script setup lang="ts">
import { computed } from 'vue';

defineOptions({
  name: 'AppWatermark'
});

const props = withDefaults(
  defineProps<{
    content?: string;
    fontSize?: number;
    width?: number;
    height?: number;
    rotate?: number;
    fontColor?: string;
    zIndex?: number;
  }>(),
  {
    content: '',
    fontSize: 14,
    width: 340,
    height: 160,
    rotate: -15,
    fontColor: 'rgba(128, 128, 128, .08)',
    zIndex: 9999
  }
);

const backgroundImage = computed(() => {
  const lines = String(props.content || '')
    .split('\n')
    .filter(Boolean);
  if (!lines.length) return '';

  const w = props.width;
  const h = props.height;
  const fs = props.fontSize;
  const lineHeight = fs + 6;
  const startY = (h - lines.length * lineHeight) / 2 + fs;

  const textNodes = lines
    .map(
      (line, i) =>
        `<text x="50%" y="${startY + i * lineHeight}" text-anchor="middle" fill="${props.fontColor}" font-size="${fs}" font-family="sans-serif">${escapeXml(line)}</text>`
    )
    .join('');

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}"><g transform="rotate(${props.rotate} ${w / 2} ${h / 2})">${textNodes}</g></svg>`;
  return `url("data:image/svg+xml,${encodeURIComponent(svg)}")`;
});

function escapeXml(s: string) {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}
</script>

<template>
  <div
    v-if="content"
    class="app-watermark"
    :style="{
      zIndex,
      backgroundImage,
      backgroundSize: `${width}px ${height}px`
    }"
  />
</template>

<style scoped>
.app-watermark {
  pointer-events: none;
  position: fixed;
  inset: 0;
  background-repeat: repeat;
}
</style>
