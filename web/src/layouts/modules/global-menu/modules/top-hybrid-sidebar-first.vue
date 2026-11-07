<script setup lang="ts">
import type { RouteKey } from '@elegant-router/types';
import { GLOBAL_HEADER_MENU_ID, GLOBAL_SIDER_MENU_ID } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { useMenuValuePush } from '@/layouts/modules/global-menu/useMenuValuePush';
import EpMenuItems from '@/layouts/modules/global-menu/components/EpMenuItems.vue';
import { findMenu } from '@/layouts/modules/global-menu/utils/findMenu';
import FirstLevelMenu from '../components/first-level-menu.vue';
import { useMenu, useMixMenuContext } from '../context';

defineOptions({
  name: 'TopHybridSidebarFirst'
});

const appStore = useAppStore();
const themeStore = useThemeStore();
const { onMenuValueUpdate } = useMenuValuePush();
const { firstLevelMenus, secondLevelMenus, activeFirstLevelMenuKey, handleSelectFirstLevelMenu } =
  useMixMenuContext('TopHybridSidebarFirst');
const { selectedKey } = useMenu();

function handleSelectMenu(key: RouteKey) {
  handleSelectFirstLevelMenu(key);
}

function handleSelect(key: string) {
  const item = findMenu(secondLevelMenus.value, key);
  onMenuValueUpdate(key, item);
}
</script>

<template>
  <Teleport :to="`#${GLOBAL_HEADER_MENU_ID}`">
    <ElMenu
      mode="horizontal"
      class="border-none! bg-transparent!"
      ellipsis
      :default-active="selectedKey || undefined"
      @select="handleSelect"
    >
      <EpMenuItems :menus="secondLevelMenus" />
    </ElMenu>
  </Teleport>
  <Teleport :to="`#${GLOBAL_SIDER_MENU_ID}`">
    <div class="h-full pt-2">
      <FirstLevelMenu
        :menus="firstLevelMenus"
        :active-menu-key="activeFirstLevelMenuKey"
        :sider-collapse="appStore.siderCollapse"
        :dark-mode="themeStore.darkMode"
        :theme-color="themeStore.themeColor"
        @select="handleSelectMenu"
        @toggle-sider-collapse="appStore.toggleSiderCollapse"
      />
    </div>
  </Teleport>
</template>

<style scoped></style>
