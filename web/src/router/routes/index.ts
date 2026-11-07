import type { CustomRoute, ElegantConstRoute, ElegantRoute } from 'elegant-router/types';
import { routes  } from '../elegant/routes';
import { layouts, views } from '../elegant/imports';
import { transformToVueRoutes  } from '../elegant/transformer';
import {  formatMenu } from '@/router/elegant/other';
/**
 * custom routes
 *
 * @link https://github.com/soybeanjs/elegant-router?tab=readme-ov-file#custom-route
 */
const customRoutes: CustomRoute[] = [];

/** create routes when the auth route mode is static */
export function createStaticRoutes() {
  const constantRoutes: ElegantRoute[] = [];

  const authRoutes: ElegantRoute[] = [];

  [...customRoutes, ...routes ].forEach(item => {
    if (item.meta?.constant) {
      constantRoutes.push(item);
    } else {
      authRoutes.push(item);
    }
  });

  return {
    constantRoutes,
    authRoutes
  };
}

/**
 * Get auth vue routes
 *
 * @param routes Elegant routes
 */
export function getAuthVueRoutes(routes: ElegantConstRoute[]) {
  const vueRoutes = formatMenu(routes);

  // 动态菜单配置了但组件缺失/不匹配时，formatMenu 内部会兜底到 404 组件。
  // 这里把“仅动态菜单里的 404 兜底组件”替换为开发中占位页。
  const fallback404 = views._views__builtin_404_404Page;
  const blankFallback = views._widgets_sysext_views_sys_notice_blankPage;

  for (const layoutRoute of vueRoutes) {
    const children = Array.isArray(layoutRoute.children) ? layoutRoute.children : [];
    for (const child of children) {
      if (child.component === fallback404) {
        child.component = blankFallback;
      }
    }
  }

  return vueRoutes;
  //return transformToVueRoutes(routes, layouts, views);
}
