
import { routes } from './routes';
import { layouts, views } from '../elegant/imports';
import type { RouteRecordRaw } from 'vue-router';
import type {
    AutoRouterRedirect,
    AutoRouterRoute,
    AutoRouterSingleView,
    RawRouteComponent,
    RouteFileKey,
    RouteLayoutKey
} from '@elegant-router/types';
import { parseSameAppHashPath } from '@/router/utils/sameAppLink';

/** Special SleveenAdmin menu component → generated view key overrides */
const MOO_VIEW_ALIAS: Record<string, string> = {
  'view.p1_inbox': '_views_p1_notice_inbox',
  'view.p1_online': '_views_p1_online_user',
  'view.manage_auth-bag': '_views_manage_auth_bag',
  'view.home': '_views_home_homePage',
  'view.ai_assistant': '_views_ai_assistant'
};

/**
 * Map SleveenAdmin `view.xxx` / `layout.base$view.xxx` to elegant-router file keys.
 * elegant-router 1.x keys for `views/foo/bar/index.vue` are `_views_foo_bar` (no `_index`).
 */
export function resolveSleveenAdminViewKey(component: string): string {
  let key = component.trim();
  if (!key) return key;

  if (key.includes('$')) {
    const parts = key.split('$');
    key = parts[parts.length - 1] || key;
  }

  if (MOO_VIEW_ALIAS[key]) {
    return MOO_VIEW_ALIAS[key];
  }

  if (key.startsWith('view.')) {
    const name = key.slice(5).replaceAll('-', '_');
    // view.manage_user → _views_manage_user
    // view.manage_auth-bag → _views_manage_auth_bag
    return `_views_${name}`;
  }

  if (key.startsWith('layout.')) {
    // directory-only layout nodes are skipped elsewhere
    return key;
  }

  return component;
}

/**
 * get route name by route path
 * @param path route path
 */
export function getRouteName(path: string) {

    var routeName = routes.find(route => route.path === path)?.name;
    return routeName;
}

/** 兼容后端 Children / children */
function getMenuChildren(raw: any): any[] {
    if (!raw || typeof raw !== 'object') return [];
    const kids = raw.children ?? raw.Children;
    return Array.isArray(kids) ? kids : [];
}

//把路由展平
function travelMenuTree(menulist: any, res: any[]) {
    if (!Array.isArray(menulist)) return res;

    for (var i = 0; i < menulist.length; i++) {
        var raw = menulist[i];
        if (!raw || typeof raw !== 'object') continue;
        // 先取 children：preFormatRoute 会删掉树节点上的 children，避免泄漏进 vue-router
        const kids = getMenuChildren(raw);
        var item = preFormatRoute(raw);
        if (item != null && item != undefined) {
            res.push(item);
        }
        // 父级 component 为 inner / 目录时 preFormatRoute 返回 null，但仍必须展开 children
        if (kids.length) {
            travelMenuTree(kids, res);
        }
    }
    return res;
}

//超前对接口返回的菜单进行格式处理，处理组件格式、布局

