<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';

defineOptions({
  name: 'BorderlessDiv'
});

type Edge = number | string;

interface Props {
  /** 上边留白（px 或 CSS 长度）；未传且 auto 时按页签推算 */
  marginTop?: Edge;
  /** 右边留白 */
  marginRight?: Edge;
  /** 下边留白 */
  marginBottom?: Edge;
  /** 左边留白（侧栏） */
  marginLeft?: Edge;
  /**
   * 未显式传入的边，按当前侧栏宽度 / 页签锚定自动推算
   * @default true
   */
  auto?: boolean;
  /** 内容区额外 class */
  contentClass?: string;
}

const props = withDefaults(defineProps<Props>(), {
  auto: true,
  contentClass: ''
});

/** 与 vertical-card / 悬浮页签一致的视口边距 */
const CHROME_INSET = 16;
/** 浮层与内容之间的额外间隙 */
const CHROME_GAP = 8;
/** 右侧页签面板宽度（与 CardTabHost 一致） */
const RIGHT_TAB_WIDTH = 220;

const appStore = useAppStore();
const themeStore = useThemeStore();

function toCss(value: Edge | undefined, fallbackPx: number): string {
  if (value === undefined || value === null || value === '') return `${fallbackPx}px`;
  return typeof value === 'number' ? `${value}px` : String(value);
}

const isTopOnlyLayout = computed(() => {
  const mode = themeStore.layout.mode;
  return mode === 'horizontal' || mode === 'horizontal-card' || mode === 'bottom-card';
});

/** 按壳层浮层推算建议四边 */
const autoEdges = computed(() => {
  let top = CHROME_INSET;
  let right = CHROME_INSET;
  let bottom = CHROME_INSET;
  let left = CHROME_INSET;

  if (!appStore.isMobile && !isTopOnlyLayout.value) {
    const siderW = appStore.siderCollapse ? themeStore.sider.collapsedWidth : themeStore.sider.width;
    left = CHROME_INSET + siderW + CHROME_GAP;
  }

  // 顶栏浮岛占位：内容避开顶部浮岛高度
  if (themeStore.layout.mode === 'horizontal-card') {
    top = Math.max(top, CHROME_INSET + themeStore.header.height + CHROME_GAP);
  }

  // 底部无界吸附态：内容避开底栏浮岛
  if (themeStore.layout.mode === 'bottom-card' && themeStore.layout.bottomDock?.snapped) {
    bottom = Math.max(bottom, CHROME_INSET + themeStore.header.height + CHROME_GAP);
  }

  if (themeStore.tab.visible) {
    const anchor = themeStore.tab.cardAnchor;
    const tabH = themeStore.tab.height;
    if (anchor === 'top') {
      top = CHROME_INSET + tabH + CHROME_GAP;
      if (themeStore.layout.mode === 'horizontal-card') {
        top = CHROME_INSET + themeStore.header.height + CHROME_GAP + tabH + CHROME_GAP;
      }
    } else if (anchor === 'bottom') {
      bottom = CHROME_INSET + tabH + CHROME_GAP;
      if (themeStore.layout.mode === 'bottom-card' && themeStore.layout.bottomDock?.snapped) {
        bottom = CHROME_INSET + themeStore.header.height + CHROME_GAP + tabH + CHROME_GAP;
      }
    } else if (anchor === 'right') {
      right = CHROME_INSET + RIGHT_TAB_WIDTH + CHROME_GAP;
    }
  }

  return { top, right, bottom, left };
});

const resolved = computed(() => {
  const a = autoEdges.value;
  const useAuto = props.auto;
  return {
    top: toCss(props.marginTop, useAuto ? a.top : 0),
    right: toCss(props.marginRight, useAuto ? a.right : 0),
    bottom: toCss(props.marginBottom, useAuto ? a.bottom : 0),
    left: toCss(props.marginLeft, useAuto ? a.left : 0)
  };
});

const rootStyle = computed(() => ({
  paddingTop: resolved.value.top,
  paddingRight: resolved.value.right,
  paddingBottom: resolved.value.bottom,
  paddingLeft: resolved.value.left
}));
</script>

<template>
  <div class="borderless-div" :style="rootStyle">
    <div class="borderless-div__body" :class="contentClass">
      <slot
        :margin-top="resolved.top"
        :margin-right="resolved.right"
        :margin-bottom="resolved.bottom"
        :margin-left="resolved.left"
      />
    </div>
  </div>
</template>

<style scoped>
.borderless-div {
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  width: 100%;
  height: 100%;
  min-height: 0;
  overflow: hidden;
}

.borderless-div__body {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  overflow: auto;
}
</style>
