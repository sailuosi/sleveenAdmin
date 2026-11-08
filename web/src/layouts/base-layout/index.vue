<script setup lang="ts">
import { computed, defineAsyncComponent, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { AdminLayout, LAYOUT_SCROLL_EL_ID } from '@sa/materials';
import type { LayoutMode } from '@sa/materials';
import { useAppStore } from '@/store/modules/app';
import { useAuthStore } from '@/store/modules/auth';
import { useThemeStore } from '@/store/modules/theme';
import { ensureOnlineUserHub } from '@/utils/signalR/onlineUser';
import GlobalHeader from '../modules/global-header/index.vue';
import GlobalSider from '../modules/global-sider/index.vue';
import GlobalTab from '../modules/global-tab/index.vue';
import CardTabHost from '../modules/global-tab/card-tab-host.vue';
import GlobalContent from '../modules/global-content/index.vue';
import GlobalFooter from '../modules/global-footer/index.vue';
import ThemeDrawer from '../modules/theme-drawer/index.vue';
import { provideMixMenuContext } from '../modules/global-menu/context';

defineOptions({
  name: 'BaseLayout'
});

const route = useRoute();
const appStore = useAppStore();
const authStore = useAuthStore();
const themeStore = useThemeStore();
const { secondLevelMenus, childLevelMenus, isActiveFirstLevelMenuHasChildren } = provideMixMenuContext();

const GlobalMenu = defineAsyncComponent(() => import('../modules/global-menu/index.vue'));

onMounted(() => {
  if (authStore.isLogin) {
    void ensureOnlineUserHub();
  }
});

watch(
  () => authStore.isLogin,
  loggedIn => {
    if (loggedIn) {
      void ensureOnlineUserHub();
    }
  }
);

const isVerticalCard = computed(() => themeStore.layout.mode === 'vertical-card');
const isHorizontalCard = computed(() => themeStore.layout.mode === 'horizontal-card');
/** 左栏无界 / 顶栏无界：共享页签浮层与氛围底 */
const isCardLayout = computed(() => isVerticalCard.value || isHorizontalCard.value);

/** 无界 Vue 组件：menuType=4，内容全屏，侧栏/页签悬浮 */
const isBorderlessPage = computed(() => {
  const t = route.meta?.menuType as string | number | undefined;
  return t === '4' || t === 4;
});

/** 顶栏无界默认即沉浸无界；路由 menuType=4 仍可叠加 */
const isImmersiveContent = computed(() => isBorderlessPage.value || isHorizontalCard.value);

const layoutMode = computed(() => {
  const vertical: LayoutMode = 'vertical';
  const horizontal: LayoutMode = 'horizontal';
  return themeStore.layout.mode.includes(vertical) ? vertical : horizontal;
});

const headerProps = computed(() => {
  const { mode } = themeStore.layout;

  const headerPropsConfig: Record<UnionKey.ThemeLayoutMode, App.Global.HeaderProps> = {
    vertical: {
      showLogo: false,
      showMenu: false,
      showMenuToggler: true
    },
    'vertical-mix': {
      showLogo: false,
      showMenu: false,
      showMenuToggler: false
    },
    'vertical-hybrid-header-first': {
      showLogo: !isActiveFirstLevelMenuHasChildren.value,
      showMenu: true,
      showMenuToggler: false
    },
    'vertical-card': {
      showLogo: false,
      showMenu: false,
      showMenuToggler: false
    },
    horizontal: {
      showLogo: true,
      showMenu: true,
      showMenuToggler: false
    },
    'horizontal-card': {
      showLogo: true,
      showMenu: true,
      showMenuToggler: false
    },
    'top-hybrid-sidebar-first': {
      showLogo: true,
      showMenu: true,
      showMenuToggler: false
    },
    'top-hybrid-header-first': {
      showLogo: true,
      showMenu: true,
      showMenuToggler: isActiveFirstLevelMenuHasChildren.value
    }
  };

  return headerPropsConfig[mode];
});

const siderVisible = computed(
  () => themeStore.layout.mode !== 'horizontal' && themeStore.layout.mode !== 'horizontal-card'
);

const isVerticalMix = computed(() => themeStore.layout.mode === 'vertical-mix');

const isVerticalHybridHeaderFirst = computed(() => themeStore.layout.mode === 'vertical-hybrid-header-first');

const isTopHybridSidebarFirst = computed(() => themeStore.layout.mode === 'top-hybrid-sidebar-first');

const isTopHybridHeaderFirst = computed(() => themeStore.layout.mode === 'top-hybrid-header-first');

const siderWidth = computed(() => getSiderAndCollapsedWidth(false));

const siderCollapsedWidth = computed(() => getSiderAndCollapsedWidth(true));

const siderInset = computed(() => (isVerticalCard.value || isBorderlessPage.value ? 16 : 0));
const siderRadius = computed(() => (isVerticalCard.value || isBorderlessPage.value ? 16 : 0));

const headerInset = computed(() => (isHorizontalCard.value ? 16 : 0));
const headerRadius = computed(() => (isHorizontalCard.value ? 16 : 0));
const floatHeader = computed(() => isHorizontalCard.value);

/** 顶栏无界始终显示浮岛；左栏无界 / 无界页隐藏通栏顶栏 */
const headerVisible = computed(
  () => isHorizontalCard.value || (!isVerticalCard.value && !isBorderlessPage.value)
);

const cardAnchor = computed(() => themeStore.tab.cardAnchor);

/** borderless / 顶栏无界：never embed tabs in flow as forced; card layouts use anchors */
const showTopEmbedTab = computed(
  () =>
    !isImmersiveContent.value &&
    isCardLayout.value &&
    themeStore.tab.visible &&
    cardAnchor.value === 'top'
);

const showBottomEmbedTab = computed(
  () =>
    !isImmersiveContent.value &&
    isCardLayout.value &&
    themeStore.tab.visible &&
    cardAnchor.value === 'bottom'
);

/** float dock for right always, or any anchor when immersive */
const showFloatTabDock = computed(() => {
  if (!themeStore.tab.visible) return false;
  if (isImmersiveContent.value) return cardAnchor.value !== 'hidden';
  return isCardLayout.value && cardAnchor.value === 'right';
});

const tabVisible = computed(() => {
  if (isImmersiveContent.value) return false;
  if (isCardLayout.value) {
    return themeStore.tab.visible && cardAnchor.value === 'top';
  }
  return themeStore.tab.visible;
});

const fixedTop = computed(() =>
  isCardLayout.value || isBorderlessPage.value ? false : themeStore.fixedHeaderAndTab
);

const footerVisible = computed(
  () => !isImmersiveContent.value && (showBottomEmbedTab.value || themeStore.footer.visible)
);

const footerHeight = computed(() =>
  showBottomEmbedTab.value ? themeStore.tab.height : themeStore.footer.height
);

const floatSider = computed(() => isBorderlessPage.value && !isHorizontalCard.value);

const cardContentClass = computed(() => {
  const classes: string[] = [];
  if (appStore.contentXScrollable) classes.push('overflow-x-hidden');
  if (isImmersiveContent.value) {
    classes.push('borderless-content');
  } else if (isCardLayout.value && themeStore.tab.visible && cardAnchor.value === 'right') {
    classes.push('card-content-pad-right');
  }
  if (isCardLayout.value || isBorderlessPage.value) {
    classes.push('card-main-flow');
    classes.push('glass-atmosphere');
  }
  return classes.join(' ');
});

function getSiderAndCollapsedWidth(isCollapsed: boolean) {
  const {
    mixChildMenuWidth,
    collapsedWidth,
    width: themeWidth,
    mixCollapsedWidth,
    mixWidth: themeMixWidth
  } = themeStore.sider;

  const width = isCollapsed ? collapsedWidth : themeWidth;
  const mixWidth = isCollapsed ? mixCollapsedWidth : themeMixWidth;

  if (isTopHybridHeaderFirst.value) {
    return isActiveFirstLevelMenuHasChildren.value ? width : 0;
  }

  if (isVerticalHybridHeaderFirst.value && !isActiveFirstLevelMenuHasChildren.value) {
    return 0;
  }

  const isMixMode = isVerticalMix.value || isTopHybridSidebarFirst.value || isVerticalHybridHeaderFirst.value;
  let finalWidth = isMixMode ? mixWidth : width;

  if (isVerticalMix.value && appStore.mixSiderFixed && secondLevelMenus.value.length) {
    finalWidth += mixChildMenuWidth;
  }

  if (isVerticalHybridHeaderFirst.value && appStore.mixSiderFixed && childLevelMenus.value.length) {
    finalWidth += mixChildMenuWidth;
  }

  return finalWidth;
}
</script>

<template>
  <AdminLayout
    v-model:sider-collapse="appStore.siderCollapse"
    :mode="layoutMode"
    :scroll-el-id="LAYOUT_SCROLL_EL_ID"
    :scroll-mode="themeStore.layout.scrollMode"
    :is-mobile="appStore.isMobile"
    :full-content="appStore.fullContent"
    :fixed-top="fixedTop"
    :header-visible="headerVisible"
    :header-height="themeStore.header.height"
    :header-inset="headerInset"
    :header-radius="headerRadius"
    :float-header="floatHeader"
    :tab-visible="tabVisible"
    :tab-height="themeStore.tab.height"
    :content-class="cardContentClass"
    :sider-visible="siderVisible"
    :sider-width="siderWidth"
    :sider-collapsed-width="siderCollapsedWidth"
    :sider-inset="siderInset"
    :sider-radius="siderRadius"
    :float-sider="floatSider"
    :footer-visible="footerVisible"
    :footer-height="footerHeight"
    :fixed-footer="showBottomEmbedTab ? false : themeStore.footer.fixed"
    :right-footer="themeStore.footer.right"
  >
    <template v-if="headerVisible" #header>
      <GlobalHeader v-bind="headerProps" />
    </template>
    <template #tab>
      <CardTabHost v-if="showTopEmbedTab" variant="dock" />
      <GlobalTab v-else-if="!isCardLayout && !isBorderlessPage" />
    </template>
    <template #sider>
      <GlobalSider />
    </template>
    <GlobalMenu />
    <GlobalContent :show-padding="!isImmersiveContent" :borderless="isImmersiveContent" />
    <CardTabHost v-if="isCardLayout || isBorderlessPage" variant="overlay" />
    <CardTabHost v-if="showFloatTabDock" variant="dock" force-float />
    <ThemeDrawer />
    <template #footer>
      <CardTabHost v-if="showBottomEmbedTab" variant="dock" />
      <GlobalFooter v-else />
    </template>
  </AdminLayout>
</template>

<style lang="scss">
#__SCROLL_EL_ID__ {
  @include scrollbar();
}

