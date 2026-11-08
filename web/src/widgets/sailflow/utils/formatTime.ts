/** Format API datetime (e.g. .NET ISO) to `YYYY-MM-DD HH:mm:ss`. */
export function formatTime(raw: unknown): string {
  const s = String(raw || '').trim();
  if (!s) return '-';
  return s.replace('T', ' ').replace(/\.\d+/, '').slice(0, 19);
}
