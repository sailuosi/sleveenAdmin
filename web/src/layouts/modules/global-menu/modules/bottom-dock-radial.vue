<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from 'vue';
import type { RouteKey } from '@elegant-router/types';
import { useRouterPush } from '@/hooks/common/router';
import { useMenu } from '../context';
import { distributeMegaMenuMasonry, getMegaMenuColumnCount } from '../utils/megaMenuMasonry';
import { allowSecondaryTitleNavigate } from '../utils/megaMenuConfig';
import { buildMegaSections, canNavigateMenu } from '../utils/megaMenuSections';
import type { MenuWithMeta } from '../utils/megaMenuSections';
import MegaSectionNode from '../components/mega-section-node.vue';

defineOptions({
  name: 'BottomDockRadial'
});

interface Props {
  menus: MenuWithMeta[];
  open: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  select: [menu: MenuWithMeta];
  expandDock: [];
  panelHold: [holding: boolean];
}>();

const { routerPushByKeyWithMetaQuery } = useRouterPush();
const { selectedKey } = useMenu();

const BASE_RADIUS = 78;
const ITEM_SIZE = 40;
const VIEW_PAD = 12;
const MEGA_LEAVE_DELAY = 220;

const rootRef = ref<HTMLElement | null>(null);
const activeMegaKey = ref<string | null>(null);
const center = ref({ x: 0, y: 0 });
let megaLeaveTimer: ReturnType<typeof setTimeout> | null = null;

const idealRadius = computed(() => {
  const n = Math.max(props.menus.length, 1);
  return BASE_RADIUS + Math.max(0, n - 6) * 6;
});

/** 按视口剩余空间选择扇区与半径，避免出屏 */
const layout = computed(() => {
  const cx = center.value.x;
  const cy = center.value.y;
  const need = idealRadius.value;
  const half = ITEM_SIZE / 2 + VIEW_PAD;
  const vw = typeof window === 'undefined' ? 1200 : window.innerWidth;
  const vh = typeof window === 'undefined' ? 800 : window.innerHeight;

  const space = {
    top: Math.max(0, cy - half),
    bottom: Math.max(0, vh - cy - half),
    left: Math.max(0, cx - half),
    right: Math.max(0, vw - cx - half)
  };

  const fit = (v: number) => v >= need - 4;
  const blocked = {
    top: !fit(space.top),
    bottom: !fit(space.bottom),
    left: !fit(space.left),
    right: !fit(space.right)
  };

  type Sector = { start: number; sweep: number; r: number };
  let sector: Sector;

  const allFree = !blocked.top && !blocked.bottom && !blocked.left && !blocked.right;
  if (allFree) {
    sector = { start: -Math.PI / 2, sweep: Math.PI * 2, r: need };
  } else if (blocked.bottom && !blocked.top && !blocked.left && !blocked.right) {
    // 贴底 → 上半扇
    sector = { start: Math.PI, sweep: Math.PI, r: Math.min(need, space.top) };
  } else if (blocked.top && !blocked.bottom && !blocked.left && !blocked.right) {
    sector = { start: 0, sweep: Math.PI, r: Math.min(need, space.bottom) };
  } else if (blocked.left && !blocked.right && !blocked.top && !blocked.bottom) {
    sector = { start: -Math.PI / 2, sweep: Math.PI, r: Math.min(need, space.right) };
  } else if (blocked.right && !blocked.left && !blocked.top && !blocked.bottom) {
    sector = { start: Math.PI / 2, sweep: Math.PI, r: Math.min(need, space.left) };
  } else {
    // 角落或双侧受限：取最大可用象限
    const quarters: Array<Sector & { score: number }> = [
      { start: -Math.PI / 2, sweep: Math.PI / 2, r: Math.min(need, space.top, space.right), score: Math.min(space.top, space.right) },
      { start: 0, sweep: Math.PI / 2, r: Math.min(need, space.bottom, space.right), score: Math.min(space.bottom, space.right) },
      { start: Math.PI / 2, sweep: Math.PI / 2, r: Math.min(need, space.bottom, space.left), score: Math.min(space.bottom, space.left) },
      { start: Math.PI, sweep: Math.PI / 2, r: Math.min(need, space.top, space.left), score: Math.min(space.top, space.left) }
    ];
    quarters.sort((a, b) => b.score - a.score);
    const best = quarters[0];
    sector = { start: best.start, sweep: best.sweep, r: Math.max(48, best.r) };
  }

  sector.r = Math.max(48, Math.min(sector.r, need));
  return sector;
});

