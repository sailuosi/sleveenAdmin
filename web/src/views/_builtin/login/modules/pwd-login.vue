<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { loginModuleRecord } from '@/constants/app';
import { fetchCaptcha } from '@/service/api';
import { useAuthStore } from '@/store/modules/auth';
import { useRouterPush } from '@/hooks/common/router';
import { useFormRules, useAppForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({
  name: 'PwdLogin'
});

const authStore = useAuthStore();
const { toggleLoginModule } = useRouterPush();
const { formRef, validate } = useAppForm();

interface FormModel {
  userName: string;
  password: string;
  captchaCode: string;
}

const model: FormModel = reactive({
  userName: 'admin',
  password: '123456',
  captchaCode: ''
});

const captchaId = ref('');
const captchaImage = ref('');

const rules = computed<Record<keyof FormModel, App.Global.FormRule[]>>(() => {
  const { formRules } = useFormRules();

  return {
    userName: formRules.userName,
    password: formRules.pwd,
    captchaCode: [{ required: true, message: '请输入验证码', trigger: 'blur' }]
  };
});

async function loadCaptcha() {
  const { data, error } = await fetchCaptcha();
  if (!error && data) {
    captchaId.value = data.captchaId;
    captchaImage.value = data.captchaImage;
    model.captchaCode = '';
  }
}

onMounted(loadCaptcha);

async function handleSubmit() {
  await validate();
  await authStore.login(model.userName, model.password, captchaId.value, model.captchaCode);
  await loadCaptcha();
}

type AccountKey = 'admin';

interface Account {
  key: AccountKey;
  label: string;
  userName: string;
  password: string;
}

const accounts = computed<Account[]>(() => [
  {
    key: 'admin',
    label: $t('page.login.pwdLogin.admin'),
    userName: 'admin',
    password: '123456'
  }
]);

async function handleAccountLogin(account: Account) {
  model.userName = account.userName;
  model.password = account.password;
  await handleSubmit();
}
</script>

<template>
  <ElForm ref="formRef" :model="model" :rules="rules" size="large" :show-label="false" @keyup.enter="handleSubmit">
    <ElFormItem prop="userName">
      <ElInput v-model="model.userName" :placeholder="$t('page.login.common.userNamePlaceholder')" />
    </ElFormItem>
    <ElFormItem prop="password">
      <ElInput
        v-model="model.password"
        type="password"
        show-password
        :placeholder="$t('page.login.common.passwordPlaceholder')"
      />
    </ElFormItem>
    <ElFormItem prop="captchaCode">
      <div class="w-full flex gap-12px">
        <ElInput v-model="model.captchaCode" placeholder="验证码" class="flex-1" />
        <img
          v-if="captchaImage"
          :src="captchaImage"
          alt="captcha"
          class="h-40px w-120px cursor-pointer border border-#eee rounded-4px"
          @click="loadCaptcha"
        />
      </div>
    </ElFormItem>
    <div class="flex flex-col gap-24px">
      <div class="flex-y-center justify-between">
        <ElCheckbox>{{ $t('page.login.pwdLogin.rememberMe') }}</ElCheckbox>
        <ElButton text @click="toggleLoginModule('reset-pwd')">
          {{ $t('page.login.pwdLogin.forgetPassword') }}
        </ElButton>
      </div>
      <ElButton type="primary" size="large" round class="w-full" :loading="authStore.loginLoading" @click="handleSubmit">
        {{ $t('common.confirm') }}
      </ElButton>
      <div class="flex-y-center justify-between gap-12px">
        <ElButton class="flex-1" @click="toggleLoginModule('code-login')">
          {{ $t(loginModuleRecord['code-login']) }}
        </ElButton>
        <ElButton class="flex-1" @click="toggleLoginModule('register')">
          {{ $t(loginModuleRecord.register) }}
        </ElButton>
      </div>
      <ElDivider content-position="center">{{ $t('page.login.pwdLogin.otherAccountLogin') }}</ElDivider>
      <div class="flex-center gap-12px">
        <ElButton v-for="item in accounts" :key="item.key" type="primary" @click="handleAccountLogin(item)">
          {{ item.label }}
        </ElButton>
      </div>
    </div>
  </ElForm>
</template>

<style scoped></style>
