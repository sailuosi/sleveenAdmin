<script setup lang="ts">
import type { RouteKey } from '@elegant-router/types';
import { useRouterPush } from '@/hooks/common/router';
import { useMenu } from '../context';
import { distributeMegaMenuMasonry, getMegaMenuColumnCount } from '../utils/megaMenuMasonry';
import { allowSecondaryTitleNavigate } from '../utils/megaMenuConfig';
import { buildMegaSections, canNavigateMenu } from '../utils/megaMenuSections';
import type { MenuWithMeta } from '../utils/megaMenuSections';
import MegaSectionNode from './mega-section-node.vue';

defineOptions({
  name: 'HorizontalMenuItem'
});

interface Props {
  menu: MenuWithMeta;
  /** bar=顶部栏；overflow=「更多」浮窗内行 */
  layout?: 'bar' | 'overflow';
}

const props = withDefaults(defineProps<Props>(), {
  layout: 'bar'
});

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

/** 当前一级模块是否允许二级标题跳转 */
function canNavigateSecondaryTitle(menu: App.Global.Menu) {
  return allowSecondaryTitleNavigate(menu);
}

function getMegaMenuClass(menu: App.Global.Menu) {
  const sections = buildMegaSections(menu);
  const sectionCount = sections.length;
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

function handleMenuClick(menu: MenuWithMeta) {
  if (menu.disabled || hasChildren(menu)) return;
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
}

function handleParentLabelClick(menu: MenuWithMeta) {
  if (!hasChildren(menu) || !canNavigateParent(menu)) return;
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
}

function handleLinkClick(menu: MenuWithMeta) {
  if (menu.disabled) return;
  routerPushByKeyWithMetaQuery(menu.routeKey as RouteKey, menu);
}

function adjustMegaMenuPosition(event: MouseEvent) {
  const trigger = event.currentTarget as HTMLElement | null;
  const panel = trigger?.querySelector('.mega-menu') as HTMLElement | null;
  if (!trigger || !panel) return;

  panel.style.left = '';
  panel.style.right = '';
  panel.style.transformOrigin = 'top left';

  if (props.layout === 'overflow') {
    panel.style.left = 'auto';
    panel.style.right = '100%';
    panel.style.transformOrigin = 'top right';
    return;
  }

  panel.style.left = '0';
  panel.style.right = 'auto';

  requestAnimationFrame(() => {
    const padding = 12;
    const triggerRect = trigger.getBoundingClientRect();
    const menuWidth = panel.offsetWidth;
    const viewportWidth = window.innerWidth;
    let left = 0;

    if (triggerRect.left + menuWidth > viewportWidth - padding) {
      left = triggerRect.width - menuWidth;
    }
    if (triggerRect.left + left < padding) {
      left = padding - triggerRect.left;
    }
    if (triggerRect.left + left + menuWidth > viewportWidth - padding) {
      left = viewportWidth - padding - menuWidth - triggerRect.left;
    }

    panel.style.left = `${left}px`;
    panel.style.transformOrigin = left < 0 ? 'top right' : 'top left';
  });
}
</script>

<template>
  <div
    class="menu-trigger"
    :class="{
      'menu-trigger--bar': layout === 'bar',
      'menu-trigger--overflow': layout === 'overflow',
      'menu-trigger--active': isMenuActive(menu),
      'menu-trigger--disabled': menu.disabled,
      'menu-trigger--has-children': hasChildren(menu)
    }"
    @mouseenter="hasChildren(menu) && adjustMegaMenuPosition($event)"
    @click="handleMenuClick(menu)"
  >
    <div
      class="menu-trigger-label"
      :class="{ 'menu-trigger-label--clickable': hasChildren(menu) && canNavigateParent(menu) }"
      @click.stop="handleParentLabelClick(menu)"
    >
      <component :is="menu.icon" v-if="menu.icon && layout === 'bar'" class="menu-trigger-icon" />
      <span>{{ menu.label }}</span>
    </div>

    <span v-if="hasChildren(menu)" class="arrow-icon" :class="{ 'arrow-icon--overflow': layout === 'overflow' }" />

    <div
      v-if="hasChildren(menu)"
      class="mega-menu"
      :class="[getMegaMenuClass(menu), layout === 'overflow' ? 'mega-menu--side' : '']"
    >
      <div class="mega-menu-body" :class="getMegaMenuBodyClass(menu)">
        <div
          v-for="(column, colIdx) in getMasonryColumns(menu)"
          :key="`${menu.key}-col-${colIdx}`"
          class="mega-menu-column"
        >
          <div
            v-for="section in column"
            :key="section.label + section.links.map(item => item.key).join('-')"
            class="mega-section-wrap"
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
.menu-trigger--bar {
  position: relative;
  display: flex;
  align-items: center;
  gap: 6px;
  height: 100%;
  padding: 0 15px;
  color: rgb(var(--base-text-color));
  cursor: pointer;
  white-space: nowrap;
  transition: background-color 0.2s ease, color 0.2s ease;
  flex-shrink: 0;
}

