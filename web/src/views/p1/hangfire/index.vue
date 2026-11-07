<script setup lang="ts">
import { computed } from 'vue';

const hangfireUrl = computed(() => {
  const base = import.meta.env.VITE_SERVICE_BASE_URL || '';
  if (base.startsWith('http')) return `${base.replace(/\/$/, '')}/hangfire`;
  return '/hangfire';
});
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden">
    <ElCard title="定时任务 (Hangfire)" shadow="never" size="small" class="card-wrapper flex-1-hidden">
      <iframe :src="hangfireUrl" class="h-[calc(100vh-220px)] w-full border-0" title="Hangfire Dashboard" />
    </ElCard>
  </div>
</template>
