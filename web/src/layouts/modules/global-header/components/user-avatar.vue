<script setup lang="ts">
import { useAuthStore } from '@/store/modules/auth';
import { useRouterPush } from '@/hooks/common/router';
import { $t } from '@/locales';

defineOptions({
  name: 'UserAvatar'
});

const authStore = useAuthStore();
const { toLogin } = useRouterPush();

function loginOrRegister() {
  toLogin();
}

function logout() {
  window.$dialog?.info({
    title: $t('common.tip'),
    content: $t('common.logoutConfirm'),
    positiveText: $t('common.confirm'),
    negativeText: $t('common.cancel'),
    onPositiveClick: () => {
      authStore.resetStore(true);
    }
  });
}

function handleCommand(command: string) {
  if (command === 'logout') {
    logout();
  }
}
</script>

<template>
  <ElButton v-if="!authStore.isLogin" text @click="loginOrRegister">
    {{ $t('page.login.common.loginOrRegister') }}
  </ElButton>
  <ElDropdown v-else placement="bottom" trigger="click" @command="handleCommand">
    <div class="cursor-pointer">
      <ButtonIcon>
        <SvgIcon icon="ph:user-circle" class="text-icon-large" />
        <span class="text-16px font-medium">{{ authStore.userInfo.userName }}</span>
      </ButtonIcon>
    </div>
    <template #dropdown>
      <ElDropdownMenu>
        <ElDropdownItem command="logout">
          <div class="flex-y-center gap-8px">
            <SvgIcon icon="ph:sign-out" class="text-18px" />
            <span>{{ $t('common.logout') }}</span>
          </div>
        </ElDropdownItem>
      </ElDropdownMenu>
    </template>
  </ElDropdown>
</template>

<style scoped></style>
