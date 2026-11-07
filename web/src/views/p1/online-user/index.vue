<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { fetchGetOnlineUserList } from '@/service/api';

const loading = ref(false);
const list = ref<Api.SystemP1.OnlineUser[]>([]);
let timer: ReturnType<typeof setInterval> | null = null;

async function loadData() {
  loading.value = true;
  try {
    const { data } = await fetchGetOnlineUserList();
    if (data) list.value = data;
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  loadData();
  timer = setInterval(loadData, 5000);
});

onUnmounted(() => {
  if (timer) clearInterval(timer);
});

const columns = [
  { title: '账号', key: 'account' },
  { title: '姓名', key: 'realName' },
  { title: 'IP', key: 'ip' },
  { title: '连接时间', key: 'connectedAt' }
];
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard shadow="never" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header>
        <div class="flex w-full items-center justify-between gap-12px">
          <span class="font-medium">在线用户</span>
          <ElButton size="small" :loading="loading" @click="loadData">刷新</ElButton>
        </div>
      </template>
      <ElAlert type="info" class="mb-12px">登录用户会自动上报在线状态；本页每 5 秒自动刷新。</ElAlert>
      <TableData :columns="columns" :data="list" :loading="loading" size="small" />
    </ElCard>
  </div>
</template>
