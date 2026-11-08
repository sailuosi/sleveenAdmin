import type { RouteMeta } from 'vue-router';
import ElegantRouter from 'elegant-router/vite';
//import type { RouteKey } from '@elegant-router/types';

export function setupElegantRouter() {
  return ElegantRouter({
    routerGeneratedDir:"src/router/elegant",
    layouts: {
      base: 'src/layouts/base-layout/index.vue',
      blank: 'src/layouts/blank-layout/index.vue'
    },
    //defaultLayout:'base',
    pageDir:"src/",
    // *Page/Biz_ for widgets; views/**/index.vue for SleveenAdmin Soybean menus (view.xxx)
    pageInclude:['**/*Page.vue','**/Biz_*.vue','**/views/**/index.vue'],
    getRouteName(node) {
      // hyphen in folder names (auth-bag) must become `_` for valid object keys
      return node.path.replaceAll('/', '_').replaceAll('-', '_');
    },
    //pageInclude:['**/index.vue','**/*Page.{vue,tsx}','**/Biz_*.vue'],
    //pagePatterns: ['**/index.vue','**/*Page.{vue,tsx}','**/Biz_*.vue'],
    // getRouteName(node) {
    //   const key =node.name;// routeName as RouteKey;

    //   if (key === 'login') {
    //     const modules: UnionKey.LoginModule[] = ['pwd-login', 'code-login', 'register', 'reset-pwd', 'bind-wechat'];

    //     const moduleReg = modules.join('|');

    //     return `/login/:module(${moduleReg})?`;
    //   }
    //   // if(routePath.indexOf('/views_') > -1) {
    //   //   routePath= routePath.replace('/views_', '');
    //   // }

    //   return node.path;
    // },
    // routeNameTransformer(routeName) {
    //   if(routeName.startsWith('views_')) {
    //     return routeName.replace('views_', '');
    //   }
    //   if(routeName.startsWith('widgets_')) {
    //     return routeName.replace('widgets_', '');
    //   }
    //   // if(routeName.indexOf('_views_') > -1) {
    //   //   return routeName.replace('_views_', '_');
    //   // }
    //   return routeName;
    // },
    // onRouteMetaGen(routeName) {
    //   const key = routeName as RouteKey;

    //   const constantRoutes: RouteKey[] = ['login', '403', '404', '500','home'];

    //   const meta: Partial<RouteMeta> = {
    //     title: key,
    //     i18nKey: `route.${key}` as App.I18n.I18nKey
    //   };

    //   if (constantRoutes.includes(key)) {
    //     meta.constant = true;
    //   }

    //   return meta;
    // }
  });
  // return ElegantVueRouter.vueRouterVitePlugin({
  //   layouts: {
  //     base: 'src/layouts/base-layout/index.vue',
  //     blank: 'src/layouts/blank-layout/index.vue'
  //   },
  //   defaultLayout:'base',
  //   pageDir:"src/",
  //   pagePatterns:['**/*Page.vue','**/Biz_*.vue'],
  //   //pageInclude:['**/index.vue','**/*Page.{vue,tsx}','**/Biz_*.vue'],
  //   //pagePatterns: ['**/index.vue','**/*Page.{vue,tsx}','**/Biz_*.vue'],
  //   routePathTransformer(routeName, routePath) {
  //     const key = routeName as RouteKey;

  //     if (key === 'login') {
  //       const modules: UnionKey.LoginModule[] = ['pwd-login', 'code-login', 'register', 'reset-pwd', 'bind-wechat'];

  //       const moduleReg = modules.join('|');

  //       return `/login/:module(${moduleReg})?`;
  //     }
  //     // if(routePath.indexOf('/views_') > -1) {
  //     //   routePath= routePath.replace('/views_', '');
  //     // }

  //     return routePath;
  //   },
  //   routeNameTransformer(routeName) {
  //     if(routeName.startsWith('views_')) {
  //       return routeName.replace('views_', '');
  //     }
  //     if(routeName.startsWith('widgets_')) {
  //       return routeName.replace('widgets_', '');
  //     }
  //     // if(routeName.indexOf('_views_') > -1) {
  //     //   return routeName.replace('_views_', '_');
  //     // }
  //     return routeName;
  //   },
  //   onRouteMetaGen(routeName) {
  //     const key = routeName as RouteKey;

  //     const constantRoutes: RouteKey[] = ['login', '403', '404', '500','home'];

  //     const meta: Partial<RouteMeta> = {
  //       title: key,
  //       i18nKey: `route.${key}` as App.I18n.I18nKey
  //     };

  //     if (constantRoutes.includes(key)) {
  //       meta.constant = true;
  //     }

  //     return meta;
  //   }
  // });
}
