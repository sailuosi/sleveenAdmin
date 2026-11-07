import NProgress from 'nprogress';

/** NextLoading - 页面加载进度条，与 NProgress 兼容 */
export const NextLoading = {
  start: () => NProgress.start(),
  done: () => NProgress.done(),
  inc: (amount?: number) => NProgress.inc(amount),
  set: (n: number) => NProgress.set(n)
};
