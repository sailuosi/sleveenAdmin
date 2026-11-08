<script setup lang="ts">
import type { RouteKey } from '@elegant-router/types';
import { useRouterPush } from '@/hooks/common/router';
import { useMenu } from '../context';
import { distributeMegaMenuMasonry, getMegaMenuColumnCount } from '../utils/megaMenuMasonry';
import { allowSecondaryTitleNavigate } from '../utils/megaMenuConfig';
import { buildMegaSections, canNavigateMenu } from '../utils/megaMenuSections';
import type { MenuWithMeta } from '../utils/megaMenuSections';
import MegaSectionNode from '../components/mega-section-node.vue';

defineOptions({
  name: 'BottomDockItem'
});

interface Props {
  menu: MenuWithMeta;
  scale: number;
  hovered: boolean;
}

const props = defineProps<Props>();

const { routerPushByKeyWithMetaQuery } = useRouterPush();
const { selectedKey } = useMenu();

function hasChildren(menu: App.Global.Menu) {
  return Boolean(menu.children?.length);
}

function isMenuActive(menu: App.Global.Menu) {
  const key = selectedKey.value;
  if (!key) return false;
  if (key === menu.key) return true;
  return key.startsWith(`${menu.key}_`);
}

function isLinkActive(menu: App.Global.Menu) {
  return selectedKey.value === menu.key;
}

function canNavigateParent(menu: MenuWithMeta) {
  return canNavigateMenu(menu);
}

function canNavigateSecondaryTitle(menu: App.Global.Menu) {
  return allowSecondaryTitleNavigate(menu);
}

function getMegaMenuClass(menu: App.Global.Menu) {
  const sectionCount = buildMegaSections(menu).length;
  return {
    'mega-menu--compact': sectionCount <= 2,
    'mega-menu--wide': sectionCount >= 4
  };
}

function getMegaMenuBodyClass(menu: App.Global.Menu) {
  const sectionCount = buildMegaSections(menu).length;
  return {
    'mega-menu-body--single': sectionCount <= 2,
    'mega-menu-body--masonry': sectionCount > 2
  };
}

function getMasonryColumns(menu: App.Global.Menu) {
  const sections = buildMegaSections(menu);
  const columnCount = getMegaMenuColumnCount(sections.length);
  return distributeMegaMenuMasonry(sections, columnCount);
}

function handleClick(menu: MenuWithMeta) {
  if (menu.disabled) return;
  if (hasChildren(menu)) {
    if (canNavigateParent(menu)) {
      routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
    }
    return;
  }
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
}

function handleLinkClick(menu: App.Global.Menu) {
  if (menu.disabled) return;
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
}

function adjustMegaMenuPosition(event: MouseEvent) {
  const trigger = event.currentTarget as HTMLElement | null;
  const panel = trigger?.querySelector('.dock-mega') as HTMLElement | null;
  if (!trigger || !panel) return;

  panel.style.left = '50%';
  panel.style.right = 'auto';
  panel.style.transform = '';

  requestAnimationFrame(() => {
    const padding = 12;
    const triggerRect = trigger.getBoundingClientRect();
    const menuWidth = panel.offsetWidth;
    const viewportWidth = window.innerWidth;
    let shift = 0;
    const centerX = triggerRect.left + triggerRect.width / 2;
    if (centerX - menuWidth / 2 < padding) {
      shift = padding - (centerX - menuWidth / 2);
    } else if (centerX + menuWidth / 2 > viewportWidth - padding) {
      shift = viewportWidth - padding - (centerX + menuWidth / 2);
    }
    panel.style.setProperty('--dock-mega-shift', `${shift}px`);
  });
}
</script>

