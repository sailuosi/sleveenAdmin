/**
 * 大卡片菜单「二级分组标题」点击跳转白名单。
 *
 * - 命中一级菜单的 label / key / routeKey 任一即可
 * - 仅白名单内的模块：二级标题在具备页面组件/外链时才可跳转
 * - 未命中的模块：二级标题只作分组展示，不可点击跳转（三级链接不受影响）
 * - 需要给其它模块也开二级跳转时，往数组里追加对应一级菜单名或 key 即可
 */
export const MEGA_MENU_SECONDARY_TITLE_NAV_ALLOWLIST: string[] = [
  '机电运输'
];

/** 当前一级菜单是否允许其下二级标题跳转 */
export function allowSecondaryTitleNavigate(rootMenu?: App.Global.Menu | null): boolean {
  if (!rootMenu) return false;

  const allowlist = MEGA_MENU_SECONDARY_TITLE_NAV_ALLOWLIST.map(item => String(item).trim()).filter(Boolean);
  if (!allowlist.length) return false;

  const candidates = [rootMenu.label, rootMenu.key, rootMenu.routeKey]
    .map(item => String(item || '').trim())
    .filter(Boolean);

  return allowlist.some(allowed => candidates.some(candidate => candidate === allowed));
}
