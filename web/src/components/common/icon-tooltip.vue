<script lang="ts" setup>
import { computed, useSlots } from 'vue';
import type { Placement } from 'element-plus';

defineOptions({ name: 'IconTooltip' });

interface Props {
  icon?: string;
  localIcon?: string;
  desc?: string;
  placement?: Placement;
}

const props = withDefaults(defineProps<Props>(), {
  icon: 'mdi-help-circle',
  localIcon: '',
  desc: '',
  placement: 'top'
});

const slots = useSlots();
const hasCustomTrigger = computed(() => Boolean(slots.trigger));

if (!hasCustomTrigger.value && !props.icon && !props.localIcon) {
  throw new Error('icon or localIcon is required when no custom trigger slot is provided');
}
</script>

<template>
  <ElTooltip :placement="placement">
    <span class="inline-flex cursor-pointer">
      <slot name="trigger">
        <SvgIcon :icon="icon" :local-icon="localIcon" />
      </slot>
    </span>
    <template #content>
      <slot>
        <span>{{ desc }}</span>
      </slot>
    </template>
  </ElTooltip>
</template>
