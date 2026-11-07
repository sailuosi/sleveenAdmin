/**
 * 全局注册的 Vue 组件集中在此维护：在 globalComponents 数组中追加 { name, component } 即可。
 * name 为模板中使用的 PascalCase 名称（同时可用对应的 kebab-case 标签）。
 */
import mdialog from '@/ui/common/modal/mdialog.vue';

const globalComponents = [{ name: 'mdialog', component: mdialog }];

export function setupGlobalComponents(app) {
  for (const { name, component } of globalComponents) {
    app.component(name, component);
  }
}
