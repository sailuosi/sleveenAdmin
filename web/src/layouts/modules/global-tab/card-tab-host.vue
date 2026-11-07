<script setup lang="ts">
import { computed, onUnmounted } from 'vue';
import { onKeyStroke } from '@vueuse/core';
import { useAppStore } from '@/store/modules/app';
import { useTabStore } from '@/store/modules/tab';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';
import GlobalTab from './index.vue';
import CardTabAnchorMenu from './card-tab-anchor-menu.vue';

defineOptions({
  name: 'CardTabHost'
});

interface Props {
  /** overlay panel | dock strip */
  variant?: 'overlay' | 'dock';
  /**
   * Force top/bottom docks to fixed float (borderless page).
   * Right dock is always fixed.
   */
  forceFloat?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'overlay',
  forceFloat: false
});

const appStore = useAppStore();
const themeStore = useThemeStore();
const tabStore = useTabStore();

const anchor = computed(() => themeStore.tab.cardAnchor);
const showOverlay = computed(
  () => props.variant === 'overlay' && anchor.value === 'hidden' && appStore.cardTabOverlayVisible
);

/** top/bottom embedded in layout flow — skipped when forceFloat */
const showEmbedDock = computed(
  () =>
    props.variant === 'dock' &&
    !props.forceFloat &&
    themeStore.tab.visible &&
    (anchor.value === 'top' || anchor.value === 'bottom')
);

const showRightDock = computed(
  () => props.variant === 'dock' && themeStore.tab.visible && anchor.value === 'right'
);

/** borderless: top/bottom as fixed float strips */
const showFloatEdgeDock = computed(
  () =>
    props.variant === 'dock' &&
    props.forceFloat &&
    themeStore.tab.visible &&
    (anchor.value === 'top' || anchor.value === 'bottom')
);

const rightDockStyle = computed(() => {
  const inset = 16;
  return {
    top: `${inset}px`,
    right: `${inset}px`,
    bottom: `${inset}px`,
    width: '220px'
  };
});

const floatEdgeStyle = computed(() => {
  const inset = 16;
  const h = themeStore.tab.height;
  if (anchor.value === 'bottom') {
    return {
      left: `${inset}px`,
      right: `${inset}px`,
      bottom: `${inset}px`,
      height: `${h}px`
    };
  }
  return {
    left: `${inset}px`,
    right: `${inset}px`,
    top: `${inset}px`,
    height: `${h}px`
  };
});

function closeOverlay() {
  appStore.closeCardTabOverlay();
}

function handleSelectRightTab(tab: App.Global.Tab) {
  tabStore.switchRouteByTab(tab);
}

onKeyStroke('Escape', () => {
  if (appStore.cardTabOverlayVisible) closeOverlay();
});

onUnmounted(() => {
  if (anchor.value === 'hidden') appStore.closeCardTabOverlay();
});
</script>

