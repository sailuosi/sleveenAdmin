<script setup lang="ts">
import { computed, onUnmounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { GLOBAL_SIDER_MENU_ID } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import GlobalLogo from '../global-logo/index.vue';
import SiderSettingsPopover from './components/sider-settings-popover.vue';
import SiderDockBar from './components/sider-dock-bar.vue';

defineOptions({
  name: 'GlobalSider'
});

const SIDER_WIDTH_MIN = 180;
const SIDER_WIDTH_MAX = 480;

const route = useRoute();
const appStore = useAppStore();
const themeStore = useThemeStore();

const isBorderlessPage = computed(() => {
  const t = route.meta?.menuType as string | number | undefined;
  return t === '4' || t === 4;
});

const isTopHybridSidebarFirst = computed(() => themeStore.layout.mode === 'top-hybrid-sidebar-first');
const isTopHybridHeaderFirst = computed(() => themeStore.layout.mode === 'top-hybrid-header-first');
const isVerticalCard = computed(() => themeStore.layout.mode === 'vertical-card');
/** card shell / dock also when borderless overlays content */
const useCardShell = computed(() => isVerticalCard.value || isBorderlessPage.value);
const darkMenu = computed(
  () =>
    !themeStore.darkMode &&
    !isTopHybridSidebarFirst.value &&
    !isTopHybridHeaderFirst.value &&
    !useCardShell.value &&
    themeStore.sider.inverted
);
const showLogo = computed(() => themeStore.layout.mode === 'vertical' || useCardShell.value);
const menuWrapperClass = computed(() => (showLogo.value ? 'flex-1-hidden' : 'h-full'));

/** drag-resize right edge for single-column left sider */
const resizable = computed(
  () =>
    !appStore.isMobile &&
    !appStore.siderCollapse &&
    (themeStore.layout.mode === 'vertical' || themeStore.layout.mode === 'vertical-card' || isBorderlessPage.value)
);

const resizing = ref(false);
let resizeStartX = 0;
let resizeStartWidth = 0;
let pendingWidth: number | null = null;
let rafId = 0;

/** layout uses transition-all-300; kill it while dragging or width feels lagged */
function setResizingClass(on: boolean) {
  document.documentElement.classList.toggle('sider-resizing', on);
}

function flushWidth() {
  rafId = 0;
  if (pendingWidth == null) return;
  themeStore.sider.width = pendingWidth;
  pendingWidth = null;
}

function onResizeMove(event: PointerEvent) {
  pendingWidth = Math.min(
    SIDER_WIDTH_MAX,
    Math.max(SIDER_WIDTH_MIN, Math.round(resizeStartWidth + (event.clientX - resizeStartX)))
  );
  if (!rafId) {
    rafId = requestAnimationFrame(flushWidth);
  }
}

function stopResize() {
  if (!resizing.value) return;
  resizing.value = false;
  if (rafId) {
    cancelAnimationFrame(rafId);
    rafId = 0;
  }
  if (pendingWidth != null) {
    themeStore.sider.width = pendingWidth;
    pendingWidth = null;
  }
  setResizingClass(false);
  document.body.style.removeProperty('cursor');
  document.body.style.removeProperty('user-select');
  window.removeEventListener('pointermove', onResizeMove);
  window.removeEventListener('pointerup', stopResize);
  window.removeEventListener('pointercancel', stopResize);
}

function startResize(event: PointerEvent) {
  if (!resizable.value || event.button !== 0) return;
  event.preventDefault();
  resizing.value = true;
  resizeStartX = event.clientX;
  resizeStartWidth = themeStore.sider.width;
  setResizingClass(true);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
  window.addEventListener('pointermove', onResizeMove);
  window.addEventListener('pointerup', stopResize);
  window.addEventListener('pointercancel', stopResize);
}

onUnmounted(stopResize);
</script>

<template>
  <DarkModeContainer
    class="relative size-full flex-col-stretch"
    :class="useCardShell ? 'sider-card-shell' : 'shadow-sider'"
    :inverted="darkMenu"
  >
    <div
      v-if="showLogo"
      class="sider-logo-row"
      :style="{ height: themeStore.header.height + 'px' }"
    >
      <GlobalLogo class="sider-logo min-w-0 flex-1" :show-title="!appStore.siderCollapse" />
      <SiderSettingsPopover v-if="useCardShell" />
    </div>
    <div :id="GLOBAL_SIDER_MENU_ID" :class="menuWrapperClass"></div>
    <SiderDockBar v-if="useCardShell" />
    <div
      v-if="resizable"
      class="sider-resize-handle"
      :class="{ 'is-active': resizing }"
      title="拖动调整宽度"
      @pointerdown="startResize"
    ></div>
  </DarkModeContainer>
</template>

<style scoped>
.sider-logo-row {
  --uno: w-full flex-y-center gap-4px px-8px shrink-0;
}

.sider-logo :deep(a) {
  --uno: w-auto justify-start;
}

.sider-card-shell {
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.08);
}

html.dark .sider-card-shell {
  background: rgba(30, 30, 40, 0.55);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
}

.sider-resize-handle {
  position: absolute;
  top: 0;
  right: -3px;
  z-index: 20;
  width: 6px;
  height: 100%;
  cursor: col-resize;
  touch-action: none;
}

.sider-resize-handle::after {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  width: 2px;
  height: 36px;
  border-radius: 1px;
  background: transparent;
  transform: translate(-50%, -50%);
  transition: background-color 0.15s ease;
}

.sider-resize-handle:hover::after,
.sider-resize-handle.is-active::after {
  background: rgb(var(--primary-color) / 0.55);
}
</style>

<!-- unscoped: AdminLayout commonClass is transition-all-300 -->
<style>
html.sider-resizing,
html.sider-resizing * {
  transition: none !important;
}
</style>
