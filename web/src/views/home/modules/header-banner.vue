<script setup lang="ts">
import { computed } from 'vue';
import { useAppStore } from '@/store/modules/app';
import { useAuthStore } from '@/store/modules/auth';
import { $t } from '@/locales';

defineOptions({
  name: 'HeaderBanner'
});

const appStore = useAppStore();
const authStore = useAuthStore();

const isMobile = computed(() => appStore.isMobile);

interface StatisticData {
  id: number;
  label: string;
  value: string;
}

const statisticData = computed<StatisticData[]>(() => [
  {
    id: 0,
    label: $t('page.home.projectCount'),
    value: '25'
  },
  {
    id: 1,
    label: $t('page.home.todo'),
    value: '4/16'
  },
  {
    id: 2,
    label: $t('page.home.message'),
    value: '12'
  }
]);
</script>

<template>
  <ElCard shadow="never" class="card-wrapper">
    <div class="flex flex-wrap items-center justify-between gap-16px" :class="{ 'flex-col items-stretch': isMobile }">
      <div class="flex-y-center">
        <div class="size-72px shrink-0 overflow-hidden rd-1/2">
          <img src="@/assets/imgs/logo.png" class="size-full" alt="sleveen-admin" />
        </div>
        <div class="pl-12px">
          <h3 class="text-18px font-semibold">
            {{ $t('page.home.greeting', { userName: authStore.userInfo.userName }) }}
          </h3>
          <p class="text-#999 leading-30px">{{ $t('page.home.weatherDesc') }}</p>
        </div>
      </div>
      <div class="flex flex-wrap gap-24px" :class="isMobile ? 'justify-start' : 'justify-end'">
        <div v-for="item in statisticData" :key="item.id" class="whitespace-nowrap text-center">
          <div class="text-20px font-semibold">{{ item.value }}</div>
          <div class="text-12px text-#999">{{ item.label }}</div>
        </div>
      </div>
    </div>
  </ElCard>
</template>

<style scoped></style>
