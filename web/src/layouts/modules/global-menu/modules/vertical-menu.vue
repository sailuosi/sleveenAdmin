<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
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

const route = useRoute();
const appStore = useAppStore();
const themeStore = useThemeStore();
const routeStore = useRouteStore();
const { onMenuValueUpdate } = useMenuValuePush();
const { selectedKey } = useMenu();

const inverted = computed(() => !themeStore.darkMode && themeStore.sider.inverted);
/** dark mode or light inverted sider: transparent menu so DarkModeContainer shows through */
const darkMenu = computed(() => themeStore.darkMode || inverted.value);

const expandedKeys = ref<string[]>([]);

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

watch(
  () => route.name,
  () => {
    updateExpandedKeys();
  },
  { immediate: true }
);
</script>

<template>
  <Teleport :to="`#${GLOBAL_SIDER_MENU_ID}`">
    <SimpleScrollbar>
      <ElMenu
        :key="`${String(selectedKey)}-${expandedKeys.join(',')}`"
        class="border-none!"
        :class="{ 'is-inverted': darkMenu }"
        :default-active="selectedKey || undefined"
        :default-openeds="expandedKeys"
        :collapse="appStore.siderCollapse"
        :collapse-transition="false"
        @select="handleSelect"
      >
        <EpMenuItems :menus="routeStore.menus" />
      </ElMenu>
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
</style>
