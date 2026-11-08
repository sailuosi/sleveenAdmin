<script setup lang="ts">
import { computed, onUnmounted, reactive, ref, watch } from 'vue';
import { useRouteStore } from '@/store/modules/route';
import { useThemeStore } from '@/store/modules/theme';
import type { MenuWithMeta } from '../utils/megaMenuSections';
import BottomDockItem from './bottom-dock-item.vue';

defineOptions({
  name: 'BottomDockMenu'
});

const CHROME_INSET = 16;
const DOCK_HEIGHT = 56;
const SNAP_THRESHOLD = 56;
const CLICK_SLOP = 4;
const MAGNIFY_RANGE = 88;
const MAGNIFY_MAX = 1.55;

const routeStore = useRouteStore();
const themeStore = useThemeStore();

const dockRef = ref<HTMLElement | null>(null);
const itemRefs = ref<(HTMLElement | null)[]>([]);

const menus = computed(() => routeStore.menus as MenuWithMeta[]);

const defaultDock = {
  snapped: true,
  offsetX: 0,
  offsetY: 96,
  collapsed: false
};

const dock = computed(() => themeStore.layout.bottomDock ?? defaultDock);

const dragging = ref(false);
const showSnapHint = ref(false);
const hoverIndex = ref(-1);
const scales = ref<number[]>([]);

/** Live geometry while dragging (before commit) */
const live = reactive({
  active: false,
  snappedPreview: false,
  offsetX: 0,
  offsetY: 96
});

const visuallySnapped = computed(() => {
  if (live.active) return live.snappedPreview;
  return dock.value.snapped;
});

const dockStyle = computed(() => {
  if (visuallySnapped.value) {
    return {
      left: `${CHROME_INSET}px`,
      right: `${CHROME_INSET}px`,
      bottom: `${CHROME_INSET}px`,
      transform: 'none',
      width: 'auto',
      height: `${DOCK_HEIGHT}px`
    };
  }

  const x = live.active ? live.offsetX : dock.value.offsetX;
  const y = live.active ? live.offsetY : dock.value.offsetY;

  return {
    left: `calc(50% + ${x}px)`,
    right: 'auto',
    bottom: `${y}px`,
    transform: 'translateX(-50%)',
    width: 'auto',
    height: `${DOCK_HEIGHT}px`
  };
});

function setItemRef(el: Element | null, index: number) {
  itemRefs.value[index] = el as HTMLElement | null;
}

function resetScales() {
  scales.value = menus.value.map(() => 1);
  hoverIndex.value = -1;
}

function computeScale(distance: number) {
  if (distance >= MAGNIFY_RANGE) return 1;
  const t = 1 - distance / MAGNIFY_RANGE;
  return 1 + (MAGNIFY_MAX - 1) * t * t;
}

function onDockPointerMove(event: PointerEvent) {
  if (dragging.value || dock.value.collapsed) {
    resetScales();
    return;
  }

  const next: number[] = [];
  let best = -1;
  let bestDist = Number.POSITIVE_INFINITY;

  menus.value.forEach((_, index) => {
    const el = itemRefs.value[index];
    if (!el) {
      next[index] = 1;
      return;
    }
    const rect = el.getBoundingClientRect();
    const cx = rect.left + rect.width / 2;
    const cy = rect.top + rect.height / 2;
    const dist = Math.hypot(event.clientX - cx, event.clientY - cy);
    next[index] = computeScale(dist);
    if (dist < bestDist) {
      bestDist = dist;
      best = index;
    }
  });

  scales.value = next;
  hoverIndex.value = bestDist < MAGNIFY_RANGE ? best : -1;
}

function onDockPointerLeave() {
  if (!dragging.value) resetScales();
}

/** Drag handle state */
let dragPointerId = -1;
let dragStartX = 0;
let dragStartY = 0;
let originOffsetX = 0;
let originOffsetY = 0;
let moved = false;
let startedFromSnapped = false;
let dockWidthAtStart = 0;

function ensureBottomDock() {
  if (!themeStore.layout.bottomDock) {
    themeStore.layout.bottomDock = {
      snapped: true,
      offsetX: 0,
      offsetY: 96,
      collapsed: false
    };
  }
  return themeStore.layout.bottomDock;
}

