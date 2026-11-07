<script setup lang="ts">
import { computed, useAttrs } from 'vue';
import { Icon } from '@iconify/vue';

defineOptions({ name: 'SvgIcon', inheritAttrs: false });

/**
 * Props
 *
 * - Support iconify and local svg icon
 * - If icon and localIcon are passed at the same time, localIcon will be rendered first
 */
interface Props {
  /** Iconify icon name */
  icon?: string;
  /** Legacy icon name, supports iconify name or icon font classes */
  name?: string;
  /** Local svg icon name */
  localIcon?: string;
  /** Legacy size prop */
  size?: string | number;
}

const props = defineProps<Props>();

const attrs = useAttrs();

const bindAttrs = computed(() => ({
  ...attrs,
  class: ['svg-icon', (attrs.class as string) || ''].filter(Boolean).join(' ')
}));

const mergedStyle = computed(() => {
  const style = attrs.style ? String(attrs.style) : '';

  if (props.size === undefined || props.size === null || props.size === '') {
    return style;
  }

  const size = typeof props.size === 'number' ? `${props.size}px` : String(props.size);

  return style ? `font-size: ${size}; ${style}` : `font-size: ${size};`;
});

function normalizeIconName(rawIcon: string) {
  const icon = rawIcon.trim();

  if (!icon) {
    return '';
  }

  if (icon.includes(':')) {
    return icon;
  }

  const legacyPrefixes = new Set(['fa', 'fas', 'far', 'fab', 'fal', 'fad']);
  const classNames = icon.split(/\s+/).filter(Boolean);
  const candidate = classNames.find(item => !legacyPrefixes.has(item) && !item.startsWith('iconColor')) || '';

  if (!candidate) {
    return '';
  }

  if (/^fa[srldb]?-/i.test(candidate) || /^fa-/i.test(candidate)) {
    return `fa:${candidate.replace(/^fa[srldb]?-/i, '').replace(/^fa-/i, '')}`;
  }

  return candidate;
}

const resolvedIcon = computed(() => normalizeIconName(props.icon || props.name || ''));

const symbolId = computed(() => {
  const { VITE_ICON_LOCAL_PREFIX: prefix } = import.meta.env;

  const defaultLocalIcon = 'no-icon';

  const icon = props.localIcon || defaultLocalIcon;

  return `#${prefix}-${icon}`;
});

/** If localIcon is passed, render localIcon first */
const renderLocalIcon = computed(() => props.localIcon || !resolvedIcon.value);
</script>

<template>
  <template v-if="renderLocalIcon">
    <svg aria-hidden="true" width="1em" height="1em" v-bind="bindAttrs" :style="mergedStyle">
      <use :xlink:href="symbolId" fill="currentColor" />
    </svg>
  </template>
  <template v-else>
    <Icon v-if="resolvedIcon" :icon="resolvedIcon" v-bind="bindAttrs" :style="mergedStyle" />
  </template>
</template>

<style scoped></style>
