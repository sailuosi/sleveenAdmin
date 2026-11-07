/**
 * 解析菜单「网页链接」是否指向本系统 hash 路由。
 * 例：http://host/app/#/some-path → /some-path
 */
export function parseSameAppHashPath(link: string | null | undefined): string | null {
  if (!link || typeof link !== 'string') return null;
  const trimmed = link.trim();
  if (!trimmed) return null;

  if (trimmed.startsWith('#/')) {
    return trimmed.slice(1).split('?')[0] || null;
  }
  if (trimmed.startsWith('/') && !trimmed.startsWith('//')) {
    return trimmed.split('?')[0] || null;
  }

  try {
    const u = new URL(trimmed, window.location.origin);
    if (u.host !== window.location.host) return null;
    if (u.hash?.startsWith('#/')) {
      return u.hash.slice(1).split('?')[0] || null;
    }
  } catch {
    return null;
  }
  return null;
}
