<script setup lang="ts">
import { computed } from 'vue';
import { LAYOUT_SCROLL_EL_ID } from '@sa/materials';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { useRouteStore } from '@/store/modules/route';
import { useTabStore } from '@/store/modules/tab';

defineOptions({
  name: 'GlobalContent'
});

interface Props {
  /** Show padding for content */
  showPadding?: boolean;
  /** 无界页：去掉实色底，透出氛围渐变 */
  borderless?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  showPadding: true,
  borderless: false
});

const appStore = useAppStore();
const themeStore = useThemeStore();
const routeStore = useRouteStore();
const tabStore = useTabStore();

const transitionName = computed(() => (themeStore.page.animate ? themeStore.page.animateMode : ''));

const pageClass = computed(() => ({
  'p-6px': props.showPadding,
  'flex-grow': true,
  'transition-300': true,
  'bg-layout': !props.borderless,
  'bg-transparent': props.borderless
}));

function resetScroll() {
  const el = document.querySelector(`#${LAYOUT_SCROLL_EL_ID}`);

  el?.scrollTo({ left: 0, top: 0 });
}

function geneBodd(p) {
  console.log(p);
  return '1';
}

function menuParaProps(p: any) {
  const meta = p?.route?.meta || {};
  const t: Record<string, unknown> = { role: meta.role };
  if (meta.role == '2') {
    t.disabled = true;
  }
  return t;
}
</script>

<template>
  <RouterView #default="p">
    <Transition
      :name="transitionName"
      mode="out-in"
      @before-leave="appStore.setContentXScrollable(true)"
      @after-leave="resetScroll"
      @after-enter="appStore.setContentXScrollable(false)"
    >
      <div class="mainpan-lv2" :class="{ 'mainpan-lv2--borderless': borderless }">
        <KeepAlive :include="routeStore.cacheRoutes" :exclude="routeStore.excludeCacheRoutes">
          <component
            :is="p.Component"
            v-if="appStore.reloadFlag"
            :key="p.route.fullPath"
            :class="pageClass"
            v-bind="menuParaProps(p)"
          />
        </KeepAlive>
      </div>
    </Transition>
  </RouterView>
</template>

<style scoped>
.mainpan-lv2--borderless :deep(> div) {
  background-color: transparent !important;
}
</style>
