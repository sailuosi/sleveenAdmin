<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useTabStore } from '@/store/modules/tab';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';
import CardTabAnchorMenu from '../../global-tab/card-tab-anchor-menu.vue';

defineOptions({
  name: 'SiderDockBar'
});

const appStore = useAppStore();
const themeStore = useThemeStore();
const tabStore = useTabStore();

const tabCount = computed(() => tabStore.tabs.length);
const anchor = computed(() => themeStore.tab.cardAnchor);
const overlayOpen = computed(() => appStore.cardTabOverlayVisible);

function toggleTabs() {
  if (anchor.value !== 'hidden') {
    // docked already — switch to overlay preview by temporarily using open is N/A; just no-op
    return;
  }
  appStore.toggleCardTabOverlay();
}
</script>

<template>
  <div class="sider-dock" :class="{ 'is-collapsed': appStore.siderCollapse }">
    <button
      type="button"
      class="dock-btn"
      :class="{ 'is-active': overlayOpen || anchor !== 'hidden' }"
      :title="$t('theme.layout.tab.cardOpen')"
      @click="toggleTabs"
    >
      <span class="dock-btn__icon-wrap">
        <SvgIcon icon="mdi:tab" class="text-18px" />
        <span v-if="tabCount > 0" class="dock-badge">{{ tabCount > 99 ? '99+' : tabCount }}</span>
      </span>
    </button>

    <CardTabAnchorMenu />

    <button type="button" class="dock-btn is-placeholder" title="消息" disabled>
      <span class="dock-btn__icon-wrap">
        <SvgIcon icon="mdi:bell-outline" class="text-18px" />
      </span>
    </button>
  </div>
</template>

<style scoped>
.sider-dock {
  --uno: shrink-0 flex-y-center justify-around gap-4px px-10px py-10px border-t border-solid border-#000/6;
}

.sider-dock.is-collapsed {
  --uno: flex-col px-6px;
}

.dock-btn {
  --uno: relative size-36px flex-center rd-10px border-none bg-transparent text-base-text cursor-pointer hover:bg-primary/10 transition-colors;
}

.dock-btn.is-active {
  --uno: text-primary bg-primary/12;
}

.dock-btn.is-placeholder {
  --uno: opacity-45 cursor-not-allowed;
}

.dock-btn__icon-wrap {
  --uno: relative inline-flex;
}

.dock-badge {
  position: absolute;
  top: -7px;
  right: -9px;
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  border-radius: 999px;
  background: rgb(var(--error-color, 245 34 45));
  color: #fff;
  font-size: 10px;
  line-height: 16px;
  text-align: center;
  pointer-events: none;
}
</style>
