import type { CustomRoute, ElegantConstRoute, ElegantRoute } from 'elegant-router/types';
import { routes } from '../elegant/routes';
import { formatMenu } from '@/router/elegant/other';
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
  // 组件缺失 / 空 component 时 formatMenu 已兜底到 404 页，保持真实 404 内容
  return formatMenu(routes);
}
