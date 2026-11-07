import type { RouteKey } from '@elegant-router/types';
import { useRouterPush } from '@/hooks/common/router';

/**
 * el-menu select：带子菜单的父项不传第二参数给 routerPushByKeyWithMetaQuery，
 * 避免父级若误配 menuType=3 时走 window.open，与站内目录跳转冲突。
 */
export function useMenuValuePush() {
  const { routerPushByKeyWithMetaQuery } = useRouterPush();

  function onMenuValueUpdate(key: string | number, item?: App.Global.Menu) {
    const routeKey = key as RouteKey;
    if (item?.children?.length) {
      return routerPushByKeyWithMetaQuery(routeKey);
    }
    return routerPushByKeyWithMetaQuery(routeKey, item);
  }

  return { onMenuValueUpdate };
}

/** @deprecated Use useMenuValuePush */
export function useNMenuValuePush() {
  const { onMenuValueUpdate } = useMenuValuePush();
  return { onNMenuValueUpdate: onMenuValueUpdate };
}
