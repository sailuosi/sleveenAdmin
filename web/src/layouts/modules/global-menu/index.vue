<script setup lang="ts">
import { computed } from 'vue';
import type { Component } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import VerticalMenu from './modules/vertical-menu.vue';
import VerticalMixMenu from './modules/vertical-mix-menu.vue';
import VerticalHybridHeaderFirst from './modules/vertical-hybrid-header-first.vue';
import HorizontalMenu from './modules/horizontal-menu.vue';
import BottomDockMenu from './modules/bottom-dock-menu.vue';
import TopHybridSidebarFirst from './modules/top-hybrid-sidebar-first.vue';
import TopHybridHeaderFirst from './modules/top-hybrid-header-first.vue';

defineOptions({
  name: 'GlobalMenu'
});

const appStore = useAppStore();
const themeStore = useThemeStore();

const activeMenu = computed(() => {
  const menuMap: Record<UnionKey.ThemeLayoutMode, Component> = {
    vertical: VerticalMenu,
    'vertical-mix': VerticalMixMenu,
    'vertical-hybrid-header-first': VerticalHybridHeaderFirst,
    'vertical-card': VerticalMenu,
    horizontal: HorizontalMenu,
    'horizontal-card': HorizontalMenu,
    'bottom-card': BottomDockMenu,
    'top-hybrid-sidebar-first': TopHybridSidebarFirst,
    'top-hybrid-header-first': TopHybridHeaderFirst
  };

  return menuMap[themeStore.layout.mode];
});

const reRenderVertical = computed(
  () => (themeStore.layout.mode === 'vertical' || themeStore.layout.mode === 'vertical-card') && appStore.isMobile
);
</script>

<template>
  <component :is="activeMenu" :key="reRenderVertical" />
</template>

<style scoped></style>
