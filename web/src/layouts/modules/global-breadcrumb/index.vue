<script setup lang="ts">
import { createReusableTemplate } from '@vueuse/core';
import type { RouteKey } from '@elegant-router/types';
import { useThemeStore } from '@/store/modules/theme';
import { useRouteStore } from '@/store/modules/route';
import { useRouterPush } from '@/hooks/common/router';

defineOptions({
  name: 'GlobalBreadcrumb'
});

const themeStore = useThemeStore();
const routeStore = useRouteStore();
const { routerPushByKey } = useRouterPush();

interface BreadcrumbContentProps {
  breadcrumb: App.Global.Menu;
}

const [DefineBreadcrumbContent, BreadcrumbContent] = createReusableTemplate<BreadcrumbContentProps>();

function handleClickMenu(key: RouteKey) {
  routerPushByKey(key);
}
</script>

<template>
  <ElBreadcrumb v-if="themeStore.header.breadcrumb.visible" separator="/">
    <DefineBreadcrumbContent v-slot="{ breadcrumb }">
      <div class="i-flex-y-center align-middle">
        <component :is="breadcrumb.icon" v-if="themeStore.header.breadcrumb.showIcon" class="mr-4px text-icon" />
        {{ breadcrumb.label }}
      </div>
    </DefineBreadcrumbContent>

    <ElBreadcrumbItem v-for="item in routeStore.breadcrumbs" :key="item.key">
      <ElDropdown v-if="item.options?.length" trigger="click" @command="handleClickMenu">
        <span class="cursor-pointer">
          <BreadcrumbContent :breadcrumb="item" />
        </span>
        <template #dropdown>
          <ElDropdownMenu>
            <ElDropdownItem v-for="opt in item.options" :key="String(opt.key)" :command="opt.key">
              {{ opt.label }}
            </ElDropdownItem>
          </ElDropdownMenu>
        </template>
      </ElDropdown>
      <BreadcrumbContent v-else :breadcrumb="item" />
    </ElBreadcrumbItem>
  </ElBreadcrumb>
</template>

<style scoped></style>
