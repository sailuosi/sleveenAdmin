import { createApp } from 'vue';
import './plugins/assets';
import { setupVueRootValidator } from 'vite-plugin-vue-transition-root-validator/client';
import {
  setupAppVersionNotification,
  setupDayjs,
  setupFeedback,
  setupIconifyOffline,
  setupLoading,
  setupNProgress
} from './plugins';
import { setupGlobalComponents } from '@/plugins/global-components';
import { setupDirectives } from '@/directives';
import { setupStore } from './store';
import { setupRouter } from './router';
import { getLocale, setupI18n } from './locales';
import App from './App.vue';
import ElementPlus from 'element-plus';
import zhCn from 'element-plus/es/locale/lang/zh-cn.mjs';
import 'element-plus/dist/index.css';
import 'element-plus/theme-chalk/dark/css-vars.css';

async function setupApp() {
  setupLoading();

  setupNProgress();

  setupFeedback();

  setupIconifyOffline();

  setupDayjs();

  const app = createApp(App);

  setupStore(app);

  await setupRouter(app);

  setupI18n(app);

  setupAppVersionNotification();
  app.use(ElementPlus, { locale: zhCn });
  app.config.globalProperties.$message = window.$message;
  setupGlobalComponents(app);
  setupDirectives(app);
  setupVueRootValidator(app, {
    lang: getLocale() === 'zh-CN' ? 'zh' : 'en'
  });

  // Sailflow：form-create + FcDesigner（办理/表单设计）
  const { installSailflow } = await import('@/widgets/sailflow/install');
  installSailflow(app);

  app.mount('#app');
}

setupApp();
