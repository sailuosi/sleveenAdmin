<script setup lang="ts">
import { computed } from 'vue';
import { useFullscreen } from '@vueuse/core';
import { GLOBAL_HEADER_MENU_ID } from '@/constants/app';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import GlobalLogo from '../global-logo/index.vue';
import GlobalBreadcrumb from '../global-breadcrumb/index.vue';
import ThemeButton from './components/theme-button.vue';
import UserAvatar from './components/user-avatar.vue';

defineOptions({
  name: 'GlobalHeader'
});

interface Props {
  /** Whether to show the logo */
  showLogo?: App.Global.HeaderProps['showLogo'];
  /** Whether to show the menu toggler */
  showMenuToggler?: App.Global.HeaderProps['showMenuToggler'];
  /** Whether to show the menu */
  showMenu?: App.Global.HeaderProps['showMenu'];
}

defineProps<Props>();

const appStore = useAppStore();
const themeStore = useThemeStore();
const { isFullscreen, toggle } = useFullscreen();

const isHorizontalCard = computed(() => themeStore.layout.mode === 'horizontal-card');

const headerShellClass = computed(() =>
  isHorizontalCard.value ? 'header-island-shell' : 'shadow-header'
);

const logoStyle = computed(() => {
  if (isHorizontalCard.value) {
    return { width: 'auto', paddingInline: '12px' };
  }
  return { width: `${themeStore.sider.width}px` };
});
</script>

<template>
  <DarkModeContainer class="h-full flex-y-center overflow-visible" :class="headerShellClass">
    <GlobalLogo v-if="showLogo" class="h-full shrink-0" :style="logoStyle" />
    <MenuToggler v-if="showMenuToggler" :collapsed="appStore.siderCollapse" @click="appStore.toggleSiderCollapse" />
    <div
      v-if="showMenu"
      :id="GLOBAL_HEADER_MENU_ID"
      class="header-menu-slot h-full flex-y-center flex-1 min-w-0 overflow-visible"
    ></div>
    <div v-else class="h-full flex-y-center flex-1-hidden">
      <GlobalBreadcrumb v-if="!appStore.isMobile" class="ml-12px" />
    </div>
    <div class="h-full flex-y-center justify-end head-btns shrink-0">
      <FullScreen v-if="!appStore.isMobile" :full="isFullscreen" @click="toggle" />
      <LangSwitch
        v-if="themeStore.header.multilingual.visible"
        :lang="appStore.locale"
        :lang-options="appStore.localeOptions"
        @change-lang="appStore.changeLocale"
      />
      <ThemeSchemaSwitch
        :theme-schema="themeStore.themeScheme"
        :is-dark="themeStore.darkMode"
        @switch="themeStore.toggleThemeScheme"
      />
      <ThemeButton />
      <UserAvatar />
    </div>
  </DarkModeContainer>
</template>

<style scoped>
.header-island-shell {
  background: rgba(255, 255, 255, 0.55) !important;
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
  border-radius: inherit;
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.08);
  overflow: visible;
}

:global(html.dark) .header-island-shell {
  background: rgba(30, 30, 40, 0.55) !important;
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
}
</style>
