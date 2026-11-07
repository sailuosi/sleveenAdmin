<script setup lang="ts">
import { computed } from 'vue';
import { $t } from '@/locales';

defineOptions({
  name: 'ProjectNews'
});

interface NewsItem {
  id: number;
  content: string;
  time: string;
}

const newses = computed<NewsItem[]>(() => [
  { id: 1, content: $t('page.home.projectNews.desc1'), time: '2026-06-07 09:00:00' },
  { id: 2, content: $t('page.home.projectNews.desc2'), time: '2026-06-07 11:30:00' },
  { id: 3, content: $t('page.home.projectNews.desc3'), time: '2026-06-07 13:45:00' },
  { id: 4, content: $t('page.home.projectNews.desc4'), time: '2026-06-07 16:00:00' },
  { id: 5, content: $t('page.home.projectNews.desc5'), time: '2026-06-07 18:20:00' }
]);
</script>

<template>
  <ElCard shadow="never" class="card-wrapper">
    <template #header>
      <div class="flex items-center justify-between">
        <span>{{ $t('page.home.projectNews.title') }}</span>
        <a class="text-primary" href="javascript:;">{{ $t('page.home.projectNews.moreNews') }}</a>
      </div>
    </template>
    <div class="flex flex-col gap-12px">
      <div v-for="item in newses" :key="item.id" class="flex-y-center gap-12px">
        <AppAvatar class="size-48px!" />
        <div class="min-w-0 flex-1">
          <div class="truncate">{{ item.content }}</div>
          <div class="text-12px text-#999">{{ item.time }}</div>
        </div>
      </div>
    </div>
  </ElCard>
</template>

<style scoped></style>
