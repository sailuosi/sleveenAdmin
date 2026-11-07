<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { SimpleScrollbar } from '@sa/materials';
import { useBoolean } from '@sa/hooks';
import type { RouteKey } from '@elegant-router/types';
import { GLOBAL_SIDER_MENU_ID } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { useRouteStore } from '@/store/modules/route';
import { useMenuValuePush } from '@/layouts/modules/global-menu/useMenuValuePush';
import EpMenuItems from '@/layouts/modules/global-menu/components/EpMenuItems.vue';
import { findMenu } from '@/layouts/modules/global-menu/utils/findMenu';
import { $t } from '@/locales';
import { useMenu, useMixMenuContext } from '../context';

defineOptions({
  name: 'VerticalMixMenu'
});

const route = useRoute();
const appStore = useAppStore();
const themeStore = useThemeStore();
const routeStore = useRouteStore();
const { onMenuValueUpdate } = useMenuValuePush();
const { bool: drawerVisible, setBool: setDrawerVisible } = useBoolean();
const {
  firstLevelMenus,
  secondLevelMenus,
  activeFirstLevelMenuKey,
  isActiveFirstLevelMenuHasChildren,
  getActiveFirstLevelMenuKey,
  handleSelectFirstLevelMenu
} = useMixMenuContext('VerticalMixMenu');
const { selectedKey } = useMenu();

const inverted = computed(() => !themeStore.darkMode && themeStore.sider.inverted);
/** dark mode or light inverted sider: transparent menu so DarkModeContainer shows through */
const darkMenu = computed(() => themeStore.darkMode || inverted.value);

const hasChildMenus = computed(() => secondLevelMenus.value.length > 0);

const showDrawer = computed(() => hasChildMenus.value && (drawerVisible.value || appStore.mixSiderFixed));

function handleSelectMenu(key: RouteKey) {
  handleSelectFirstLevelMenu(key);

  if (isActiveFirstLevelMenuHasChildren.value) {
    setDrawerVisible(true);
  }
}

function handleResetActiveMenu() {
  setDrawerVisible(false);

  if (!appStore.mixSiderFixed) {
    getActiveFirstLevelMenuKey();
  }
}

const expandedKeys = ref<string[]>([]);

function updateExpandedKeys() {
  if (appStore.siderCollapse || !selectedKey.value) {
    expandedKeys.value = [];
    return;
  }
  expandedKeys.value = routeStore.getSelectedMenuKeyPath(selectedKey.value);
}

function handleSelect(key: string) {
  const item = findMenu(secondLevelMenus.value, key);
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
    <div class="h-full flex" @mouseleave="handleResetActiveMenu">
      <FirstLevelMenu
        :menus="firstLevelMenus"
        :active-menu-key="activeFirstLevelMenuKey"
        :inverted="inverted"
        :sider-collapse="appStore.siderCollapse"
        :dark-mode="themeStore.darkMode"
        :theme-color="themeStore.themeColor"
        @select="handleSelectMenu"
        @toggle-sider-collapse="appStore.toggleSiderCollapse"
      >
        <GlobalLogo :show-title="false" :style="{ height: themeStore.header.height + 'px' }" />
      </FirstLevelMenu>
      <div
        class="relative h-full transition-width-300"
        :style="{ width: appStore.mixSiderFixed && hasChildMenus ? themeStore.sider.mixChildMenuWidth + 'px' : '0px' }"
      >
        <DarkModeContainer
          class="absolute-lt h-full flex-col-stretch nowrap-hidden shadow-sm transition-all-300"
          :inverted="inverted"
          :style="{ width: showDrawer ? themeStore.sider.mixChildMenuWidth + 'px' : '0px' }"
        >
          <header class="flex-y-center justify-between px-12px" :style="{ height: themeStore.header.height + 'px' }">
            <h2 class="text-16px text-primary font-bold">{{ $t('system.title') }}</h2>
            <PinToggler
              :pin="appStore.mixSiderFixed"
              :class="{ 'text-white:88 !hover:text-white': inverted }"
              @click="appStore.toggleMixSiderFixed"
            />
          </header>
          <SimpleScrollbar>
            <ElMenu
              :key="`${String(selectedKey)}-${expandedKeys.join(',')}`"
              class="border-none!"
              :class="{ 'is-inverted': darkMenu }"
              :default-active="selectedKey || undefined"
              :default-openeds="expandedKeys"
              :collapse-transition="false"
              @select="handleSelect"
            >
              <EpMenuItems :menus="secondLevelMenus" />
            </ElMenu>
          </SimpleScrollbar>
        </DarkModeContainer>
      </div>
    </div>
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
