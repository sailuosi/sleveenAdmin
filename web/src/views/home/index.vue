<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { ElConfigProvider } from 'element-plus';
import zhCn from 'element-plus/es/locale/lang/zh-cn';
import Paotal from '@/ui/portal/paotal.vue';
import { fetchHomePortal } from '@/service/api/sys-portal-common';

const portalKey = ref('home');
const portalOid = ref('');
const loading = ref(true);

onMounted(async () => {
  try {
    const hit = await fetchHomePortal('home');
    if (hit && typeof hit === 'object') {
      const data = hit as Record<string, string>;
      portalKey.value = data.portalKey || data.portalkey || 'home';
      portalOid.value = data.oid || '';
    }
  } catch {
    portalKey.value = 'home';
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <ElSpace direction="vertical" :size="16">
    <ElAlert :title="$t('common.tip')" type="info">
      {{ $t('page.home.branchDesc') }}
    </ElAlert>
    <ElCard shadow="never" class="card-wrapper" title="工作台门户">
      <ElConfigProvider :locale="zhCn">
        <div class="portal-home-body">
          <div v-if="loading" class="py-8 text-center">门户加载中...</div>
          <Paotal v-else :name="portalKey" :oid="portalOid" :disable="true" />
        </div>
      </ElConfigProvider>
    </ElCard>
  </ElSpace>
</template>

<style scoped></style>
