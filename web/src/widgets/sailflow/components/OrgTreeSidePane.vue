<template>
  <div class="sail-org-side" :class="{ 'is-collapsed': !expanded }" :style="sideStyle">
    <button v-if="!expanded" type="button" class="sail-org-expand" title="展开组织树" @click="toggle">
      »
    </button>
    <div v-else class="sail-org-panel">
      <div class="sail-org-card">
        <div class="sail-org-body">
          <div class="sail-org-toolbar">
            <span class="sail-org-title">组织筛选</span>
            <button type="button" class="sail-org-collapse" title="收起组织树" @click="toggle">«</button>
          </div>
          <slot />
        </div>
      </div>
      <div class="sail-org-resizer" title="拖拽调整宽度" @mousedown.prevent="startResize" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue';

defineOptions({ name: 'SailOrgTreeSidePane' });

const props = withDefaults(
  defineProps<{
    storageKey?: string;
    defaultWidth?: number;
    minWidth?: number;
    maxWidth?: number;
  }>(),
  {
    storageKey: 'default',
    defaultWidth: 300,
    minWidth: 220,
    maxWidth: 480
  }
);

const widthKey = computed(() => `sail.orgTree.width.${props.storageKey}`);

function readWidth() {
  try {
    const raw = localStorage.getItem(widthKey.value);
    const n = raw ? Number(raw) : NaN;
    if (Number.isFinite(n)) return Math.min(props.maxWidth, Math.max(props.minWidth, n));
  } catch {
    /* ignore */
  }
  return props.defaultWidth;
}

const expanded = ref(false);
const width = ref(readWidth());

const sideStyle = computed(() =>
  expanded.value
    ? { width: `${width.value}px`, flex: `0 0 ${width.value}px` }
    : { width: '20px', flex: '0 0 20px' }
);

function toggle() {
  expanded.value = !expanded.value;
}

let resizing = false;
let startX = 0;
let startW = 0;

function onMove(e: MouseEvent) {
  if (!resizing) return;
  width.value = Math.min(props.maxWidth, Math.max(props.minWidth, startW + (e.clientX - startX)));
}

function stopResize() {
  if (!resizing) return;
  resizing = false;
  document.body.style.cursor = '';
  document.body.style.userSelect = '';
  window.removeEventListener('mousemove', onMove);
  window.removeEventListener('mouseup', stopResize);
  try {
    localStorage.setItem(widthKey.value, String(width.value));
  } catch {
    /* ignore */
  }
}

function startResize(e: MouseEvent) {
  resizing = true;
  startX = e.clientX;
  startW = width.value;
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
  window.addEventListener('mousemove', onMove);
  window.addEventListener('mouseup', stopResize);
}

onBeforeUnmount(stopResize);
</script>

<style scoped>
.sail-org-side {
  display: flex;
  flex-direction: row;
  align-items: stretch;
  min-width: 0;
  min-height: 0;
  height: 100%;
  align-self: stretch;
}
.sail-org-expand {
  display: inline-flex;
  align-items: flex-start;
  justify-content: center;
  width: 100%;
  height: 100%;
  padding-top: 12px;
  border: none;
  background: transparent;
  color: var(--el-text-color-secondary);
  cursor: pointer;
}
.sail-org-expand:hover {
  color: var(--el-color-primary);
}
.sail-org-panel {
  position: relative;
  display: flex;
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  height: 100%;
  padding-right: 6px;
  box-sizing: border-box;
}
.sail-org-card {
  flex: 1 1 auto;
  width: 100%;
  min-width: 0;
  min-height: 0;
  height: 100%;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 4px;
  background: var(--el-bg-color);
  box-sizing: border-box;
}
.sail-org-body {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  padding: 10px;
  box-sizing: border-box;
}
.sail-org-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
  flex: 0 0 auto;
}
.sail-org-title {
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.sail-org-collapse {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  background: var(--el-bg-color);
  cursor: pointer;
}
.sail-org-resizer {
  position: absolute;
  top: 0;
  right: 0;
  z-index: 3;
  width: 6px;
  height: 100%;
  cursor: col-resize;
}
.sail-org-resizer:hover,
.sail-org-resizer:active {
  background: var(--el-color-primary-light-7);
}
</style>
