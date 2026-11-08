import { useRouter } from 'vue-router';
import type { RouteLocationRaw } from 'vue-router';
import type { RouteKey } from '@elegant-router/types';
import { router as globalRouter } from '@/router';
import { parseSameAppHashPath } from '@/router/utils/sameAppLink';

/**
 * Router push
 *
 * Jump to the specified route, it can replace function router.push
 *
 * @param inSetup Whether is in vue script setup
 */
export function useRouterPush(inSetup = true) {
    const router = inSetup ? useRouter() : globalRouter;
    const route = globalRouter.currentRoute;

    const routerPush = router.push;

    const routerBack = router.back;

    async function pushNotFound() {
        return routerPush({ name: '404' });
    }

    async function routerPushByKey(key: RouteKey, options?: App.Global.RouterPushOptions) {
        const { query, params } = options || {};

        // 无此 name 时不抛 No match，进内置 404 页
        if (!router.hasRoute(key as string)) {
            return pushNotFound();
        }

        const routeLocation: RouteLocationRaw = {
            name: key
        };

        if (Object.keys(query || {}).length) {
            routeLocation.query = query;
        }

        if (Object.keys(params || {}).length) {
            routeLocation.params = params;
        }

        try {
            return await routerPush(routeLocation);
        } catch {
            return pushNotFound();
        }
    }

    function routerPushByKeyWithMetaQuery(key: RouteKey, menuItem?: any) {
        //console.log('menuItem', menuItem);
        if (menuItem?.raw) {
            var raw = menuItem.raw;
            // 菜单类型「网页链接」+ 不内嵌
            if (raw.meta?.menuType == '3' && !raw.meta.isIframe) {
                var url = raw.meta.isLink as string;
                // 本系统 hash：站内跳到对应 builtin/动态路由
                const innerPath = parseSameAppHashPath(url);
                if (innerPath) {
                    return routerPush(innerPath);
                }
                if (url) {
                    window.open(url, '_blank');
                    return;
                }
            }
        }
        const allRoutes = router.getRoutes();
        const meta = allRoutes.find(item => item.name === key)?.meta || null;

        const query: Record<string, string> = {};

        meta?.query?.forEach(item => {
            query[item.key] = item.value;
        });

        return routerPushByKey(key, { query });
    }

    async function toHome() {
        return routerPushByKey('root');
    }

    /**
     * Navigate to login page
     *
     * @param loginModule The login module
     * @param redirectUrl The redirect url, if not specified, it will be the current route fullPath
     */
    async function toLogin(loginModule?: UnionKey.LoginModule, redirectUrl?: string) {
        const module = loginModule || 'pwd-login';

        const options: App.Global.RouterPushOptions = {
            params: {
                module
            }
        };

        const redirect = redirectUrl || route.value.fullPath;

        options.query = {
            redirect
        };

        return routerPushByKey('login', options);
    }

    /**
     * Toggle login module
     *
     * @param module
     */
    async function toggleLoginModule(module: UnionKey.LoginModule) {
        const query = route.value.query as Record<string, string>;

        return routerPushByKey('login', { query, params: { module } });
    }

    /**
     * Redirect from login
     *
     * @param [needRedirect=true] Whether to redirect after login. Default is `true`
     */
    async function redirectFromLogin(needRedirect = true) {
        const locationQuery: Record<string, any> = {};
        try {
            new URLSearchParams(window.location.search).forEach((value, key) => {
                locationQuery[key] = value;
            });
        } catch { }

        try {
            const hash = window.location.hash || '';
            const qIndex = hash.indexOf('?');
            if (qIndex !== -1) {
                const hashQuery = hash.slice(qIndex + 1);
                new URLSearchParams(hashQuery).forEach((value, key) => {
                    locationQuery[key] = value;
                });
            }
        } catch { }

        const rawQueryFromRoute = (route.value.query || {}) as Record<string, any>;
        const mergedQuery = { ...rawQueryFromRoute, ...locationQuery };

        const redirect = mergedQuery?.redirect as string;
        if (needRedirect && redirect) {
            // 兼容 ucmlVue：登录后把 ssojump 上除 ticket/redirect 外的 query 一并拼到目标路由
            // 否则集成平台跳转时会丢失 WorkID/FK_Flow/FK_Node 等参数。
            const ssoQuery: Record<string, any> = { ...mergedQuery };
            delete ssoQuery.redirect;
            delete ssoQuery.ticket;

            const decodedRedirect = decodeURIComponent(redirect);
            let targetPath = decodedRedirect;
            const redirectQuery: Record<string, any> = {};

            // redirect 可能本身携带 ?xxx=yyy
            if (decodedRedirect.includes('?')) {
                const u = new URL(decodedRedirect, 'http://localhost');
                targetPath = u.pathname;
                u.searchParams.forEach((value, key) => {
                    redirectQuery[key] = value;
                });
            }

            console.log("跳转到", targetPath, { ...redirectQuery, ...ssoQuery });
            await routerPush({
                path: targetPath,
                query: { ...redirectQuery, ...ssoQuery },
            });
        } else {
            console.log("跳转到首页", redirect)
            await toHome();
        }
    }

    return {
        routerPush,
        routerBack,
        routerPushByKey,
        routerPushByKeyWithMetaQuery,
        toLogin,
        toggleLoginModule,
        redirectFromLogin
    };
}