.card-main-flow {
  min-height: 0;
}

.card-content-pad-right {
  padding-right: 236px;
}

/* vertical-card / horizontal-card / 无界：纯 CSS 氛围底（无背景图） */
.glass-atmosphere {
  background:
    radial-gradient(ellipse 80% 60% at 15% 20%, rgba(200, 210, 255, 0.55), transparent 55%),
    radial-gradient(ellipse 70% 50% at 85% 30%, rgba(180, 220, 255, 0.5), transparent 50%),
    radial-gradient(ellipse 60% 40% at 70% 85%, rgba(230, 210, 255, 0.35), transparent 45%),
    linear-gradient(135deg, #e8eefc 0%, #dceaf8 45%, #e4f0fb 100%);
}

html.dark .glass-atmosphere {
  background:
    radial-gradient(ellipse 80% 60% at 15% 20%, rgba(60, 80, 140, 0.45), transparent 55%),
    radial-gradient(ellipse 70% 50% at 85% 30%, rgba(40, 90, 130, 0.4), transparent 50%),
    radial-gradient(ellipse 60% 40% at 70% 85%, rgba(80, 60, 120, 0.3), transparent 45%),
    linear-gradient(135deg, #1a1f2e 0%, #1c2433 45%, #1a2230 100%);
}

/* 无界页：内容铺满，去掉 mainpan 外边距与固定一屏外溢 */
.borderless-content {
  min-height: 0;
  height: 100%;
}

.borderless-content div.mainpan-lv2 {
  width: 100% !important;
  height: 100% !important;
  margin: 0 !important;
  border-radius: 0 !important;
  background-color: transparent !important;
}

.borderless-content .mainpan-lv2 > div {
  border-radius: 0 !important;
  height: 100%;
  min-height: 0;
  background-color: transparent !important;
}
</style>
