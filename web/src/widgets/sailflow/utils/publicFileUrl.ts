import { getServiceBaseURL } from '@/utils/service';

/**
 * 将后端公开文件路径（如 /uploads/...）转成浏览器可加载的地址。
 * 开发态走 Vite 代理时，裸 /uploads 会打到前端源站导致 el-image「加载失败」。
 * 写入 Conditions / 流程变量时仍应保存原始 URL，仅展示时调用本方法。
 */
export function resolvePublicFileUrl(raw: string | null | undefined): string {
  const u = String(raw || '').trim();
  if (!u) return '';
  if (/^(blob:|data:|https?:)/i.test(u)) return u;

  const isProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
  const path = u.startsWith('/') ? u : `/${u}`;
  if (!isProxy) return path;

  const { baseURL } = getServiceBaseURL(import.meta.env, true);
  const prefix = String(baseURL || '').replace(/\/$/, '');
  return `${prefix}${path}`;
}
