<script setup lang="ts">
import { computed } from 'vue';
import zhCn from 'element-plus/es/locale/lang/zh-cn.mjs';
import en from 'element-plus/es/locale/lang/en.mjs';
import { useAppStore } from './store/modules/app';
import { useThemeStore } from './store/modules/theme';
import AppWatermark from '@/components/common/AppWatermark.vue';

defineOptions({
  name: 'App'
});

const appStore = useAppStore();
const themeStore = useThemeStore();

const elementLocale = computed(() => (appStore.locale === 'zh-CN' ? zhCn : en));

/** 与主题布局 mode 同名，挂在 App 根便于全局 CSS 选择（如 .vertical-card） */
const layoutModeClass = computed(() => themeStore.layout.mode);
</script>

<template>
  <ElConfigProvider :locale="elementLocale">
    <div class="h-full" :class="layoutModeClass">
      <AppProvider>
        <RouterView class="bg-layout" />
        <AppWatermark
          v-if="themeStore.watermark.visible"
          :content="themeStore.watermarkContent"
          :font-size="14"
          :width="340"
          :height="160"
          :rotate="-15"
          font-color="rgba(128, 128, 128, .08)"
          :z-index="9999"
        />
      </AppProvider>
    </div>
  </ElConfigProvider>
</template>

<style scoped></style>
