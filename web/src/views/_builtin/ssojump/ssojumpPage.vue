<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/store/modules/auth';
import { getServiceBaseURL } from '@/utils/service';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const loadingText = ref('正在进行单点登录...');

function resolveSsoBase() {
  const rawBase = baseURL || import.meta.env.VITE_SERVICE_BASE_URL || '';
  if (!rawBase) return window.location.origin;
  try {
    const parsed = new URL(rawBase, window.location.origin);
    const pathPrefix = parsed.pathname.replace(/\/$/, '');
    if (/^https?:/i.test(parsed.protocol)) {
      // In production we always use same-origin path, mirroring zaihai mode.
      return `${window.location.origin}${pathPrefix}`;
    }
    return parsed.pathname.replace(/\/$/, '');
  } catch {
    return rawBase.replace(/\/$/, '');
  }
}

function goToSso(redirect?: string) {
  const base = resolveSsoBase();
  const encoded = redirect ? `?redirect=${encodeURIComponent(redirect)}` : '';
  window.location.href = `${base}/api/sysAuth/jcptsso${encoded}`;
}

onMounted(async () => {
  const ticket = String(route.query.ticket || '');
  if (!ticket) {
    loadingText.value = '未检测到票据，正在跳转统一认证...';
    goToSso(typeof route.query.redirect === 'string' ? route.query.redirect : '/home');
    return;
  }

  try {
    await authStore.loginBySsoTicket(ticket, true);
  } catch (error) {
    loadingText.value = '票据无效，正在重新发起统一认证...';
    goToSso(typeof route.query.redirect === 'string' ? route.query.redirect : '/home');
    return;
  }

  if (!authStore.isLogin) {
    await router.replace('/login/pwd-login');
  }
});
</script>

<template>
  <div class="size-full flex-center">
    <div class="text-center">
      <div class="mb-16px text-18px">{{ loadingText }}</div>
    </div>
  </div>
</template>

<style scoped></style>