const hostSize = computed(() => layout.value.r * 2 + ITEM_SIZE + 32);

interface RadialItem {
  menu: MenuWithMeta;
  x: number;
  y: number;
  delay: number;
  index: number;
  side: 'top' | 'bottom' | 'left' | 'right';
}

function pickMegaSide(x: number, y: number, cx: number, cy: number): RadialItem['side'] {
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  // 优先朝屏幕中心方向展开面板，减少贴边难移入
  const toCenterX = vw / 2 - (cx + x);
  const toCenterY = vh / 2 - (cy + y);
  if (Math.abs(toCenterX) >= Math.abs(toCenterY)) {
    return toCenterX >= 0 ? 'right' : 'left';
  }
  return toCenterY >= 0 ? 'bottom' : 'top';
}

const radialItems = computed<RadialItem[]>(() => {
  const list = props.menus;
  const n = list.length;
  if (n === 0) return [];

  const { start, sweep, r } = layout.value;
  const cx = center.value.x;
  const cy = center.value.y;
  const vw = typeof window === 'undefined' ? 1200 : window.innerWidth;
  const vh = typeof window === 'undefined' ? 800 : window.innerHeight;
  const minX = VIEW_PAD + ITEM_SIZE / 2;
  const maxX = vw - VIEW_PAD - ITEM_SIZE / 2;
  const minY = VIEW_PAD + ITEM_SIZE / 2;
  const maxY = vh - VIEW_PAD - ITEM_SIZE / 2;

  return list.map((menu, index) => {
    // 全圆用 n 等分；扇形用 n-1 端点对齐
    const ratio = sweep >= Math.PI * 2 - 0.01 ? index / n : n === 1 ? 0.5 : index / (n - 1);
    const angle = start + ratio * sweep;
    let x = Math.cos(angle) * r;
    let y = Math.sin(angle) * r;

    // 钳制到视口内
    const absX = Math.min(maxX, Math.max(minX, cx + x));
    const absY = Math.min(maxY, Math.max(minY, cy + y));
    x = absX - cx;
    y = absY - cy;

    return {
      menu,
      x,
      y,
      delay: index * 0.028,
      index,
      side: pickMegaSide(x, y, cx, cy)
    };
  });
});

function measureCenter() {
  const el = rootRef.value?.parentElement ?? rootRef.value;
  if (!el) return;
  const rect = el.getBoundingClientRect();
  center.value = {
    x: rect.left + rect.width / 2,
    y: rect.top + rect.height / 2
  };
}

function clearMegaLeaveTimer() {
  if (megaLeaveTimer) {
    clearTimeout(megaLeaveTimer);
    megaLeaveTimer = null;
  }
}

function holdMega(key: string | null, holding: boolean) {
  clearMegaLeaveTimer();
  activeMegaKey.value = key;
  emit('panelHold', holding && Boolean(key));
}

function scheduleClearMega(key: string) {
  clearMegaLeaveTimer();
  megaLeaveTimer = setTimeout(() => {
    if (activeMegaKey.value === key) {
      activeMegaKey.value = null;
      emit('panelHold', false);
    }
    megaLeaveTimer = null;
  }, MEGA_LEAVE_DELAY);
}

watch(
  () => props.open,
  async open => {
    if (open) {
      await nextTick();
      measureCenter();
    } else {
      clearMegaLeaveTimer();
      activeMegaKey.value = null;
      emit('panelHold', false);
    }
  }
);

function onWinChange() {
  if (props.open) measureCenter();
}

if (typeof window !== 'undefined') {
  window.addEventListener('resize', onWinChange);
  window.addEventListener('scroll', onWinChange, true);
}

onUnmounted(() => {
  clearMegaLeaveTimer();
  window.removeEventListener('resize', onWinChange);
  window.removeEventListener('scroll', onWinChange, true);
});

function isMenuActive(menu: App.Global.Menu) {
  const key = selectedKey.value;
  if (!key) return false;
  if (key === menu.key) return true;
  return key.startsWith(`${menu.key}_`);
}

function isLinkActive(menu: App.Global.Menu) {
  return selectedKey.value === menu.key;
}

function hasChildren(menu: App.Global.Menu) {
  return Boolean(menu.children?.length);
}

function canNavigateSecondaryTitle(menu: App.Global.Menu) {
  return allowSecondaryTitleNavigate(menu);
}

