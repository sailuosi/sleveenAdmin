<script setup lang="ts">
import { computed, nextTick, onUnmounted, reactive, ref, watch, type CSSProperties } from 'vue';
import { useRoute } from 'vue-router';
import { useResizeObserver } from '@vueuse/core';
import { SimpleScrollbar } from '@sa/materials';
import { GLOBAL_SIDER_MENU_ID } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { useRouteStore } from '@/store/modules/route';
import { useMenuValuePush } from '@/layouts/modules/global-menu/useMenuValuePush';
import EpMenuItems from '@/layouts/modules/global-menu/components/EpMenuItems.vue';
import { useMenu } from '../context';

defineOptions({
  name: 'VerticalMenu'
});

const HORIZONTAL_INSET = 8;
const VERTICAL_INSET = 3;
const ANIM_DURATION_MS = 420;
const SNAP_DISTANCE_PX = 420;

const route = useRoute();
const appStore = useAppStore();
const themeStore = useThemeStore();
const routeStore = useRouteStore();
const { onMenuValueUpdate } = useMenuValuePush();
const { selectedKey } = useMenu();

const inverted = computed(() => !themeStore.darkMode && themeStore.sider.inverted);
/** dark mode or light inverted sider: transparent menu so DarkModeContainer shows through */
const darkMenu = computed(() => themeStore.darkMode || inverted.value);
const isVerticalCard = computed(() => themeStore.layout.mode === 'vertical-card');

const expandedKeys = ref<string[]>([]);
const menuRootRef = ref<HTMLElement | null>(null);

interface CapsuleGeom {
  top: number;
  left: number;
  width: number;
  height: number;
}

const capsule = reactive({
  visible: false,
  top: 0,
  left: 0,
  width: 0,
  height: 0
});

const capsuleStyle = computed<CSSProperties>(() => ({
  opacity: capsule.visible ? 1 : 0,
  top: `${capsule.top}px`,
  left: `${capsule.left}px`,
  width: `${capsule.width}px`,
  height: `${capsule.height}px`
}));

let animFrame = 0;
let hasPlaced = false;
let currentGeom: CapsuleGeom | null = null;

function updateExpandedKeys() {
  if (appStore.siderCollapse || !selectedKey.value) {
    expandedKeys.value = [];
    return;
  }
  expandedKeys.value = routeStore.getSelectedMenuKeyPath(selectedKey.value);
}

function findMenu(menus: App.Global.Menu[], key: string): App.Global.Menu | undefined {
  for (const menu of menus) {
    if (menu.key === key) return menu;
    if (menu.children?.length) {
      const found = findMenu(menu.children, key);
      if (found) return found;
    }
  }
  return undefined;
}

function handleSelect(key: string) {
  const item = findMenu(routeStore.menus, key);
  onMenuValueUpdate(key, item);
}

function lerp(a: number, b: number, t: number) {
  return a + (b - a) * t;
}

/** easeOut with light overshoot for elastic settle */
function elasticOut(t: number) {
  if (t <= 0) return 0;
  if (t >= 1) return 1;
  const c4 = (2 * Math.PI) / 2.8;
  return Math.pow(2, -9 * t) * Math.sin((t * 9.2 - 0.75) * c4) + 1;
}

function measureActiveItem(): CapsuleGeom | null {
  const root = menuRootRef.value;
  if (!root) return null;

  const active = root.querySelector('.el-menu-item.is-active') as HTMLElement | null;
  if (!active) return null;

  const rootRect = root.getBoundingClientRect();
  const itemRect = active.getBoundingClientRect();

  return {
    top: itemRect.top - rootRect.top + root.scrollTop + VERTICAL_INSET,
    left: itemRect.left - rootRect.left + root.scrollLeft + HORIZONTAL_INSET,
    width: Math.max(0, itemRect.width - HORIZONTAL_INSET * 2),
    height: Math.max(0, itemRect.height - VERTICAL_INSET * 2)
  };
}

function applyGeom(geom: CapsuleGeom) {
  capsule.top = geom.top;
  capsule.left = geom.left;
  capsule.width = geom.width;
  capsule.height = geom.height;
  capsule.visible = geom.width > 0 && geom.height > 0;
  currentGeom = { ...geom };
}

function cancelAnim() {
  if (animFrame) {
    cancelAnimationFrame(animFrame);
    animFrame = 0;
  }
}

function animateTo(target: CapsuleGeom) {
  cancelAnim();

  const from: CapsuleGeom = currentGeom
    ? { ...currentGeom }
    : {
        top: capsule.top,
        left: capsule.left,
        width: capsule.width,
        height: capsule.height
      };

  const dy = target.top - from.top;
  const distance = Math.abs(dy);

  if (!hasPlaced || distance > SNAP_DISTANCE_PX) {
    applyGeom(target);
    hasPlaced = true;
    return;
  }

  const start = performance.now();
  // height stays fixed at target; only slide + elastic settle on position
  const height = target.height;

  const tick = (now: number) => {
    const raw = Math.min(1, (now - start) / ANIM_DURATION_MS);
    const p = elasticOut(raw);

    applyGeom({
      top: lerp(from.top, target.top, p),
      left: lerp(from.left, target.left, p),
      width: lerp(from.width, target.width, p),
      height
    });

    if (raw < 1) {
      animFrame = requestAnimationFrame(tick);
    } else {
      applyGeom(target);
      animFrame = 0;
    }
  };

  animFrame = requestAnimationFrame(tick);
  hasPlaced = true;
}

