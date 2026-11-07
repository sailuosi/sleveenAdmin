import { defineConfig } from 'elegant-router';

export default defineConfig({
  // 页面目录
  pageDir:"src/",
  // 布局配置
  layouts: {
      base: 'src/layouts/base-layout/index.vue',
      blank: 'src/layouts/blank-layout/index.vue'
  },
  pageInclude:['**/*Page.vue','**/BPO_*.vue'],
  // 复用路由配置
  reuseRoutes: ['/reuse1', '/reuse2/:id', '/reuse3/:id?', '/reuse4/:id?/:name?'],
  // 默认复用路由组件
  defaultReuseRouteComponent: 'wip',
  routerGeneratedDir:"src/router/elegant",
  // 根路由重定向
  rootRedirect: '/dashboard',
  // 404 路由组件
  notFoundRouteComponent: 'NotFound'
});
