<script setup lang="ts">
import { computed, ref } from 'vue';
import { useFullscreen } from '@vueuse/core';
import { useAppStore } from '@/store/modules/app';
import { useAuthStore } from '@/store/modules/auth';
import { useThemeStore } from '@/store/modules/theme';
import { useRouterPush } from '@/hooks/common/router';
import { $t } from '@/locales';
import CardTabAnchorMenu from '../../global-tab/card-tab-anchor-menu.vue';

defineOptions({
  name: 'SiderSettingsPopover'
});

const appStore = useAppStore();
const authStore = useAuthStore();
const themeStore = useThemeStore();
const { toLogin } = useRouterPush();
const { isFullscreen, toggle: toggleFullscreen } = useFullscreen();

const visible = ref(false);

const displayName = computed(() => authStore.userInfo.userName || authStore.userInfo.userId || '-');
const displayCode = computed(() => `${authStore.userInfo.userId || ''}`.trim());

function loginOrRegister() {
  visible.value = false;
  toLogin();
}

function logout() {
  visible.value = false;
  window.$dialog?.info({
    title: $t('common.tip'),
    content: $t('common.logoutConfirm'),
    positiveText: $t('common.confirm'),
    negativeText: $t('common.cancel'),
    onPositiveClick: () => {
      authStore.resetStore(true);
    }
  });
}

function openThemeConfig() {
  visible.value = false;
  appStore.openThemeDrawer();
}

function openTabs() {
  visible.value = false;
  if (themeStore.tab.cardAnchor !== 'hidden') return;
  appStore.openCardTabOverlay();
}
</script>

<template>
  <ElPopover v-model:visible="visible" placement="bottom-end" :width="280" trigger="click" :teleported="true">
    <template #reference>
      <button
        type="button"
        class="sider-gear-btn"
        :title="$t('icon.themeConfig')"
        @click.stop
      >
        <SvgIcon icon="ph:gear" class="text-16px" />
      </button>
    </template>

    <div class="flex-col gap-12px p-4px">
      <div v-if="authStore.isLogin" class="profile-card">
        <div class="flex items-center gap-10px">
          <div class="profile-avatar">
            <SvgIcon icon="ph:user-circle" class="text-32px" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="truncate text-14px font-600">{{ displayName }}</div>
            <div v-if="displayCode" class="truncate text-12px text-gray-500">{{ displayCode }}</div>
          </div>
        </div>
      </div>
      <ElButton v-else class="w-full" @click="loginOrRegister">
        {{ $t('page.login.common.loginOrRegister') }}
      </ElButton>

      <div class="flex-col gap-8px">
        <div class="action-row">
          <span>{{ $t('theme.appearance.themeSchema.title') }}</span>
          <ThemeSchemaSwitch
            :theme-schema="themeStore.themeScheme"
            :is-dark="themeStore.darkMode"
            @switch="themeStore.toggleThemeScheme"
          />
        </div>

        <button type="button" class="action-item" @click="openThemeConfig">
          <SvgIcon icon="majesticons:color-swatch-line" class="text-16px" />
          <span>{{ $t('icon.themeConfig') }}</span>
        </button>

        <div v-if="themeStore.header.multilingual.visible" class="action-row">
          <span>{{ $t('icon.lang') }}</span>
          <LangSwitch
            :lang="appStore.locale"
            :lang-options="appStore.localeOptions"
            @change-lang="appStore.changeLocale"
          />
        </div>

        <button v-if="!appStore.isMobile" type="button" class="action-item" @click="toggleFullscreen">
          <SvgIcon :icon="isFullscreen ? 'gridicons-fullscreen-exit' : 'gridicons-fullscreen'" class="text-16px" />
          <span>{{ isFullscreen ? $t('icon.fullscreenExit') : $t('icon.fullscreen') }}</span>
        </button>

        <div class="action-row">
          <button type="button" class="action-item flex-1" @click="openTabs">
            <SvgIcon icon="mdi:tab" class="text-16px" />
            <span>{{ $t('theme.layout.tab.cardOpen') }}</span>
          </button>
        </div>
        <div class="action-row">
          <span>{{ $t('theme.layout.tab.cardAnchor') }}</span>
          <CardTabAnchorMenu :compact="false" />
        </div>

        <button v-if="authStore.isLogin" type="button" class="action-item text-error!" @click="logout">
          <SvgIcon icon="ph:sign-out" class="text-16px" />
          <span>{{ $t('common.logout') }}</span>
        </button>
      </div>
    </div>
  </ElPopover>
</template>

<style scoped>
.sider-gear-btn {
  --uno: size-28px flex-center rd-full text-base-text hover:bg-primary/10 transition-colors cursor-pointer border-none bg-transparent shrink-0;
}

.profile-card {
  --uno: rd-10px bg-layout p-10px;
}

.profile-avatar {
  --uno: size-40px flex-center rd-full bg-container text-primary;
}

.action-row {
  --uno: flex-y-center justify-between gap-8px text-13px;
}

.action-item {
  --uno: w-full flex-y-center gap-8px rd-8px px-8px py-8px text-13px text-left cursor-pointer border-none bg-transparent hover:bg-primary/8 transition-colors;
}

</style>
