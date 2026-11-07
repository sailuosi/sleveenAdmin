<script setup lang="ts">

defineOptions({
  name: 'GlobalMenuLabel'
});

interface Props {
  label?: string;
  disabled?: boolean;
  clickable?: boolean;
  componentPath?: string;
  menuAddress?: string;
  onNavigate?: (() => void) | undefined;
}

const props = defineProps<Props>();

function handleClick() {
  if (props.disabled || !props.clickable) {
    return;
  }

  const componentPath = props.componentPath?.trim();
  const menuAddress = props.menuAddress?.trim();

  if (!componentPath && !menuAddress) return;

  props.onNavigate?.();
}
</script>

<template>
  <div
    class="global-menu-label"
    :class="{ 'global-menu-label--clickable': clickable, 'global-menu-label--disabled': disabled }"
    @click="handleClick"
  >
    {{ label }}
  </div>
</template>

<style lang="css" scoped>
.global-menu-label {
  width: 100%;
}

.global-menu-label--clickable {
  cursor: pointer;
}

.global-menu-label--disabled {
  cursor: not-allowed;
}
</style>
