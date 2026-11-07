<script setup lang="ts">
import { computed } from 'vue';
import { watermarkTimeFormatOptions } from '@/constants/app';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';
import SettingItem from '../../../components/setting-item.vue';

defineOptions({
  name: 'WatermarkSettings'
});

const themeStore = useThemeStore();

const isWatermarkTextVisible = computed(
  () => themeStore.watermark.visible && !themeStore.watermark.enableUserName && !themeStore.watermark.enableTime
);
</script>

<template>
  <ElDivider>{{ $t('theme.general.watermark.title') }}</ElDivider>
  <TransitionGroup tag="div" name="setting-list" class="flex-col-stretch gap-12px">
    <SettingItem key="1" :label="$t('theme.general.watermark.visible')">
      <ElSwitch v-model="themeStore.watermark.visible" />
    </SettingItem>
    <SettingItem v-if="themeStore.watermark.visible" key="2" :label="$t('theme.general.watermark.enableUserName')">
      <ElSwitch :model-value="themeStore.watermark.enableUserName" @update:model-value="themeStore.setWatermarkEnableUserName" />
    </SettingItem>
    <SettingItem v-if="themeStore.watermark.visible" key="3" :label="$t('theme.general.watermark.enableTime')">
      <ElSwitch :model-value="themeStore.watermark.enableTime" @update:model-value="themeStore.setWatermarkEnableTime" />
    </SettingItem>
    <SettingItem
      v-if="themeStore.watermark.visible && themeStore.watermark.enableTime"
      key="4"
      :label="$t('theme.general.watermark.timeFormat')"
    >
      <ElSelect
        v-model="themeStore.watermark.timeFormat"
        
        size="small"
        class="w-210px"
      >
        <ElOption v-for="opt in watermarkTimeFormatOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
      </ElSelect>
    </SettingItem>
    <SettingItem v-if="isWatermarkTextVisible" key="5" :label="$t('theme.general.watermark.text')">
      <ElInput
        v-model="themeStore.watermark.text"
        autosize
        type="text"
        size="small"
        class="w-120px"
        placeholder="MineOne"
      />
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
