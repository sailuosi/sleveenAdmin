/**
 * 分帧渲染辅助（门户用）。无实现时退化为立即执行。
 */
export function useDefer(maxCount = 100) {
  let frame = 0;
  let rafId = 0;
  const tick = () => {
    frame += 1;
    if (frame < maxCount) {
      rafId = requestAnimationFrame(tick);
    }
  };
  rafId = requestAnimationFrame(tick);
  const defer = n => frame >= n;
  defer.cancel = () => {
    if (rafId) cancelAnimationFrame(rafId);
  };
  return defer;
}
