declare module '@/service/api/bpoCommon.js' {
  import type { PageInitLike } from '@/utils/pageInitCache';

  export function fetchPageInitByBpoName(bpoName: string): Promise<PageInitLike>;
}
