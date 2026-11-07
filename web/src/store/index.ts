import type { App } from 'vue';
import { createPinia } from 'pinia';
import { resetSetupStore } from './plugins';

/** Shared pinia instance（旧代码 /@/stores/index 会引用） */
export const pinia = createPinia();
pinia.use(resetSetupStore);

/** Setup Vue store plugin pinia */
export function setupStore(app: App) {
  app.use(pinia);
}

export default pinia;