async function syncCapsule(opts: { animate?: boolean } = {}) {
  if (!isVerticalCard.value) {
    cancelAnim();
    capsule.visible = false;
    hasPlaced = false;
    currentGeom = null;
    return;
  }

  await nextTick();
  // ElMenu remounts via :key — wait a frame for layout
  await new Promise<void>(resolve => requestAnimationFrame(() => resolve()));

  const geom = measureActiveItem();
  if (!geom) {
    capsule.visible = false;
    return;
  }

  if (opts.animate === false || !hasPlaced) {
    applyGeom(geom);
    hasPlaced = true;
    return;
  }

  animateTo(geom);
}

watch(
  () => route.name,
  () => {
    updateExpandedKeys();
  },
  { immediate: true }
);

watch(
  [selectedKey, expandedKeys, () => appStore.siderCollapse, isVerticalCard],
  () => {
    void syncCapsule({ animate: true });
  },
  { deep: true }
);

useResizeObserver(menuRootRef, () => {
  if (!isVerticalCard.value) return;
  void syncCapsule({ animate: false });
});

watch(
  () => themeStore.sider.width,
  () => {
    if (!isVerticalCard.value) return;
    void syncCapsule({ animate: false });
  }
);

onUnmounted(() => {
  cancelAnim();
});
</script>

<template>
  <Teleport :to="`#${GLOBAL_SIDER_MENU_ID}`">
    <SimpleScrollbar>
      <div ref="menuRootRef" class="vc-menu-root" :class="{ 'is-card': isVerticalCard }">
        <div
          v-if="isVerticalCard"
          class="vc-active-capsule"
          :class="{ 'is-ready': capsule.visible }"
          :style="capsuleStyle"
          aria-hidden="true"
        />
        <ElMenu
          :key="`${String(selectedKey)}-${expandedKeys.join(',')}`"
          class="border-none! vc-el-menu"
          :class="{ 'is-inverted': darkMenu }"
          :default-active="selectedKey || undefined"
          :default-openeds="expandedKeys"
          :collapse="appStore.siderCollapse"
          :collapse-transition="false"
          @select="handleSelect"
        >
          <EpMenuItems :menus="routeStore.menus" />
        </ElMenu>
      </div>
    </SimpleScrollbar>
  </Teleport>
</template>

<style scoped>
.el-menu.is-inverted {
  --el-menu-bg-color: transparent;
  --el-menu-text-color: rgba(255, 255, 255, 0.88);
  --el-menu-hover-text-color: #fff;
  --el-menu-active-color: #fff;
}

.vc-menu-root {
  position: relative;
}

.vc-menu-root.is-card :deep(.vc-el-menu) {
  --el-menu-bg-color: transparent;
  /* 叶子项默认 hover；父级 title 再用更高不透明度覆盖 */
  --el-menu-hover-bg-color: rgb(255 255 255 / 0.4);
  position: relative;
  z-index: 1;
}

.vc-menu-root.is-card :deep(.el-menu-item),
.vc-menu-root.is-card :deep(.el-sub-menu__title) {
  position: relative;
  z-index: 1;
  border-radius: 999px;
  margin: 2px 0;
}

.vc-menu-root.is-card :deep(.el-menu-item.is-active),
.vc-menu-root.is-card :deep(.el-menu-item.is-active:hover) {
  background-color: transparent !important;
}

/* 仅 hover 加深；不要绑 is-opened，否则父级展开会像「激活变深」 */
.vc-menu-root.is-card :deep(.el-sub-menu__title:hover) {
  background-color: rgb(255 255 255 / 0.72) !important;
}

.vc-active-capsule {
  position: absolute;
  z-index: 0;
  pointer-events: none;
  border-radius: 999px;
  background: rgb(255 255 255 / 0.42);
  backdrop-filter: blur(12px) saturate(1.35);
  -webkit-backdrop-filter: blur(12px) saturate(1.35);
  box-shadow:
    inset 0 0 0 1px rgb(255 255 255 / 0.65),
    0 0 0 0.5px rgb(255 255 255 / 0.35),
    0 4px 14px rgb(31 38 135 / 0.08);
  transition: opacity 0.18s ease;
  will-change: top, left, width, height;
}

.vc-active-capsule:not(.is-ready) {
  opacity: 0;
}

:global(html.dark) .vc-active-capsule {
  background: rgb(255 255 255 / 0.1);
  box-shadow:
    inset 0 0 0 1px rgb(255 255 255 / 0.28),
    0 0 0 0.5px rgb(255 255 255 / 0.16),
    0 4px 16px rgb(0 0 0 / 0.28);
}

:global(html.dark) .vc-menu-root.is-card :deep(.vc-el-menu) {
  --el-menu-hover-bg-color: rgb(255 255 255 / 0.14);
}

:global(html.dark) .vc-menu-root.is-card :deep(.el-sub-menu__title:hover) {
  background-color: rgb(255 255 255 / 0.28) !important;
}
</style>