function clampFloat(x: number, y: number) {
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  const halfW = Math.max(dockWidthAtStart, 64) / 2;
  const minX = -vw / 2 + halfW + 8;
  const maxX = vw / 2 - halfW - 8;
  const minY = CHROME_INSET;
  const maxY = vh - DOCK_HEIGHT - 8;
  return {
    offsetX: Math.min(maxX, Math.max(minX, x)),
    offsetY: Math.min(maxY, Math.max(minY, y))
  };
}

function nearBottom(bottomPx: number) {
  return bottomPx <= SNAP_THRESHOLD;
}

function onHandlePointerDown(event: PointerEvent) {
  if (event.button !== 0) return;
  event.preventDefault();
  event.stopPropagation();

  ensureBottomDock();
  const state = dock.value;

  dragPointerId = event.pointerId;
  dragStartX = event.clientX;
  dragStartY = event.clientY;
  moved = false;
  startedFromSnapped = state.snapped;
  dragging.value = true;
  resetScales();

  const el = dockRef.value;
  dockWidthAtStart = el?.getBoundingClientRect().width ?? 200;

  if (state.snapped && el) {
    const rect = el.getBoundingClientRect();
    originOffsetX = rect.left + rect.width / 2 - window.innerWidth / 2;
    originOffsetY = window.innerHeight - rect.bottom;
  } else {
    originOffsetX = state.offsetX;
    originOffsetY = state.offsetY;
  }

  live.active = true;
  live.snappedPreview = false;
  live.offsetX = originOffsetX;
  live.offsetY = originOffsetY;
  showSnapHint.value = nearBottom(originOffsetY);

  (event.currentTarget as HTMLElement).setPointerCapture?.(event.pointerId);
  window.addEventListener('pointermove', onHandlePointerMove);
  window.addEventListener('pointerup', onHandlePointerUp);
  window.addEventListener('pointercancel', onHandlePointerUp);
}

function onHandlePointerMove(event: PointerEvent) {
  if (event.pointerId !== dragPointerId) return;

  const dx = event.clientX - dragStartX;
  const dy = event.clientY - dragStartY;
  if (!moved && Math.hypot(dx, dy) >= CLICK_SLOP) {
    moved = true;
  }
  if (!moved) return;

  const next = clampFloat(originOffsetX + dx, originOffsetY - dy);
  live.offsetX = next.offsetX;
  live.offsetY = next.offsetY;

  const snap = nearBottom(next.offsetY);
  live.snappedPreview = snap;
  showSnapHint.value = snap;
}

function onHandlePointerUp(event: PointerEvent) {
  if (dragPointerId !== -1 && event.pointerId !== dragPointerId) return;

  window.removeEventListener('pointermove', onHandlePointerMove);
  window.removeEventListener('pointerup', onHandlePointerUp);
  window.removeEventListener('pointercancel', onHandlePointerUp);

  const state = ensureBottomDock();
  dragging.value = false;
  showSnapHint.value = false;
  dragPointerId = -1;

  if (!moved) {
    live.active = false;
    live.snappedPreview = false;
    state.collapsed = !state.collapsed;
    return;
  }

  if (nearBottom(live.offsetY)) {
    state.snapped = true;
    state.offsetX = 0;
    state.offsetY = 96;
  } else {
    state.snapped = false;
    state.offsetX = live.offsetX;
    state.offsetY = live.offsetY;
  }

  live.active = false;
  live.snappedPreview = false;
  void startedFromSnapped;
}

onUnmounted(() => {
  window.removeEventListener('pointermove', onHandlePointerMove);
  window.removeEventListener('pointerup', onHandlePointerUp);
  window.removeEventListener('pointercancel', onHandlePointerUp);
});

watch(
  menus,
  () => {
    itemRefs.value = [];
    resetScales();
  },
  { immediate: true }
);
</script>

