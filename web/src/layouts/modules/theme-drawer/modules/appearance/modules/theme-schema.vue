<script setup lang="ts">
import { computed } from 'vue';
import { themeSchemaRecord } from '@/constants/app';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';
import SettingItem from '../../../components/setting-item.vue';

defineOptions({
  name: 'ThemeSchema'
});

const themeStore = useThemeStore();

const icons: Record<UnionKey.ThemeScheme, string> = {
  light: 'material-symbols:sunny',
  dark: 'material-symbols:nightlight-rounded',
  auto: 'material-symbols:hdr-auto'
};

function handleSegmentChange(value: string | number) {
  themeStore.setThemeScheme(value as UnionKey.ThemeScheme);
}

const showSiderInverted = computed(
  () =>
    !themeStore.darkMode &&
    themeStore.layout.mode.includes('vertical') &&
    themeStore.layout.mode !== 'vertical-card'
);
</script>

<template>
  <ElDivider>{{ $t('theme.appearance.themeSchema.title') }}</ElDivider>
  <div class="flex-col-stretch gap-16px">
    <div class="i-flex-center gap-8px">
      <ElButton
        v-for="(_, key) in themeSchemaRecord"
        :key="key"
        :type="themeStore.themeScheme === key ? 'primary' : 'default'"
        size="small"
        @click="handleSegmentChange(key)"
      >
        <SvgIcon :icon="icons[key]" class="h-23px text-icon-small" />
      </ElButton>
    </div>
    <Transition name="sider-inverted">
      <SettingItem v-if="showSiderInverted" :label="$t('theme.layout.sider.inverted')">
        <ElSwitch v-model="themeStore.sider.inverted" />
      </SettingItem>
    </Transition>
    <SettingItem :label="$t('theme.appearance.grayscale')">
      <ElSwitch :model-value="themeStore.grayscale" @update:model-value="themeStore.setGrayscale" />
    </SettingItem>
    <SettingItem :label="$t('theme.appearance.colourWeakness')">
      <ElSwitch :model-value="themeStore.colourWeakness" @update:model-value="themeStore.setColourWeakness" />
    </SettingItem>
  </div>
</template>

<style scoped>
.sider-inverted-enter-active,
.sider-inverted-leave-active {
  --uno: h-22px transition-all-300;
}

.sider-inverted-enter-from,
.sider-inverted-leave-to {
  --uno: translate-x-20px opacity-0 h-0;
}
</style>
