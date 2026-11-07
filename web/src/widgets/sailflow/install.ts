import type { App } from 'vue';
import formCreate from '@form-create/element-ui';
import FcDesigner from '@form-create/designer';

/** 在宿主 main 中调用：installSailflow(app) */
export function installSailflow(app: App) {
  app.use(formCreate);
  app.use(FcDesigner);
}

export default installSailflow;
