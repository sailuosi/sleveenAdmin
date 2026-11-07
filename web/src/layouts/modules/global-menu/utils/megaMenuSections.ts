/** 大卡片菜单分组节点 */
export interface MegaSection {
  label: string;
  links: App.Global.Menu[];
  groups: MegaSection[];
  isGroup: boolean;
  /**
   * 分组标题对应的源菜单节点。
   * 当父级菜单配置了组件/外链（非仅目录）时，标题可点击跳转。
   */
  titleMenu?: App.Global.Menu;
}

type MenuNode = App.Global.Menu & { disabled?: boolean };

export type MenuWithMeta = MenuNode & { raw?: any };

function isDisabled(menu: App.Global.Menu) {
  return Boolean((menu as MenuNode).disabled);
}

/** 父级分组是否可跳转（配置了页面组件或外链，且非 inner 目录） */
export function canNavigateMenu(menu?: App.Global.Menu | null): menu is App.Global.Menu {
  if (!menu || isDisabled(menu)) return false;
  const withMeta = menu as MenuWithMeta;
  const fromMeta = String(withMeta.componentPath || '').trim();
  const fromRaw = String(withMeta.raw?.component || withMeta.raw?.meta?.componentPath || '').trim();
  const componentPath = fromMeta || fromRaw;
  const menuAddress = String(withMeta.menuAddress || withMeta.raw?.meta?.isLink || withMeta.raw?.meta?.href || '').trim();
  if (componentPath && componentPath !== 'inner') return true;
  return Boolean(menuAddress);
}

/** 递归收集末级可跳转菜单 */
export function collectLeafMenus(menu: App.Global.Menu): App.Global.Menu[] {
  if (!menu.children?.length) {
    if (isDisabled(menu)) return [];
    return menu.routeKey ? [menu] : [];
  }
  return menu.children.flatMap(child => collectLeafMenus(child));
}

function sectionHasContent(section: MegaSection): boolean {
  return section.links.length > 0 || section.groups.some(sectionHasContent);
}

/** 由菜单节点构建分组树，递归直到末级 */
function buildSectionFromNode(node: App.Global.Menu): MegaSection | null {
  if (!node.children?.length) {
    if (isDisabled(node)) return null;
    return { label: node.label, links: [node], groups: [], isGroup: false, titleMenu: node };
  }

  const directLeaves: App.Global.Menu[] = [];
  const groups: MegaSection[] = [];

  for (const child of node.children) {
    if (!child.children?.length) {
      if (!isDisabled(child)) directLeaves.push(child);
      continue;
    }

    const nested = buildSectionFromNode(child);
    if (nested && sectionHasContent(nested)) {
      groups.push(nested);
      continue;
    }

    const leaves = collectLeafMenus(child);
    if (leaves.length) {
      groups.push({
        label: child.label,
        links: leaves,
        groups: [],
        isGroup: true,
        titleMenu: child
      });
    }
  }

  if (!directLeaves.length && !groups.length) {
    const leaves = collectLeafMenus(node);
    if (!leaves.length) return null;
    return { label: node.label, links: leaves, groups: [], isGroup: true, titleMenu: node };
  }

  return {
    label: node.label,
    links: directLeaves,
    groups,
    isGroup: true,
    titleMenu: node
  };
}

/** 构建顶部大卡片的全部分组 */
export function buildMegaSections(rootMenu: App.Global.Menu): MegaSection[] {
  return (rootMenu.children || [])
    .map(child => buildSectionFromNode(child))
    .filter((section): section is MegaSection => section !== null && sectionHasContent(section));
}

/** 统计分组内末级链接总数（含嵌套） */
export function countMegaSectionLinks(section: Pick<MegaSection, 'links' | 'groups'>): number {
  const nested = section.groups.reduce((sum, group) => sum + countMegaSectionLinks(group), 0);
  return section.links.length + nested;
}