function getMegaMenuClass(menu: App.Global.Menu) {
  const sectionCount = buildMegaSections(menu).length;
  return {
    'radial-mega--compact': sectionCount <= 2,
    'radial-mega--wide': sectionCount >= 4
  };
}

function getMegaMenuBodyClass(menu: App.Global.Menu) {
  const sectionCount = buildMegaSections(menu).length;
  return {
    'radial-mega-body--single': sectionCount <= 2,
    'radial-mega-body--masonry': sectionCount > 2
  };
}

function getMasonryColumns(menu: App.Global.Menu) {
  const sections = buildMegaSections(menu);
  const columnCount = getMegaMenuColumnCount(sections.length);
  return distributeMegaMenuMasonry(sections, columnCount);
}

/** fixed 子面板样式：贴图标外侧，朝屏幕中心，并做视口钳制 */
const megaPanelStyle = ref<Record<string, string>>({});

function placeMegaPanel(trigger: HTMLElement, side: RadialItem['side'], panelEl?: HTMLElement | null) {
  const rect = trigger.getBoundingClientRect();
  const gap = 8;
  const pad = 10;
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  const estW = panelEl?.offsetWidth || 280;
  const estH = panelEl?.offsetHeight || 220;

  let left = 0;
  let top = 0;

  if (side === 'top') {
    left = rect.left + rect.width / 2 - estW / 2;
    top = rect.top - gap - estH;
  } else if (side === 'bottom') {
    left = rect.left + rect.width / 2 - estW / 2;
    top = rect.bottom + gap;
  } else if (side === 'left') {
    left = rect.left - gap - estW;
    top = rect.top + rect.height / 2 - estH / 2;
  } else {
    left = rect.right + gap;
    top = rect.top + rect.height / 2 - estH / 2;
  }

  left = Math.min(vw - pad - estW, Math.max(pad, left));
  top = Math.min(vh - pad - Math.min(estH, vh - pad * 2), Math.max(pad, top));

  megaPanelStyle.value = {
    left: `${left}px`,
    top: `${top}px`
  };
}

function onItemEnter(menu: MenuWithMeta, side: RadialItem['side'], event: MouseEvent) {
  if (!props.open || menu.disabled) return;
  if (!hasChildren(menu)) return;
  const trigger = event.currentTarget as HTMLElement;
  holdMega(menu.key, true);
  nextTick(() => {
    placeMegaPanel(trigger, side);
    requestAnimationFrame(() => {
      const panel = document.querySelector(`[data-radial-mega="${menu.key}"]`) as HTMLElement | null;
      placeMegaPanel(trigger, side, panel);
    });
  });
}

function onItemLeave(menu: MenuWithMeta, event: MouseEvent) {
  const related = event.relatedTarget as HTMLElement | null;
  if (related?.closest?.(`[data-radial-mega="${menu.key}"]`)) return;
  if (activeMegaKey.value === menu.key) scheduleClearMega(menu.key);
}

function onMegaEnter(menu: MenuWithMeta) {
  holdMega(menu.key, true);
}

function onMegaLeave(menu: MenuWithMeta, event: MouseEvent) {
  const related = event.relatedTarget as HTMLElement | null;
  if (related?.closest?.(`[data-radial-item="${menu.key}"]`)) return;
  if (activeMegaKey.value === menu.key) scheduleClearMega(menu.key);
}

function handleItemClick(menu: MenuWithMeta) {
  if (menu.disabled) return;
  if (hasChildren(menu)) {
    if (canNavigateMenu(menu)) {
      routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
      emit('select', menu);
    }
    return;
  }
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
  emit('select', menu);
}

function handleLinkClick(menu: App.Global.Menu) {
  if (menu.disabled) return;
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
  emit('select', menu as MenuWithMeta);
  holdMega(null, false);
}

const activeMegaMenu = computed(() =>
  props.menus.find(m => m.key === activeMegaKey.value && hasChildren(m)) as MenuWithMeta | undefined
);

const activeMegaSide = computed(() => {
  const item = radialItems.value.find(i => i.menu.key === activeMegaKey.value);
  return item?.side ?? 'top';
});
</script>