<template>
  <Teleport to="body">
    <div
      v-show="showSnapHint"
      class="dock-snap-hint"
      :style="{
        left: `${CHROME_INSET}px`,
        right: `${CHROME_INSET}px`,
        bottom: `${CHROME_INSET}px`,
        height: `${DOCK_HEIGHT}px`
      }"
    />

    <div
      ref="dockRef"
      class="bottom-dock"
      :class="{
        'bottom-dock--snapped': visuallySnapped,
        'bottom-dock--floating': !visuallySnapped,
        'bottom-dock--dragging': dragging,
        'bottom-dock--collapsed': dock.collapsed
      }"
      :style="dockStyle"
      @pointermove="onDockPointerMove"
      @pointerleave="onDockPointerLeave"
    >
      <div class="bottom-dock-glass">
        <button
          type="button"
          class="dock-handle"
          title="拖动停靠栏 / 点击收起"
          @pointerdown="onHandlePointerDown"
        >
          <SvgIcon icon="ph:squares-four" class="dock-handle-icon" />
        </button>

        <Transition name="dock-collapse">
          <div v-show="!dock.collapsed" class="dock-items">
            <div
              v-for="(menu, index) in menus"
              :key="menu.key"
              :ref="el => setItemRef(el as Element | null, index)"
              class="dock-item-slot"
            >
              <BottomDockItem
                :menu="menu"
                :scale="scales[index] ?? 1"
                :hovered="hoverIndex === index"
              />
            </div>
          </div>
        </Transition>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.dock-snap-hint {
  position: fixed;
  z-index: 1990;
  border-radius: 16px;
  border: 2px dashed rgb(var(--primary-color) / 0.55);
  background: rgb(var(--primary-color) / 0.12);
  box-shadow: 0 0 0 1px rgb(var(--primary-color) / 0.08) inset;
  pointer-events: none;
  animation: dock-hint-pulse 1.1s ease-in-out infinite;
}

@keyframes dock-hint-pulse {
  0%,
  100% {
    opacity: 0.55;
  }
  50% {
    opacity: 0.9;
  }
}

.bottom-dock {
  position: fixed;
  z-index: 2000;
  pointer-events: none;
  transition:
    left 0.22s cubic-bezier(0.4, 0, 0.2, 1),
    right 0.22s cubic-bezier(0.4, 0, 0.2, 1),
    bottom 0.22s cubic-bezier(0.4, 0, 0.2, 1),
    transform 0.22s cubic-bezier(0.4, 0, 0.2, 1);
}

.bottom-dock--dragging {
  transition: none;
}

.bottom-dock-glass {
  pointer-events: auto;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  gap: 4px;
  height: 100%;
  padding: 6px 12px 8px;
  box-sizing: border-box;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.08);
  overflow: visible;
}

html.dark .bottom-dock-glass {
  background: rgba(30, 30, 40, 0.55);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
}

.bottom-dock--floating .bottom-dock-glass {
  width: max-content;
  margin: 0 auto;
  border-radius: 20px;
  box-shadow:
    0 12px 40px rgba(31, 38, 135, 0.16),
    0 2px 8px rgba(0, 0, 0, 0.06);
}

.bottom-dock--snapped .bottom-dock-glass {
  width: 100%;
  justify-content: flex-start;
}

.bottom-dock--collapsed .bottom-dock-glass {
  justify-content: center;
  padding-inline: 10px;
}

.dock-handle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  border: none;
  border-radius: 12px;
  background: transparent;
  color: rgb(var(--base-text-color));
  cursor: grab;
  touch-action: none;
  user-select: none;
  transition: background-color 0.15s ease, color 0.15s ease;
}

.dock-handle:hover {
  color: rgb(var(--primary-color));
  background: rgb(var(--primary-color) / 0.12);
}

.dock-handle:active {
  cursor: grabbing;
}

.dock-handle-icon {
  font-size: 22px;
  pointer-events: none;
}

.dock-items {
  display: flex;
  align-items: flex-end;
  gap: 2px;
  min-width: 0;
  height: 100%;
  overflow: visible;
}

.dock-item-slot {
  display: flex;
  align-items: flex-end;
  flex-shrink: 0;
}

.bottom-dock--snapped .dock-items {
  flex: 1;
  justify-content: center;
}

.dock-collapse-enter-active,
.dock-collapse-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease, max-width 0.2s ease;
  max-width: 1200px;
  overflow: hidden;
}

.dock-collapse-enter-from,
.dock-collapse-leave-to {
  opacity: 0;
  transform: scaleX(0.85);
  max-width: 0;
}
</style>
