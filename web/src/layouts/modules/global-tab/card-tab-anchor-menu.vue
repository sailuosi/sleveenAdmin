<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useThemeStore } from '@/store/modules/theme';
import { $t } from '@/locales';

defineOptions({
  name: 'CardTabAnchorMenu'
});

interface Props {
  /** compact icon trigger vs text row */
  compact?: boolean;
}

withDefaults(defineProps<Props>(), {
  compact: true
});

const appStore = useAppStore();
const themeStore = useThemeStore();

const anchor = computed(() => themeStore.tab.cardAnchor);

const options: { value: UnionKey.ThemeTabCardAnchor; icon: string; labelKey: App.I18n.I18nKey }[] = [
  { value: 'hidden', icon: 'mdi:eye-off-outline', labelKey: 'theme.layout.tab.cardAnchorHidden' },
  { value: 'top', icon: 'mdi:arrow-collapse-up', labelKey: 'theme.layout.tab.cardAnchorTop' },
  { value: 'bottom', icon: 'mdi:arrow-collapse-down', labelKey: 'theme.layout.tab.cardAnchorBottom' },
  { value: 'right', icon: 'mdi:arrow-collapse-right', labelKey: 'theme.layout.tab.cardAnchorRight' }
];

const current = computed(() => options.find(item => item.value === anchor.value) || options[0]);

function setAnchor(value: UnionKey.ThemeTabCardAnchor) {
  themeStore.tab.cardAnchor = value;
  if (value !== 'hidden') {
    appStore.closeCardTabOverlay();
  }
}
</script>

<template>
  <ElDropdown trigger="click" @command="setAnchor">
    <button
      type="button"
      class="anchor-trigger"
      :class="{ 'is-compact': compact, 'is-active': anchor !== 'hidden' }"
      :title="$t('theme.layout.tab.cardAnchor')"
    >
      <SvgIcon :icon="current.icon" class="text-16px" />
      <span v-if="!compact" class="text-13px">{{ $t(current.labelKey) }}</span>
      <SvgIcon v-if="!compact" icon="mdi:chevron-down" class="text-14px opacity-60" />
    </button>
    <template #dropdown>
      <ElDropdownMenu>
        <ElDropdownItem
          v-for="item in options"
          :key="item.value"
          :command="item.value"
          :class="{ 'is-selected': item.value === anchor }"
        >
          <div class="flex-y-center gap-8px">
            <SvgIcon :icon="item.icon" class="text-16px" />
            <span>{{ $t(item.labelKey) }}</span>
          </div>
        </ElDropdownItem>
      </ElDropdownMenu>
    </template>
  </ElDropdown>
</template>

<style scoped>
.anchor-trigger {
  --uno: flex-y-center gap-6px rd-8px border-none bg-transparent text-base-text cursor-pointer hover:bg-primary/10 transition-colors;
}

.anchor-trigger.is-compact {
  --uno: size-36px flex-center rd-10px;
}

.anchor-trigger.is-active {
  --uno: text-primary bg-primary/12;
}

.anchor-trigger:not(.is-compact) {
  --uno: px-8px py-8px;
}
</style>
