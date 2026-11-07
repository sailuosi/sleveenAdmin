<script setup lang="ts">
import type { Placement } from 'element-plus';
import { twMerge } from 'tailwind-merge';

defineOptions({
  name: 'ButtonIcon',
  inheritAttrs: false
});

interface Props {
  /** Button class */
  class?: string;
  /** Iconify icon name */
  icon?: string;
  /** Tooltip content */
  tooltipContent?: string;
  /** Tooltip placement */
  tooltipPlacement?: Placement;
  zIndex?: number;
}

const props = withDefaults(defineProps<Props>(), {
  class: '',
  icon: '',
  tooltipContent: '',
  tooltipPlacement: 'bottom',
  zIndex: 98
});

const DEFAULT_CLASS = 'h-[36px] text-icon';
</script>

<template>
  <ElTooltip :placement="tooltipPlacement" :z-index="zIndex" :disabled="!tooltipContent" :content="tooltipContent">
    <ElButton text :class="twMerge(DEFAULT_CLASS, props.class)" v-bind="$attrs">
      <div class="flex-center gap-8px">
        <slot>
          <SvgIcon :icon="icon" />
        </slot>
      </div>
    </ElButton>
  </ElTooltip>
</template>

<style scoped></style>
