import { fetchPageInitByBpoName } from '@/service/api/bpoCommon.js';
import { useAuthStore } from '@/store/modules/auth';

const TTL_MS = 20 * 60 * 1000;

export type PageInitLike = Record<string, unknown> & {
  pmcode?: unknown[];
  rw?: Record<string, unknown>;
};

type CacheEntry = {
  data: PageInitLike;
  expiresAt: number;
};

const cache = new Map<string, CacheEntry>();
const inflight = new Map<string, Promise<PageInitLike>>();

function cacheKey(userId: string, bpoName: string) {
  return `${userId}:${bpoName}`;
}

export function clearPageInitCache() {
  cache.clear();
  inflight.clear();
}

export async function getPageInitByBpoName(bpoName: string): Promise<PageInitLike> {
  const authStore = useAuthStore();
  const userId = authStore.userInfo?.userId || authStore.token || 'anonymous';
  const key = cacheKey(String(userId), bpoName);
  const now = Date.now();
  const hit = cache.get(key);
  if (hit && hit.expiresAt > now) {
    return hit.data;
  }

  const pending = inflight.get(key);
  if (pending) {
    return pending;
  }

  const task = fetchPageInitByBpoName(bpoName)
    .then((res: PageInitLike) => {
      const data = (res || {}) as PageInitLike;
      cache.set(key, { data, expiresAt: Date.now() + TTL_MS });
      return data;
    })
    .finally(() => {
      inflight.delete(key);
    });

  inflight.set(key, task);
  return task;
}