<template>
  <Teleport v-if="variant === 'overlay'" to="body">
    <div v-if="showOverlay" class="card-tab-mask" @click.self="closeOverlay">
      <div class="card-tab-panel shadow-lg">
        <div class="card-tab-panel__bar">
          <span class="text-13px font-600">{{ $t('theme.layout.tab.title') }}</span>
          <div class="flex-y-center gap-4px">
            <CardTabAnchorMenu />
            <button type="button" class="card-tab-icon-btn" :title="$t('common.close')" @click="closeOverlay">
              <SvgIcon icon="mdi:close" class="text-16px" />
            </button>
          </div>
        </div>
        <div class="card-tab-panel__body">
          <GlobalTab />
        </div>
      </div>
    </div>
  </Teleport>

  <!-- Embedded top / bottom (document flow) -->
  <div
    v-else-if="showEmbedDock"
    class="card-tab-dock-embed"
    :class="anchor === 'bottom' ? 'is-bottom' : 'is-top'"
    :style="{ height: `${themeStore.tab.height}px` }"
  >
    <div class="card-tab-dock__main min-w-0 flex-1">
      <GlobalTab />
    </div>
    <CardTabAnchorMenu class="mx-8px" />
  </div>

  <!-- Borderless: top / bottom fixed float -->
  <Teleport v-else-if="showFloatEdgeDock" to="body">
    <div
      class="card-tab-dock-fixed card-tab-dock--edge shadow-md"
      :class="anchor === 'bottom' ? 'is-bottom' : 'is-top'"
      :style="floatEdgeStyle"
    >
      <div class="card-tab-dock__main min-w-0 flex-1">
        <GlobalTab />
      </div>
      <CardTabAnchorMenu class="mx-8px" />
    </div>
  </Teleport>

  <!-- Right stays as floating side panel -->
  <Teleport v-else-if="showRightDock" to="body">
    <div class="card-tab-dock-fixed card-tab-dock--right shadow-md" :style="rightDockStyle">
      <div class="card-tab-dock__right-head">
        <span class="text-13px font-600">{{ $t('theme.layout.tab.title') }}</span>
        <CardTabAnchorMenu />
      </div>
      <div class="card-tab-dock__right-list">
        <button
          v-for="tab in tabStore.tabs"
          :key="tab.id"
          type="button"
          class="right-tab-item"
          :class="{ 'is-active': tab.id === tabStore.activeTabId }"
          @click="handleSelectRightTab(tab)"
        >
          <SvgIcon :icon="tab.icon" :local-icon="tab.localIcon" class="text-16px shrink-0" />
          <span class="min-w-0 flex-1 ellipsis-text text-left">{{ tab.label }}</span>
          <button
            v-if="!tabStore.isTabRetain(tab.id)"
            type="button"
            class="right-tab-close"
            @click.stop="tabStore.removeTab(tab.id)"
          >
            <SvgIcon icon="mdi:close" class="text-14px" />
          </button>
        </button>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.card-tab-mask {
  --uno: fixed inset-0 z-2000 flex items-start justify-end bg-black/20 p-16px;
}

.card-tab-panel {
  --uno: mt-48px w-full max-w-720px overflow-hidden rd-12px;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.08);
}

html.dark .card-tab-panel {
  background: rgba(30, 30, 40, 0.55);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
}

.card-tab-panel__bar {
  --uno: flex-y-center justify-between border-b border-solid border-#000/6 px-12px py-8px;
}

.card-tab-panel__body {
  --uno: min-h-42px;
}

.card-tab-dock-embed {
  --uno: w-full flex-y-center shrink-0;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
}

html.dark .card-tab-dock-embed {
  background: rgba(30, 30, 40, 0.55);
  border-color: rgba(255, 255, 255, 0.12);
}

.card-tab-dock-embed.is-top {
  --uno: border-b border-solid border-#000/6;
}

.card-tab-dock-embed.is-bottom {
  --uno: border-t border-solid border-#000/6;
}

.card-tab-dock-fixed {
  --uno: fixed z-100 flex rd-12px overflow-hidden;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.08);
}

html.dark .card-tab-dock-fixed {
  background: rgba(30, 30, 40, 0.55);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
}

.card-tab-dock--edge {
  --uno: flex-y-center;
}

.card-tab-dock--right {
  --uno: flex-col;
}

.card-tab-dock__right-head {
  --uno: flex-y-center justify-between px-12px py-10px border-b border-solid border-#000/6;
}

.card-tab-dock__right-list {
  --uno: flex-1 overflow-y-auto p-8px flex-col gap-4px;
}

.right-tab-item {
  --uno: w-full flex-y-center gap-8px rd-8px px-10px py-8px border-none bg-transparent text-base-text cursor-pointer hover:bg-primary/10;
}

.right-tab-item.is-active {
  --uno: text-primary bg-primary/12;
}

.right-tab-close {
  --uno: size-22px flex-center rd-4px border-none bg-transparent opacity-50 hover:opacity-100 cursor-pointer;
}

.card-tab-icon-btn {
  --uno: size-28px flex-center rd-6px cursor-pointer border-none bg-transparent text-base-text hover:bg-primary/10;
}
</style>
