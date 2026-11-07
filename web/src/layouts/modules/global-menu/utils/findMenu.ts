export function findMenu(menus: App.Global.Menu[], key: string): App.Global.Menu | undefined {
  for (const menu of menus) {
    if (menu.key === key) return menu;
    if (menu.children?.length) {
      const found = findMenu(menu.children, key);
      if (found) return found;
    }
  }
  return undefined;
}
