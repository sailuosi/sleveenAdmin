import type { ElegantConstRoute, RouteKey } from '@elegant-router/types';

const LAYOUT_VIEW_SPLIT = '$';

/**
 * elegant-router treats route names containing "_" as multi-level routes.
 * Backend flat menus like `layout.base$view.ai_assistant` therefore fail Vue Router registration.
 * Split them into a synthetic parent/child pair before transform (menus still use original routes).
 */
export function patchAuthRoutesForVueRouter(routes: ElegantConstRoute[]): ElegantConstRoute[] {
  return routes.map(patchAuthRoute);
}

function patchAuthRoute(route: ElegantConstRoute): ElegantConstRoute {
  const children = route.children?.length ? route.children.map(patchAuthRoute) : undefined;

  if (shouldSplitFlatLayoutViewRoute(route, children)) {
    return splitFlatLayoutViewRoute(route);
  }

  return children ? { ...route, children } : route;
}

function shouldSplitFlatLayoutViewRoute(
  route: ElegantConstRoute,
  children?: ElegantConstRoute[]
): route is ElegantConstRoute & { component: string } {
  if (children?.length) return false;

  const { component } = route;
  if (!component?.includes(LAYOUT_VIEW_SPLIT)) return false;

  const [layoutPart, viewPart] = component.split(LAYOUT_VIEW_SPLIT);
  if (!layoutPart.startsWith('layout.') || !viewPart.startsWith('view.')) return false;

  return route.name.includes('_');
}

function splitFlatLayoutViewRoute(route: ElegantConstRoute & { component: string }): ElegantConstRoute {
  const [layoutPart, viewPart] = route.component.split(LAYOUT_VIEW_SPLIT);
  const pathSegments = route.path.split('/').filter(Boolean);
  const parentPath = pathSegments.length > 1 ? `/${pathSegments.slice(0, -1).join('/')}` : route.path;
  const parentName = route.name.split('_').slice(0, -1).join('_');

  return {
    name: parentName,
    path: parentPath,
    component: layoutPart,
    meta: {
      title: route.meta?.title ?? route.name,
      hideInMenu: true,
      activeMenu: route.name as RouteKey
    },
    children: [
      {
        ...route,
        component: viewPart,
        children: []
      }
    ]
  };
}
