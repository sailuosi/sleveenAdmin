<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { fetchGetRedisInfo, fetchGetServerInfo } from '@/service/api';

const loading = ref(false);
const serverInfo = ref<Api.SysInfra.ServerInfo | null>(null);
const redisInfo = ref<Api.SysInfra.RedisInfo | null>(null);

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`;
}

function formatUptime(uptime: string) {
  const match = uptime.match(/([\d.]+):([\d]{2}):([\d]{2})/);
  if (!match) return uptime;
  const hours = Number(match[1]);
  const days = Math.floor(hours / 24);
  const remainHours = hours % 24;
  return days > 0 ? `${days}天 ${remainHours}小时` : `${hours}小时`;
}

async function loadData() {
  loading.value = true;
  try {
    const [{ data: server }, { data: redis }] = await Promise.all([
      fetchGetServerInfo(),
      fetchGetRedisInfo()
    ]);
    serverInfo.value = server ?? null;
    redisInfo.value = redis ?? null;
  } finally {
    loading.value = false;
  }
}

onMounted(loadData);
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <div v-loading="loading">
      <div class="grid grid-cols-1 gap-16px xl:grid-cols-2">
        <ElCard shadow="never" size="small">
          <template #header>
            <div class="flex w-full items-center justify-between gap-12px">
              <span class="font-medium">服务器信息</span>
              <ElButton size="small" @click="loadData">刷新</ElButton>
            </div>
          </template>
          <ElDescriptions v-if="serverInfo" :column="1" label-position="left" border size="small">
            <ElDescriptionsItem label="机器名">{{ serverInfo.machineName }}</ElDescriptionsItem>
            <ElDescriptionsItem label="操作系统">{{ serverInfo.osDescription }}</ElDescriptionsItem>
            <ElDescriptionsItem label="架构">{{ serverInfo.osArchitecture }}</ElDescriptionsItem>
            <ElDescriptionsItem label="运行时">{{ serverInfo.frameworkDescription }}</ElDescriptionsItem>
            <ElDescriptionsItem label="CPU 核心">{{ serverInfo.processorCount }}</ElDescriptionsItem>
            <ElDescriptionsItem label="工作集内存">{{ formatBytes(serverInfo.workingSetBytes) }}</ElDescriptionsItem>
            <ElDescriptionsItem label="GC 堆内存">{{ formatBytes(serverInfo.gcHeapBytes) }}</ElDescriptionsItem>
            <ElDescriptionsItem label="进程运行时间">{{ formatUptime(serverInfo.processUptime) }}</ElDescriptionsItem>
          </ElDescriptions>
        </ElCard>

        <ElCard shadow="never" size="small">
          <template #header>
            <span class="font-medium">Redis 状态</span>
          </template>
          <ElDescriptions v-if="redisInfo" :column="1" label-position="left" border size="small">
            <ElDescriptionsItem label="已配置">
              <ElTag :type="redisInfo.configured ? 'success' : 'info'" size="small">
                {{ redisInfo.configured ? '是' : '否' }}
              </ElTag>
            </ElDescriptionsItem>
            <ElDescriptionsItem label="已连接">
              <ElTag :type="redisInfo.connected ? 'success' : 'danger'" size="small">
                {{ redisInfo.connected ? '是' : '否' }}
              </ElTag>
            </ElDescriptionsItem>
            <ElDescriptionsItem label="版本">{{ redisInfo.serverVersion || '-' }}</ElDescriptionsItem>
            <ElDescriptionsItem label="端点">{{ redisInfo.endPoint || '-' }}</ElDescriptionsItem>
            <ElDescriptionsItem label="Key 数量">{{ redisInfo.dbSize ?? '-' }}</ElDescriptionsItem>
            <ElDescriptionsItem v-if="redisInfo.error" label="错误">
              <span class="text-red-500">{{ redisInfo.error }}</span>
            </ElDescriptionsItem>
          </ElDescriptions>
        </ElCard>
      </div>

      <ElCard v-if="serverInfo?.disks?.length" shadow="never" size="small" class="mt-16px">
        <template #header>
          <span class="font-medium">磁盘使用</span>
        </template>
        <TableData
          :columns="[
            { key: 'name', title: '盘符' },
            { key: 'driveType', title: '类型' },
            { key: 'total', title: '总容量' },
            { key: 'free', title: '可用' },
            { key: 'used', title: '使用率' }
          ]"
          :data="
            serverInfo.disks.map(d => ({
              name: d.name,
              driveType: d.driveType,
              total: formatBytes(d.totalBytes),
              free: formatBytes(d.freeBytes),
              used: `${d.usedPercent}%`
            }))
          "
          size="small"
        />
      </ElCard>
    </div>
  </div>
</template>

<style scoped></style>
