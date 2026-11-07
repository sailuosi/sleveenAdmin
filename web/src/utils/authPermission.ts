import { unref, type Ref } from 'vue';
import { useAuthStore } from '@/store/modules/auth';
import { getPageInitByBpoName, type PageInitLike } from '@/utils/pageInitCache';

export const ALL_PERMISSION = '*:*:*';

export type AuthBinding =
  | string
  | [code: string, pageOrBpo: PageInitLike | Ref<PageInitLike | null | undefined> | string | null | undefined];

export function normalizeButtons(raw: unknown): string[] {
  if (Array.isArray(raw)) {
    return [...new Set(raw.filter((x): x is string => typeof x === 'string'))];
  }
  if (raw && typeof raw === 'object') {
    return [
      ...new Set(
        Object.values(raw as Record<string, unknown>)
          .flatMap(v => (Array.isArray(v) ? v : []))
          .filter((x): x is string => typeof x === 'string')
      )
    ];
  }
  return [];
}

function isTruthyFlag(v: unknown) {
  return v === true || v === 1 || v === '1' || v === 'true';
}

export function hasGlobalAuth(code: string, buttons?: string[], isLogin?: boolean) {
  if (isLogin === false) {
    return false;
  }
  const authStore = useAuthStore();
  if (!authStore.isLogin) {
    return false;
  }
  if (!code) {
    return false;
  }
  const list = buttons ?? authStore.userInfo.buttons;
  if (!Array.isArray(list)) {
    return false;
  }
  if (list.includes(ALL_PERMISSION)) {
    return true;
  }
  return list.includes(code);
}

export function normalizePmcode(raw: unknown): string[] {
  if (Array.isArray(raw)) {
    return raw.filter((x): x is string => typeof x === 'string');
  }
  return [];
}

/** page init 尚未返回（BPO 异步加载 pmcode 前） */
export function isPageInitPending(page?: PageInitLike | null) {
  if (!page || typeof page !== 'object') {
    return true;
  }
  return !('pmcode' in page) && !('codeTable' in page);
}

export function hasPageAuth(code: string, page?: PageInitLike | null) {
  if (!page || !code) {
    return false;
  }
  const pmcode = normalizePmcode(page.pmcode);
  if (pmcode.includes(code)) {
    return true;
  }
  const rw = page.rw;
  if (rw && typeof rw === 'object' && isTruthyFlag(rw[code])) {
    return true;
  }
  return false;
}

export function resolveAuthBinding(value: AuthBinding) {
  if (Array.isArray(value) && typeof value[0] === 'string') {
    const code = value[0];
    const second = unref(value[1]);
    if (typeof second === 'string' && second.startsWith('BPO_')) {
      return { mode: 'pageByBpo' as const, code, bpoName: second };
    }
    return { mode: 'page' as const, code, page: second as PageInitLike | null | undefined };
  }
  return { mode: 'global' as const, code: value as string };
}

export function checkAuth(value: AuthBinding) {
  const resolved = resolveAuthBinding(value);
  if (resolved.mode === 'global') {
    return hasGlobalAuth(resolved.code);
  }
  if (resolved.mode === 'page') {
    return hasPageAuth(resolved.code, resolved.page);
  }
  return false;
}

export async function checkAuthAsync(value: AuthBinding) {
  const resolved = resolveAuthBinding(value);
  if (resolved.mode === 'global') {
    return hasGlobalAuth(resolved.code);
  }
  if (resolved.mode === 'page') {
    return hasPageAuth(resolved.code, resolved.page);
  }
  if (resolved.mode === 'pageByBpo') {
    try {
      const page = await getPageInitByBpoName(resolved.bpoName);
      return hasPageAuth(resolved.code, page);
    } catch {
      return false;
    }
  }
  return false;
}
