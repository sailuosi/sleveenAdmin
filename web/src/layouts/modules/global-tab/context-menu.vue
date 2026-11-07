<script setup lang="ts">
import { computed } from 'vue';
import { useTabStore } from '@/store/modules/tab';
import { $t } from '@/locales';

defineOptions({
  name: 'ContextMenu'
});

interface Props {
  /** ClientX */
  x: number;
  /** ClientY */
  y: number;
  tabId: string;
  excludeKeys?: App.Global.DropdownKey[];
  disabledKeys?: App.Global.DropdownKey[];
}

const props = withDefaults(defineProps<Props>(), {
  excludeKeys: () => [],
  disabledKeys: () => []
});

const visible = defineModel<boolean>('visible');

const { removeTab, clearTabs, clearLeftTabs, clearRightTabs, fixTab, unfixTab, isTabRetain, homeTab } = useTabStore();

type MenuOption = {
  key: App.Global.DropdownKey;
  label: string;
  icon: string;
  disabled?: boolean;
};

const options = computed(() => {
  const opts: MenuOption[] = [
    { key: 'closeCurrent', label: $t('dropdown.closeCurrent'), icon: 'ant-design:close-outlined' },
    { key: 'closeOther', label: $t('dropdown.closeOther'), icon: 'ant-design:column-width-outlined' },
    { key: 'closeLeft', label: $t('dropdown.closeLeft'), icon: 'mdi:format-horizontal-align-left' },
    { key: 'closeRight', label: $t('dropdown.closeRight'), icon: 'mdi:format-horizontal-align-right' },
    { key: 'closeAll', label: $t('dropdown.closeAll'), icon: 'ant-design:line-outlined' }
  ];

  if (props.tabId !== homeTab?.id) {
    if (isTabRetain(props.tabId)) {
      opts.push({ key: 'unpin', label: $t('dropdown.unpin'), icon: 'mdi:pin-off-outline' });
    } else {
      opts.push({ key: 'pin', label: $t('dropdown.pin'), icon: 'mdi:pin-outline' });
    }
  }

  const { excludeKeys, disabledKeys } = props;
  const result = opts.filter(opt => !excludeKeys.includes(opt.key));

  disabledKeys.forEach(key => {
    const opt = result.find(item => item.key === key);
    if (opt) opt.disabled = true;
  });

  return result;
});

function hideDropdown() {
  visible.value = false;
}

const dropdownAction: Record<App.Global.DropdownKey, () => void> = {
  closeCurrent() {
    removeTab(props.tabId);
  },
  closeOther() {
    clearTabs([props.tabId]);
  },
  closeLeft() {
    clearLeftTabs(props.tabId);
  },
  closeRight() {
    clearRightTabs(props.tabId);
  },
  closeAll() {
    clearTabs();
  },
  pin() {
    fixTab(props.tabId);
  },
  unpin() {
    unfixTab(props.tabId);
  }
};

function handleDropdown(optionKey: App.Global.DropdownKey) {
  dropdownAction[optionKey]?.();
  hideDropdown();
}
</script>

<template>
  <Teleport to="body">
    <div v-if="visible" class="fixed inset-0 z-9999" @click="hideDropdown" @contextmenu.prevent="hideDropdown">
      <div
        class="context-menu absolute min-w-140px overflow-hidden rounded-6px bg-container py-4px shadow-md"
        :style="{ left: `${x}px`, top: `${y}px` }"
        @click.stop
      >
        <button
          v-for="opt in options"
          :key="opt.key"
          type="button"
          class="flex w-full items-center gap-8px px-12px py-8px text-left text-14px hover:bg-primary/8 disabled:cursor-not-allowed disabled:opacity-40"
          :disabled="opt.disabled"
          @click="handleDropdown(opt.key)"
        >
          <SvgIcon :icon="opt.icon" class="text-16px" />
          <span>{{ opt.label }}</span>
        </button>
      </div>
    </div>
  </Teleport>
</template>

<style scoped></style>
