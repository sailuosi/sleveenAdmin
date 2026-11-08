<script setup lang="ts">
import { useThemeStore } from '@/store/modules/theme';
import LayoutMode from './modules/layout-mode.vue';
import TabSettings from './modules/tab-settings.vue';
import HeaderSettings from './modules/header-settings.vue';
import SiderSettings from './modules/sider-settings.vue';
import FooterSettings from './modules/footer-settings.vue';
import ContentSettings from './modules/content-settings.vue';

defineOptions({
  name: 'LayoutSettings'
});

const themeStore = useThemeStore();
</script>

<template>
  <div class="flex-col-stretch gap-16px">
    <LayoutMode />
    <TabSettings />
    <HeaderSettings />
    <!-- 无侧栏模式：水平菜单 / 顶栏无界 / 底部无界 -->
    <SiderSettings
      v-if="
        themeStore.layout.mode !== 'horizontal' &&
        themeStore.layout.mode !== 'horizontal-card' &&
        themeStore.layout.mode !== 'bottom-card'
      "
    />
    <FooterSettings />
    <ContentSettings />
  </div>
</template>

<style scoped></style>
