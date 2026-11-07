<script lang="ts" setup>
import { computed } from 'vue';

/**
 * 区块小标题：左侧竖条 + 标题，右侧可选插槽（与原 SubTitle 的 tab 插槽一致）。
 */
defineOptions({ name: 'SubTitle' });

const props = withDefaults(
  defineProps<{
    /** 标题文案 */
    title: string;
    /** 左侧竖条颜色 */
    accentColor?: string;
    /** 标题字号档位 */
    size?: 'sm' | 'md' | 'lg';
  }>(),
  {
    accentColor: 'rgb(59, 130, 246)',
    size: 'md',
  },
);

const sizeClass = computed(() => {
  const map = { sm: 'sub-title--sm', md: 'sub-title--md', lg: 'sub-title--lg' } as const;
  return map[props.size];
});
</script>

<template>
  <div class="sub-title" :class="sizeClass">
    <div class="sub-title__text" :style="{ borderLeftColor: accentColor }">
      {{ title }}
    </div>
    <div v-if="$slots.tab" class="sub-title__extra">
      <slot name="tab"></slot>
    </div>
  </div>
</template>

<style scoped>
.sub-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px 12px;
}

.sub-title__text {
  border-left: 8px solid;
  font-weight: 600;
  padding-left: 8px;
  line-height: 1.4;
  min-width: 0;
}

.sub-title--sm .sub-title__text {
  font-size: 14px;
}

.sub-title--md .sub-title__text {
  font-size: 18px;
}

.sub-title--lg .sub-title__text {
  font-size: 20px;
}

.sub-title__extra {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
