<script setup lang="ts">
import SiderSettingsPopover from '../../global-sider/components/sider-settings-popover.vue';

defineOptions({
  name: 'BottomDockTools'
});
</script>

<template>
  <Teleport to="body">
    <div class="bottom-dock-tools">
      <div class="bottom-dock-tools-glass">
        <SiderSettingsPopover />
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.bottom-dock-tools {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 2000;
  pointer-events: none;
}

.bottom-dock-tools-glass {
  pointer-events: auto;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 6px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(20px) saturate(1.4);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.45);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.08);
}

html.dark .bottom-dock-tools-glass {
  background: rgba(30, 30, 40, 0.55);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
}
</style>
