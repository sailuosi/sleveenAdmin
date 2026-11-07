<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useResizeObserver } from '@vueuse/core';
import { GLOBAL_HEADER_MENU_ID } from '@/constants/app';
import { useRouteStore } from '@/store/modules/route';
import { useMenu } from '../context';
import HorizontalMenuItem from '../components/horizontal-menu-item.vue';
import type { MenuWithMeta } from '../utils/megaMenuSections';

defineOptions({
  name: 'HorizontalMenu'
});

const routeStore = useRouteStore();
const { selectedKey } = useMenu();

const navRef = ref<HTMLElement | null>(null);
const measureRef = ref<HTMLElement | null>(null);
const visibleCount = ref<number>(Number.POSITIVE_INFINITY);

const menus = computed(() => routeStore.menus as MenuWithMeta[]);

const visibleMenus = computed(() => menus.value.slice(0, visibleCount.value));
const overflowMenus = computed(() => menus.value.slice(visibleCount.value));
const hasOverflow = computed(() => overflowMenus.value.length > 0);

const hasOverflowActive = computed(() => overflowMenus.value.some(menu => isMenuActive(menu)));

function isMenuActive(menu: App.Global.Menu) {
  const key = selectedKey.value;
  if (!key) return false;
  if (key === menu.key) return true;
  return key.startsWith(`${menu.key}_`);
}

async function recalculateVisibleCount() {
  await nextTick();

  const container = navRef.value;
  const measure = measureRef.value;
  const total = menus.value.length;

  if (!container || !measure || total === 0) {
    visibleCount.value = total;
    return;
  }

  const containerWidth = container.clientWidth;
  if (containerWidth <= 0) {
    visibleCount.value = total;
    return;
  }

  const itemEls = Array.from(measure.querySelectorAll('[data-menu-measure]')) as HTMLElement[];
  const widths = itemEls.map(el => el.getBoundingClientRect().width);
  const moreEl = measure.querySelector('[data-more-measure]') as HTMLElement | null;
  const moreWidth = moreEl?.getBoundingClientRect().width ?? 72;
  const sumAll = widths.reduce((sum, width) => sum + width, 0);

  if (sumAll <= containerWidth) {
    visibleCount.value = total;
    return;
  }

  let used = 0;
  let count = 0;

  for (let i = 0; i < total; i++) {
    const remaining = total - i - 1;
    const reserve = remaining > 0 ? moreWidth : 0;
    if (used + widths[i] + reserve <= containerWidth) {
      used += widths[i];
      count += 1;
    } else {
      break;
    }
  }

  visibleCount.value = Math.max(0, count);
}

useResizeObserver(navRef, () => {
  recalculateVisibleCount();
});

watch(menus, () => recalculateVisibleCount(), { deep: true });
watch(selectedKey, () => recalculateVisibleCount());

onMounted(() => {
  recalculateVisibleCount();
});
</script>

<template>
  <Teleport :to="`#${GLOBAL_HEADER_MENU_ID}`">
    <nav ref="navRef" class="horizontal-mega-menu h-full flex-y-center">
      <HorizontalMenuItem
        v-for="menu in visibleMenus"
        :key="menu.key"
        :menu="menu"
        layout="bar"
      />

      <div
        v-if="hasOverflow"
        class="menu-trigger-more"
        :class="{ 'menu-trigger-more--active': hasOverflowActive }"
      >
        <span class="menu-trigger-more__label">更多</span>
        <span class="menu-trigger-more__arrow" />

        <div class="more-panel">
          <HorizontalMenuItem
            v-for="menu in overflowMenus"
            :key="menu.key"
            :menu="menu"
            layout="overflow"
          />
        </div>
      </div>
    </nav>

    <!-- 隐藏测量层：用于计算各一级菜单与「更多」按钮宽度 -->
    <div ref="measureRef" class="menu-measure-layer" aria-hidden="true">
      <div
        v-for="menu in menus"
        :key="`measure-${menu.key}`"
        data-menu-measure
        class="menu-measure-item"
      >
        <span v-if="menu.icon" class="menu-measure-icon" />
        <span>{{ menu.label }}</span>
        <span v-if="menu.children?.length" class="menu-measure-arrow" />
      </div>
      <div data-more-measure class="menu-measure-more">
        <span>更多</span>
        <span class="menu-measure-arrow" />
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.horizontal-mega-menu {
  display: flex;
  align-items: stretch;
  flex: 1;
  min-width: 0;
  width: 100%;
  overflow: visible;
}

.menu-trigger-more {
  position: relative;
  display: flex;
  align-items: center;
  gap: 6px;
  height: 100%;
  padding: 0 15px;
  color: rgb(var(--base-text-color));
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.menu-trigger-more:hover,
.menu-trigger-more--active {
  color: rgb(var(--primary-color));
  background-color: rgb(var(--primary-color) / 0.12);
}

.menu-trigger-more__arrow {
  display: block;
  width: 6px;
  height: 6px;
  border-top: 2px solid currentColor;
  border-right: 2px solid currentColor;
  transform: rotate(135deg);
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}

.menu-trigger-more:hover .menu-trigger-more__arrow {
  transform: rotate(-45deg);
}

.more-panel {
  position: absolute;
  top: 100%;
  right: 0;
  min-width: 168px;
  max-width: min(240px, calc(100vw - 24px));
  padding: 6px;
  padding-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 2px;
  background: #ffffff;
  border: 1px solid #e3ebf5;
  border-radius: 12px;
  box-shadow:
    0 24px 48px rgba(31, 111, 209, 0.14),
    0 6px 16px rgba(0, 0, 0, 0.04);
  opacity: 0;
  visibility: hidden;
  transform: translateY(-8px) scale(0.98);
  transform-origin: top right;
  transition:
    opacity 0.25s ease,
    visibility 0.25s ease,
    transform 0.25s ease;
  z-index: 1000;
  cursor: default;
}

.menu-trigger-more:hover .more-panel {
  opacity: 1;
  visibility: visible;
  transform: translateY(0) scale(1);
}

.more-panel :deep(.menu-trigger--overflow) {
  position: relative;
}

.menu-measure-layer {
  position: fixed;
  top: -9999px;
  left: 0;
  display: flex;
  align-items: stretch;
  visibility: hidden;
  pointer-events: none;
  white-space: nowrap;
}

.menu-measure-item,
.menu-measure-more {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 56px;
  padding: 0 15px;
  font-size: 14px;
}

.menu-measure-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

.menu-measure-arrow {
  width: 6px;
  height: 6px;
  border-top: 2px solid transparent;
  border-right: 2px solid transparent;
  flex-shrink: 0;
}
</style>
