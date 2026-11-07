<script setup lang="ts">
import { ElConfigProvider } from 'element-plus';
import zhCn from 'element-plus/es/locale/lang/zh-cn';
import PtAppletPage from '@/widgets/sysext/views/paotalconfig/ptappletPage.vue';
</script>

<template>
  <div class="portal-view-root h-full">
    <ElConfigProvider :locale="zhCn">
      <div class="portal-page h-full">
        <PtAppletPage />
      </div>
    </ElConfigProvider>
  </div>
</template>

<style scoped>
.portal-view-root {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  height: 100%;
}

.portal-page {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  padding: 12px;
}

.portal-page :deep(.flexpan.flexed) {
  flex: 1;
  min-height: 0;
}
</style>
