<script setup lang="ts">
import { ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { SimpleScrollbar } from '@sa/materials';
import type { RouteKey } from '@elegant-router/types';
import { GLOBAL_HEADER_MENU_ID, GLOBAL_SIDER_MENU_ID } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { useRouteStore } from '@/store/modules/route';
import { useMenuValuePush } from '@/layouts/modules/global-menu/useMenuValuePush';
import EpMenuItems from '@/layouts/modules/global-menu/components/EpMenuItems.vue';
import { findMenu } from '@/layouts/modules/global-menu/utils/findMenu';
import { useMenu, useMixMenuContext } from '../context';

defineOptions({
  name: 'TopHybridHeaderFirst'
});

const route = useRoute();
const appStore = useAppStore();
const themeStore = useThemeStore();
const routeStore = useRouteStore();
const { onMenuValueUpdate } = useMenuValuePush();
const { firstLevelMenus, secondLevelMenus, activeFirstLevelMenuKey, handleSelectFirstLevelMenu } =
  useMixMenuContext('TopHybridHeaderFirst');
const { selectedKey } = useMenu();

const expandedKeys = ref<string[]>([]);

function handleSelectMenu(key: string) {
  handleSelectFirstLevelMenu(key as RouteKey);
}

function handleSelect(key: string) {
  const item = findMenu(secondLevelMenus.value, key);
  onMenuValueUpdate(key, item);
}

function updateExpandedKeys() {
  if (appStore.siderCollapse || !selectedKey.value) {
    expandedKeys.value = [];
    return;
  }
  expandedKeys.value = routeStore.getSelectedMenuKeyPath(selectedKey.value);
}

watch(
  () => route.name,
  () => {
    updateExpandedKeys();
  },
  { immediate: true }
);
</script>

<template>
  <Teleport :to="`#${GLOBAL_HEADER_MENU_ID}`">
    <ElMenu
      mode="horizontal"
      class="border-none! bg-transparent!"
      ellipsis
      :default-active="activeFirstLevelMenuKey || undefined"
      @select="handleSelectMenu"
    >
      <EpMenuItems :menus="firstLevelMenus" />
    </ElMenu>
  </Teleport>
  <Teleport :to="`#${GLOBAL_SIDER_MENU_ID}`">
    <SimpleScrollbar>
      <ElMenu
        :key="`${String(selectedKey)}-${expandedKeys.join(',')}`"
        class="border-none!"
        :default-active="selectedKey || undefined"
        :default-openeds="expandedKeys"
        :collapse="appStore.siderCollapse"
        :collapse-transition="false"
        @select="handleSelect"
      >
        <EpMenuItems :menus="secondLevelMenus" />
      </ElMenu>
    </SimpleScrollbar>
  </Teleport>
</template>

<style scoped></style>