.menu-trigger--overflow {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 12px;
  color: #3d526b;
  cursor: pointer;
  white-space: nowrap;
  border-radius: 8px;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.menu-trigger--bar:hover,
.menu-trigger--bar.menu-trigger--active {
  color: rgb(var(--primary-color));
  background-color: rgb(var(--primary-color) / 0.12);
}

.menu-trigger--overflow:hover,
.menu-trigger--overflow.menu-trigger--active {
  background: rgba(47, 137, 245, 0.07);
  color: #2f89f5;
}

.menu-trigger--disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.menu-trigger-label {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.menu-trigger-label--clickable {
  cursor: pointer;
}

.menu-trigger-icon {
  font-size: 18px;
  line-height: 1;
  flex-shrink: 0;
}

.arrow-icon {
  display: block;
  width: 6px;
  height: 6px;
  border-top: 2px solid currentColor;
  border-right: 2px solid currentColor;
  transform: rotate(135deg);
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  flex-shrink: 0;
}

.menu-trigger--bar.menu-trigger--has-children:hover .arrow-icon {
  transform: rotate(-45deg);
}

.arrow-icon--overflow {
  border-color: #b0c0d0;
  transform: rotate(45deg);
}

.menu-trigger--overflow.menu-trigger--has-children:hover .arrow-icon--overflow {
  border-color: #2f89f5;
}

.mega-menu {
  position: absolute;
  top: 100%;
  left: 0;
  width: min(400px, calc(100vw - 24px));
  padding-top: 6px;
  background: transparent;
  opacity: 0;
  visibility: hidden;
  transform: translateY(-10px) scale(0.96);
  transform-origin: top left;
  transition:
    opacity 0.3s cubic-bezier(0.4, 0, 0.2, 1),
    visibility 0.3s ease,
    transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 1001;
  cursor: default;
  max-height: 80vh;
  overflow-y: auto;
}

.mega-menu--side {
  top: 0;
  left: auto;
  right: calc(100% + 4px);
  padding-top: 0;
  transform: translateX(8px) scale(0.96);
  transform-origin: top right;
}

.mega-menu-body,
.mega-menu--compact {
  background: #ffffff;
  border: 1px solid #e3ebf5;
  border-radius: 14px;
  box-shadow:
    0 32px 64px rgba(31, 111, 209, 0.16),
    0 8px 20px rgba(0, 0, 0, 0.04);
}

.mega-menu--compact {
  width: auto;
  min-width: 230px;
  max-width: min(320px, calc(100vw - 24px));
}

.mega-menu--wide {
  width: min(720px, calc(100vw - 24px));
}

.mega-menu--wide :deep(.mega-link) {
  font-size: 13px;
  white-space: normal;
  line-height: 1.35;
}

.menu-trigger--has-children:hover .mega-menu {
  opacity: 1;
  visibility: visible;
  transform: translateY(0) scale(1);
}

.menu-trigger--overflow.menu-trigger--has-children:hover .mega-menu--side {
  transform: translateX(0) scale(1);
}

.mega-menu-body {
  padding: 10px 12px 12px;
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  gap: 12px 14px;
  border-radius: 14px;
}

.mega-menu-body--single {
  flex-direction: column;
}

.mega-menu-column {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.mega-section-wrap {
  flex-shrink: 0;
}
</style>
