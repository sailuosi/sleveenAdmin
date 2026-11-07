<script setup lang="ts">
import { ElConfigProvider } from 'element-plus';
import zhCn from 'element-plus/es/locale/lang/zh-cn';
import DesignPage from '@/widgets/portaldesign/views/designPage.vue';
</script>

<template>
  <!-- 对齐 minehub：Teleport + design.vue embedded(is-viewport) 自管视口高度 -->
  <Teleport to="body">
    <ElConfigProvider :locale="zhCn">
      <DesignPage />
    </ElConfigProvider>
  </Teleport>
</template>
