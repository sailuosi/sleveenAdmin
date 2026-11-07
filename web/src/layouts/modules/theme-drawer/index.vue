<script setup lang="ts">
import { computed, ref } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { $t } from '@/locales';
import AppearanceSettings from './modules/appearance/index.vue';
import LayoutSettings from './modules/layout/index.vue';
import GeneralSettings from './modules/general/index.vue';
import ConfigOperation from './modules/config-operation.vue';
import PresetSettings from './modules/preset/index.vue';

defineOptions({
  name: 'ThemeDrawer'
});

const appStore = useAppStore();
const activeTab = ref('appearance');

const drawerWidth = computed(() => {
  const width = 400;

  if (appStore.isMobile) {
    return Math.min(window.innerWidth * 0.9, width);
  }

  return width;
});
</script>

<template>
  <ElDrawer
    v-model="appStore.themeDrawerVisible"
    class="theme-drawer"
    :title="$t('theme.themeDrawerTitle')"
    :size="drawerWidth"
    destroy-on-close
  >
    <ElTabs v-model="activeTab" class="theme-drawer-tabs">
      <ElTabPane :label="$t('theme.tabs.appearance')" name="appearance" lazy>
        <AppearanceSettings />
      </ElTabPane>
      <ElTabPane :label="$t('theme.tabs.layout')" name="layout" lazy>
        <LayoutSettings />
      </ElTabPane>
      <ElTabPane :label="$t('theme.tabs.general')" name="general" lazy>
        <GeneralSettings />
      </ElTabPane>
      <ElTabPane :label="$t('theme.tabs.preset')" name="preset" lazy>
        <PresetSettings />
      </ElTabPane>
    </ElTabs>

    <template #footer>
      <ConfigOperation />
    </template>
  </ElDrawer>
</template>

<style scoped>
:deep(.el-tabs__content) {
  padding: 0;
}
</style>

<!-- 覆盖 global.css 中 .el-drawer__body>div{height:100%}，避免空 Tabs 撑出一屏空白 -->
<style>
.el-drawer.theme-drawer .el-drawer__body > div {
  height: auto;
  width: 100%;
}
</style>
