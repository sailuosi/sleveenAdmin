<script setup lang="ts">
import type { RouteKey } from '@elegant-router/types';
import { useRouterPush } from '@/hooks/common/router';
import { useMenuLabel } from '@/layouts/modules/global-menu/components/usemenulabel';
import EpMenuItems from '@/layouts/modules/global-menu/components/EpMenuItems.vue';

defineOptions({
  name: 'EpMenuItems'
});

defineProps<{
  menus: App.Global.Menu[];
}>();

const { routerPushByKeyWithMetaQuery } = useRouterPush();
const { menulabel: MenuLabel } = useMenuLabel();

function onParentNavigate(menu: App.Global.Menu) {
  routerPushByKeyWithMetaQuery(menu.key as RouteKey);
}
</script>

<template>
  <template v-for="menu in menus" :key="menu.key">
    <ElSubMenu v-if="menu.children?.length" :index="String(menu.key)">
      <template #title>
        <span class="inline-flex items-center gap-8px">
          <component :is="menu.icon" v-if="menu.icon" class="text-icon" />
          <MenuLabel
            :label="String(menu.label ?? '')"
            clickable
            :component-path="menu.componentPath"
            :menu-address="menu.menuAddress"
            @navigate="onParentNavigate(menu)"
          />
        </span>
      </template>
      <EpMenuItems :menus="menu.children" />
    </ElSubMenu>
    <ElMenuItem v-else :index="String(menu.key)">
      <span class="inline-flex items-center gap-8px">
        <component :is="menu.icon" v-if="menu.icon" class="text-icon" />
        <MenuLabel
          :label="String(menu.label ?? '')"
          :component-path="menu.componentPath"
          :menu-address="menu.menuAddress"
        />
      </span>
    </ElMenuItem>
  </template>
</template>
