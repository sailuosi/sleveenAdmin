import type { App } from 'vue';
import { createI18n } from 'vue-i18n';
import { localStg } from '@/utils/storage';
import messages from './locale';

const i18n = createI18n({
  locale: localStg.get('lang') || 'zh-CN',
  fallbackLocale: 'en',
  messages,
  legacy: false
});

/**
 * Setup plugin i18n
 *
 * @param app
 */
export function setupI18n(app: App) {
  app.use(i18n);
}

export const $t = i18n.global.t as App.I18n.$T;

export const $te = ((key: string) => i18n.global.te(key)) as (key: string) => boolean;

/**
 * 菜单/标签标题：有 i18n 词条用翻译，否则回退接口 title（后端常带 route.xxx 的 i18nKey）。
 */
export function resolveRouteLabel(i18nKey?: string | null, fallback?: string | null): string {
  if (i18nKey && $te(i18nKey)) {
    return $t(i18nKey);
  }
  return String(fallback || i18nKey || '');
}

export function setLocale(locale: App.I18n.LangType) {
  i18n.global.locale.value = locale;
}

export function getLocale(): App.I18n.LangType {
  return i18n.global.locale.value as App.I18n.LangType;
}
