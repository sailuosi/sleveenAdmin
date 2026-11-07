<script setup lang="ts">
import { computed } from 'vue';
import { themePageAnimationModeOptions, themeScrollModeOptions } from '@/constants/app';
import { useThemeStore } from '@/store/modules/theme';
import { translateOptions } from '@/utils/common';
import { $t } from '@/locales';
import SettingItem from '../../../components/setting-item.vue';

defineOptions({
  name: 'ContentSettings'
});

const themeStore = useThemeStore();

const isWrapperScrollMode = computed(() => themeStore.layout.scrollMode === 'wrapper');
</script>

<template>
  <ElDivider>{{ $t('theme.layout.content.title') }}</ElDivider>
  <TransitionGroup tag="div" name="setting-list" class="flex-col-stretch gap-12px">
    <SettingItem key="1" :label="$t('theme.layout.content.scrollMode.title')">
      <template #suffix>
        <IconTooltip :desc="$t('theme.layout.content.scrollMode.tip')" />
      </template>
      <ElSelect
        v-model="themeStore.layout.scrollMode"
        
        size="small"
        class="w-120px"
      >
        <ElOption v-for="opt in translateOptions(themeScrollModeOptions)" :key="opt.value" :label="opt.label" :value="opt.value" />
      </ElSelect>
    </SettingItem>
    <SettingItem key="2" :label="$t('theme.layout.content.page.animate')">
      <ElSwitch v-model="themeStore.page.animate" />
    </SettingItem>
    <SettingItem v-if="themeStore.page.animate" key="3" :label="$t('theme.layout.content.page.mode.title')">
      <ElSelect
        v-model="themeStore.page.animateMode"
        
        size="small"
        class="w-120px"
      >
        <ElOption v-for="opt in translateOptions(themePageAnimationModeOptions)" :key="opt.value" :label="opt.label" :value="opt.value" />
      </ElSelect>
    </SettingItem>
    <SettingItem v-if="isWrapperScrollMode" key="4" :label="$t('theme.layout.content.fixedHeaderAndTab')">
      <ElSwitch v-model="themeStore.fixedHeaderAndTab" />
    </SettingItem>
  </TransitionGroup>
</template>

<style scoped>
.setting-list-move,
.setting-list-enter-active,
.setting-list-leave-active {
  --uno: transition-all-300;
}

.setting-list-enter-from,
.setting-list-leave-to {
  --uno: opacity-0 -translate-x-30px;
}

.setting-list-leave-active {
  --uno: absolute;
}
</style>