<template>
  <div
    ref="rootRef"
    class="dock-radial"
    :class="{ 'dock-radial--open': open }"
    :style="{
      '--radial-size': `${hostSize}px`,
      '--radial-r': `${layout.r}px`
    }"
  >
    <div class="dock-radial-hit" :class="{ 'dock-radial-hit--on': open }" aria-hidden="true" />
    <div class="dock-radial-ring" aria-hidden="true" />

    <div
      v-for="item in radialItems"
      :key="item.menu.key"
      :data-radial-item="item.menu.key"
      class="dock-radial-item"
      :class="{
        'dock-radial-item--active': isMenuActive(item.menu),
        'dock-radial-item--disabled': item.menu.disabled,
        'dock-radial-item--open': open,
        'dock-radial-item--has-children': hasChildren(item.menu),
        'dock-radial-item--mega-on': activeMegaKey === item.menu.key
      }"
      :style="{
        '--rx': `${item.x}px`,
        '--ry': `${item.y}px`,
        '--r-delay': `${item.delay}s`,
        '--r-delay-out': `${(radialItems.length - 1 - item.index) * 0.018}s`
      }"
      @mouseenter="onItemEnter(item.menu, item.side, $event)"
      @mouseleave="onItemLeave(item.menu, $event)"
      @click.stop="handleItemClick(item.menu)"
    >
      <span
        v-if="!hasChildren(item.menu) || activeMegaKey !== item.menu.key"
        class="dock-radial-item-label"
      >
        {{ item.menu.label }}
      </span>
      <span class="dock-radial-item-icon">
        <component :is="item.menu.icon" v-if="item.menu.icon" />
        <SvgIcon v-else icon="mdi:menu" />
      </span>
      <span v-if="isMenuActive(item.menu)" class="dock-radial-item-dot" />
    </div>
  </div>

  <!-- 子面板 Teleport 到 body，用 fixed + 延迟关闭，方便鼠标移入 -->
  <Teleport to="body">
    <div
      v-if="activeMegaMenu && open"
      :data-radial-mega="activeMegaMenu.key"
      class="radial-mega-fixed"
      :class="[getMegaMenuClass(activeMegaMenu), `radial-mega-fixed--${activeMegaSide}`]"
      :style="megaPanelStyle"
      @mouseenter="onMegaEnter(activeMegaMenu)"
      @mouseleave="onMegaLeave(activeMegaMenu, $event)"
      @click.stop
    >
      <div class="radial-mega-bridge" aria-hidden="true" />
      <div class="radial-mega-body" :class="getMegaMenuBodyClass(activeMegaMenu)">
        <div
          v-for="(column, colIdx) in getMasonryColumns(activeMegaMenu)"
          :key="`${activeMegaMenu.key}-col-${colIdx}`"
          class="radial-mega-column"
        >
          <div
            v-for="section in column"
            :key="section.label + section.links.map(link => link.key).join('-')"
            class="radial-mega-section"
          >
            <MegaSectionNode
              :section="section"
              :allow-title-navigate="canNavigateSecondaryTitle(activeMegaMenu)"
              :is-link-active="isLinkActive"
              :on-link-click="handleLinkClick"
            />
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.dock-radial {
  --radial-size: 200px;
  position: absolute;
  left: 50%;
  top: 50%;
  width: var(--radial-size);
  height: var(--radial-size);
  margin-left: calc(var(--radial-size) / -2);
  margin-top: calc(var(--radial-size) / -2);
  pointer-events: none;
  z-index: 5;
}

.dock-radial-hit {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  pointer-events: none;
}

.dock-radial-hit--on {
  pointer-events: auto;
}

.dock-radial-ring {
  position: absolute;
  left: 50%;
  top: 50%;
  width: calc(var(--radial-r) * 2);
  height: calc(var(--radial-r) * 2);
  margin-left: calc(var(--radial-r) * -1);
  margin-top: calc(var(--radial-r) * -1);
  border-radius: 50%;
  border: 1px dashed rgb(var(--primary-color) / 0.28);
  background: radial-gradient(circle, rgb(var(--primary-color) / 0.06), transparent 68%);
  opacity: 0;
  transform: scale(0.55);
  transition:
    opacity 0.22s ease,
    transform 0.28s cubic-bezier(0.34, 1.2, 0.64, 1);
  pointer-events: none;
}

.dock-radial--open .dock-radial-ring {
  opacity: 1;
  transform: scale(1);
}

.dock-radial-item {
  position: absolute;
  left: 50%;
  top: 50%;
  width: 40px;
  height: 40px;
  margin-left: -20px;
  margin-top: -20px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgb(var(--base-text-color));
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(12px) saturate(1.3);
  -webkit-backdrop-filter: blur(12px) saturate(1.3);
  border: 1px solid rgba(255, 255, 255, 0.5);
  box-shadow: 0 6px 18px rgba(31, 38, 135, 0.12);
  cursor: pointer;
  transform: translate(0, 0) scale(0.35);
  opacity: 0;
  pointer-events: none;
  transition:
    transform 0.28s cubic-bezier(0.34, 1.35, 0.64, 1),
    opacity 0.2s ease,
    background-color 0.15s ease,
    color 0.15s ease,
    box-shadow 0.15s ease;
  transition-delay: var(--r-delay-out, 0s);
  z-index: 2;
}

