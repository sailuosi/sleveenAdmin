import { request } from '../request';

/** get constant routes */
export function fetchGetConstantRoutes() {
  return request<Api.Route.MenuRoute[]>({ url: '/api/sysMenu/constant-routes' });
}

/** get user routes (login menu tree) */
export function fetchGetUserRoutes() {
  return request<Api.Route.UserRoute>({ url: '/api/sysMenu/login-menu-tree' });
}

/** whether the route is exist */
export function fetchIsRouteExist(routeName: string) {
  return request<boolean>({ url: '/api/sysMenu/isRouteExist', params: { routeName } });
}