/** 规范化后端菜单树字段，供侧栏与路由共用 */
export function normalizeMenuRoutes(list: any[]): any[] {
    if (!Array.isArray(list)) return [];
    return list.filter((item) => item && typeof item === 'object').map((item) => {
        if (!item.path && item.Path) item.path = item.Path;
        if (!item.name && item.Name) item.name = item.Name;
        if (!item.component && item.Component) item.component = item.Component;
        // Sm_Type：1目录 2菜单 3按钮（勿与 meta.menuType 页面形态混淆）
        if (item.type == null && item.Type != null) item.type = item.Type;
        item.meta = item.meta || item.Meta || {};
        if (item.Meta && item.meta !== item.Meta) {
            item.meta = { ...item.Meta, ...item.meta };
        }
        if (!item.meta.title && (item.Title || item.title)) item.meta.title = item.Title || item.title;
        if (item.meta.menuType == null && item.meta.MenuType != null) item.meta.menuType = item.meta.MenuType;
        const kids = item.children ?? item.Children;
        if (Array.isArray(kids) && kids.length) {
            item.children = normalizeMenuRoutes(kids);
        }
        return item;
    });
}
export function formatMenu(menulist: AutoRouterRoute[]) {
    var menus = travelMenuTree(menulist, []);
    return transformToVueRoutes(menus, layouts, views);
}
export function preFormatRoute(route: any) {
    if (!route || typeof route !== 'object') return null;

    // 统一 children 字段，避免后续菜单树读不到子节点
    const kids = getMenuChildren(route);
    if (kids.length) {
        route.children = kids;
    }

    route.meta = route.meta || {};
    var layout = route.layout;
    if (layout == void 0 || layout == null || layout == undefined) {
        route.layout = 'base';
    }

    // 后端字段可能是 Component / Path / Name / Type
    if (!route.component && route.Component) route.component = route.Component;
    if (!route.path && route.Path) route.path = route.Path;
    if (!route.name && route.Name) route.name = route.Name;
    if (route.type == null && route.Type != null) route.type = route.Type;

    // 目录只作分组：用 Sm_Type（1目录），绝不能用 meta.menuType（1=Vue页面）
    const routeType = Number(route.type ?? route.Type);
    const rawCompEarly = typeof route.component === 'string' ? route.component.trim() : '';
    if (routeType === 1 || /^layout\./i.test(rawCompEarly) || rawCompEarly === 'inner') {
        return null;
    }

    const rawComponentPath = typeof route.component === 'string' ? route.component : '';
    route.meta.componentPath = rawComponentPath || route.meta.componentPath || '';
    route.meta.menuAddress = route.meta?.isLink || route.meta?.href || route.meta?.menuAddress || '';
    var component = route.component;
    // meta.menuType：页面形态 2=内嵌 → frame 页
    if (route.meta.menuType == "2" || route.meta.menuType == 2) {
        component = '_views_home_framePage';
    }

    // SleveenAdmin Soybean elegant keys → elegant-router 1.x file keys
    // e.g. layout.base$view.manage_user | view.p1_tenant
    if (typeof component === 'string') {
        component = resolveSleveenAdminViewKey(component);
    }
    // 网页链接：仅外链写 href。本系统 hash 不写，避免 guard 退回上一页并覆盖 builtin
    if (route.meta?.menuType == "3" && route.meta?.isLink) {
        if (!parseSameAppHashPath(route.meta.isLink)) {
            route.meta.href = route.meta.isLink;
        }
    }

    // 无效路径不注册，避免空 path / 根 path 把整站路由打乱导致启动死循环
    const routePath = String(route.path || '').trim().replace(/\/+$/, '') || '';
    if (!routePath || routePath === '/' || routePath === '/base-layout') {
        return null;
    }
    route.path = routePath.startsWith('/') ? routePath : `/${routePath}`;

    // 空 / 未配置 component → 注册为 404 页（仍挂菜单 name，点击可进内容区）
    if (component == null || component === undefined || (typeof component === 'string' && !component.trim())) {
        component = '_views__builtin_404_404Page';
    }
    if (typeof component == 'string') {
        const comp = component.trim();
        // 布局占位不是页面，仍跳过注册
        if (comp === 'inner' || /^layout$/i.test(comp)) {
            return null;
        }
        component = comp;
        // 短路由 /repeatReviewPage1：菜单 Path 可写短路径，组件固定落到审阅台页面
        const routeName = String(route.name || '');
        const REPEAT_REVIEW_VIEW = '_widgets_bid_src_views_repeat_Page_review_BPO_Bd_RepeatReview';
        if (
            route.path === '/repeatReviewPage1' ||
            routeName === 'repeatReviewPage1' ||
            component === 'repeatReviewPage1' ||
            component === '/repeatReviewPage1'
        ) {
            component = REPEAT_REVIEW_VIEW;
        }
        //支持直接复制的源代码路径 src\widgets\sysext\views\menu\menuPage.vue
        try {
            if (component.startsWith('src')) {
                component = component.substring(3);
            }
        }
        catch (error) {
            component = '_views__builtin_404_404Page';
        }

        //替换反斜杠
        component = component.replaceAll('\\', '_');
        component = component.replaceAll('/', '_');
        //_widgets_sysext_views_sys_config_configPage

        //配置时，可以是路径格式
        if (component.startsWith('widgets_')) {
            component = "_widgets_" + component.substring(8);
        }
        if (component.endsWith('.vue')) {
            component = component.substring(0, component.length - 4);
        }
        if (views[component as RouteFileKey] != void 0) {
            component = views[component as RouteFileKey];
        } else if (layouts[component as RouteLayoutKey] != void 0) {
            // layout 组件不能当页面挂；当作分组跳过
            return null;
        } else {
            component = views["_views__builtin_404_404Page"];
        }
    }

    //console.log("格式化后的组件",component);
    route.component = component;
    if (!route.meta.title && (route.Title || route.title)) {
        route.meta.title = route.Title || route.title;
    }

    // 展平注册：去掉树形 children，避免 raw 字符串 component 泄漏进 vue-router
    delete route.children;
    delete route.Children;

    return route;
}



export function transformToVueRoutes(
    routes: AutoRouterRoute[],
    layouts: Record<RouteLayoutKey, RawRouteComponent>,
    views: Record<RouteFileKey, RawRouteComponent>
) {
    const groupedRoutes = getFormattedRoutes(routes);

    const vueRoutes: RouteRecordRaw[] = [];

    groupedRoutes.forEach((items, layout) => {
        if (!layout || !layouts[layout]) return;
        const layoutRoute: RouteRecordRaw = {
            path: `/${layout}-layout`,
            component: layouts[layout],
            children: items.map(item => {
                const {
                    layout: _layout,
                    component,
                    children: _children,
                    Children: _Children,
                    Component: _Component,
                    ...rest
                } = item as AutoRouterSingleView & Record<string, unknown>;
                let resolved: RawRouteComponent | undefined =
                    typeof component === 'string'
                        ? views[component as RouteFileKey] || views['_views__builtin_404_404Page']
                        : (component as RawRouteComponent);
                if (!resolved) {
                    resolved = views['_views__builtin_404_404Page'];
                }
                return {
                    ...rest,
                    component: resolved
                };
            })
        };

        vueRoutes.push(layoutRoute);
    });

    return vueRoutes;
}

function getFormattedRoutes(routes: AutoRouterRoute[]) {
    const groupedRoutes = new Map<RouteLayoutKey, AutoRouterSingleView[]>();
    if (!Array.isArray(routes)) {
        return groupedRoutes;
    }
    routes.forEach(route => {
        if (!route) return;
        const layoutKey = (route.layout || 'base') as RouteLayoutKey;
        const items = groupedRoutes.get(layoutKey) || [];
        items.push(route);
        groupedRoutes.set(layoutKey, items);
    });

    return groupedRoutes;
}