html.dark .dock-radial-item {
  background: rgba(36, 38, 48, 0.9);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
}

.dock-radial-item--open {
  transform: translate(var(--rx), var(--ry)) scale(1);
  opacity: 1;
  pointer-events: auto;
  transition-delay: var(--r-delay, 0s);
}

.dock-radial-item:hover,
.dock-radial-item--mega-on {
  color: rgb(var(--primary-color));
  background: rgba(255, 255, 255, 0.95);
  box-shadow: 0 8px 22px rgb(var(--primary-color) / 0.22);
  z-index: 8;
}

html.dark .dock-radial-item:hover,
html.dark .dock-radial-item--mega-on {
  background: rgba(48, 50, 62, 0.96);
}

.dock-radial-item--active {
  color: rgb(var(--primary-color));
  box-shadow: 0 0 0 2px rgb(var(--primary-color) / 0.35);
}

.dock-radial-item--disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.dock-radial-item-icon {
  display: flex;
  font-size: 20px;
  line-height: 1;
  pointer-events: none;
}

.dock-radial-item-label {
  position: absolute;
  left: 50%;
  bottom: calc(100% + 8px);
  transform: translateX(-50%) translateY(4px);
  padding: 3px 8px;
  border-radius: 8px;
  background: rgba(20, 24, 36, 0.9);
  color: #fff;
  font-size: 11px;
  line-height: 1.2;
  white-space: nowrap;
  pointer-events: none;
  opacity: 0;
  visibility: hidden;
  transition: opacity 0.12s ease, transform 0.12s ease, visibility 0.12s ease;
  z-index: 4;
}

.dock-radial-item:hover .dock-radial-item-label {
  opacity: 1;
  visibility: visible;
  transform: translateX(-50%) translateY(0);
}

.dock-radial-item-dot {
  position: absolute;
  bottom: 3px;
  left: 50%;
  width: 4px;
  height: 4px;
  margin-left: -2px;
  border-radius: 50%;
  background: rgb(var(--primary-color));
  pointer-events: none;
}
</style>

<style>
/* Teleport 到 body，非 scoped */
.radial-mega-fixed {
  position: fixed;
  z-index: 3000;
  width: min(400px, calc(100vw - 24px));
  pointer-events: auto;
}

.radial-mega-fixed.radial-mega--compact {
  width: auto;
  min-width: 230px;
  max-width: min(320px, calc(100vw - 24px));
}

.radial-mega-fixed.radial-mega--wide {
  width: min(720px, calc(100vw - 24px));
}

/* 透明桥：从面板向图标方向延伸，便于鼠标滑入 */
.radial-mega-bridge {
  position: absolute;
  pointer-events: auto;
  background: transparent;
}

.radial-mega-fixed--top .radial-mega-bridge {
  left: 0;
  right: 0;
  top: 100%;
  height: 28px;
}

.radial-mega-fixed--bottom .radial-mega-bridge {
  left: 0;
  right: 0;
  bottom: 100%;
  height: 28px;
}

.radial-mega-fixed--left .radial-mega-bridge {
  top: 0;
  bottom: 0;
  left: 100%;
  width: 28px;
}

.radial-mega-fixed--right .radial-mega-bridge {
  top: 0;
  bottom: 0;
  right: 100%;
  width: 28px;
}

.radial-mega-fixed .radial-mega-body {
  padding: 10px 12px 12px;
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  gap: 12px 14px;
  border-radius: 14px;
  background: #ffffff;
  border: 1px solid #e3ebf5;
  box-shadow:
    0 16px 48px rgba(31, 111, 209, 0.16),
    0 8px 20px rgba(0, 0, 0, 0.04);
  max-height: 60vh;
  overflow-y: auto;
}

html.dark .radial-mega-fixed .radial-mega-body {
  background: rgba(30, 30, 40, 0.96);
  border-color: rgba(255, 255, 255, 0.12);
}

.radial-mega-fixed .radial-mega-body--single {
  flex-direction: column;
}

.radial-mega-fixed .radial-mega-column {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.radial-mega-fixed .radial-mega-section {
  min-width: 0;
}
</style>
