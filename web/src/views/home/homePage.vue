<template>
  <BorderlessDiv>
    <paotal v-if="portalKey || portalOid" :name="portalKey" :oid="portalOid" :disabled="true" />
  </BorderlessDiv>
</template>

<script setup lang="ts">
import { onActivated, onMounted, onUnmounted, ref } from 'vue';
import paotal from '@/ui/portal/paotal.vue';
import { getHomePortal } from '@/api/system/portalCommon.js';
import mittBus from '@/utils/mitt';

defineOptions({ name: 'HomePage' });

const portalKey = ref('');
const portalOid = ref('');
const currentPageCode = ref('home');

/** 从 getHomePortal 响应中解析门户行 */
function parseHomePortalRow(res: any) {
  if (!res || res.status === 'error') return null;
  if (res.status === 'success' && res.data && (res.data.portalKey || res.data.oid)) {
    return res.data;
  }
  if (res.status === 'success' && (res.portalKey || res.oid)) {
    return res;
  }
  if (res.data?.portalKey || res.data?.oid) return res.data;
  if (res.portalKey || res.oid) return res;
  return null;
}

function applyPortalRow(row: { portalKey?: string; oid?: string }) {
  portalKey.value = row.portalKey ? String(row.portalKey) : '';
  portalOid.value = row.oid ? String(row.oid) : '';
}

const loadHomePortalKey = async (pageCode?: string) => {
  if (pageCode) {
    currentPageCode.value = pageCode;
  }
  try {
    const res: any = await getHomePortal(currentPageCode.value);
    const row = parseHomePortalRow(res);
    if (row) {
      applyPortalRow(row);
      return;
    }
    portalKey.value = '';
    portalOid.value = '';
  } catch {
    portalKey.value = '';
    portalOid.value = '';
  }
};

const onHomePageChanged = (pageCode?: string) => {
  if (!pageCode) return;
  loadHomePortalKey(pageCode);
};

onMounted(() => {
  mittBus.on('onHomePageChanged', onHomePageChanged);
  loadHomePortalKey('home');
});

onUnmounted(() => {
  mittBus.off('onHomePageChanged', onHomePageChanged);
});

onActivated(() => {
  loadHomePortalKey(currentPageCode.value || 'home');
});
</script>