<template>
  <div
    class="dock-item"
    :class="{
      'dock-item--active': isMenuActive(menu),
      'dock-item--disabled': menu.disabled,
      'dock-item--has-children': hasChildren(menu),
      'dock-item--hovered': hovered
    }"
    :style="{ '--dock-scale': scale }"
    @mouseenter="hasChildren(menu) && adjustMegaMenuPosition($event)"
    @click="handleClick(menu)"
  >
    <div class="dock-item-label" aria-hidden="true">{{ menu.label }}</div>
    <div class="dock-item-icon-wrap">
      <component :is="menu.icon" v-if="menu.icon" class="dock-item-icon" />
      <SvgIcon v-else icon="mdi:menu" class="dock-item-icon" />
    </div>
    <span v-if="isMenuActive(menu)" class="dock-item-dot" />

    <div
      v-if="hasChildren(menu)"
      class="dock-mega"
      :class="getMegaMenuClass(menu)"
    >
      <div class="dock-mega-body" :class="getMegaMenuBodyClass(menu)">
        <div
          v-for="(column, colIdx) in getMasonryColumns(menu)"
          :key="`${menu.key}-col-${colIdx}`"
          class="dock-mega-column"
        >
          <div
            v-for="section in column"
            :key="section.label + section.links.map(item => item.key).join('-')"
            class="dock-mega-section"
          >
            <MegaSectionNode
              :section="section"
              :allow-title-navigate="canNavigateSecondaryTitle(menu)"
              :is-link-active="isLinkActive"
              :on-link-click="handleLinkClick"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dock-item {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  width: 48px;
  height: 48px;
  flex-shrink: 0;
  cursor: pointer;
  transform-origin: bottom center;
  transform: scale(var(--dock-scale, 1));
  transition: transform 0.12s ease-out;
  z-index: calc(1 + var(--dock-scale, 1) * 10);
}

.dock-item--disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.dock-item-label {
  position: absolute;
  bottom: calc(100% + 10px);
  left: 50%;
  transform: translateX(-50%) translateY(4px);
  padding: 4px 10px;
  border-radius: 8px;
  background: rgba(20, 24, 36, 0.88);
  color: #fff;
  font-size: 12px;
  line-height: 1.2;
  white-space: nowrap;
  pointer-events: none;
  opacity: 0;
  visibility: hidden;
  transition: opacity 0.15s ease, transform 0.15s ease, visibility 0.15s ease;
  z-index: 20;
}

.dock-item--hovered .dock-item-label,
.dock-item:hover .dock-item-label {
  opacity: 1;
  visibility: visible;
  transform: translateX(-50%) translateY(0);
}

.dock-item-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  color: rgb(var(--base-text-color));
  transition: background-color 0.15s ease, color 0.15s ease;
}

.dock-item:hover .dock-item-icon-wrap,
.dock-item--active .dock-item-icon-wrap {
  color: rgb(var(--primary-color));
  background: rgb(var(--primary-color) / 0.12);
}

.dock-item-icon {
  font-size: 22px;
  line-height: 1;
}

.dock-item-dot {
  position: absolute;
  bottom: 2px;
  left: 50%;
  width: 5px;
  height: 5px;
  margin-left: -2.5px;
  border-radius: 50%;
  background: rgb(var(--primary-color));
  box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.55);
}

.dock-mega {
  --dock-mega-shift: 0px;
  position: absolute;
  bottom: calc(100% + 14px);
  left: 50%;
  width: min(400px, calc(100vw - 24px));
  padding-bottom: 8px;
  background: transparent;
  opacity: 0;
  visibility: hidden;
  transform: translateX(calc(-50% + var(--dock-mega-shift))) translateY(8px) scale(0.96);
  transform-origin: bottom center;
  transition:
    opacity 0.25s cubic-bezier(0.4, 0, 0.2, 1),
    visibility 0.25s ease,
    transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 1001;
  cursor: default;
  max-height: 70vh;
  overflow-y: auto;
  pointer-events: none;
}

.dock-mega--compact {
  width: auto;
  min-width: 230px;
  max-width: min(320px, calc(100vw - 24px));
}

.dock-mega--wide {
  width: min(720px, calc(100vw - 24px));
}

.dock-item--has-children:hover .dock-mega {
  opacity: 1;
  visibility: visible;
  transform: translateX(calc(-50% + var(--dock-mega-shift))) translateY(0) scale(1);
  pointer-events: auto;
}

.dock-mega-body {
  padding: 10px 12px 12px;
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  gap: 12px 14px;
  border-radius: 14px;
  background: #ffffff;
  border: 1px solid #e3ebf5;
  box-shadow:
    0 -16px 48px rgba(31, 111, 209, 0.14),
    0 8px 20px rgba(0, 0, 0, 0.04);
}

html.dark .dock-mega-body {
  background: rgba(30, 30, 40, 0.96);
  border-color: rgba(255, 255, 255, 0.12);
}

.dock-mega-body--single {
  flex-direction: column;
}

.dock-mega-column {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.dock-mega-section {
  min-width: 0;
}
</style>
