import { ElMessage } from 'element-plus';

export type AppReleasePayload = {
  Version?: string;
  version?: string;
  AppVersion?: string;
  appVersion?: string;
  UpdateNotes?: string;
  updateNotes?: string;
  UpdateTip?: string;
  updateTip?: string;
};

const RELOAD_FLAG_KEY = 'appReleaseReloadedFor';
const COUNTDOWN_SECONDS = 5;

/** 暂时关闭前端 ElMessage / 自动刷新，仅控制台输出，方便联调。恢复时改为 true。 */
const ENABLE_UI_TIP = false;

let isPromptShowing = false;
let reloadTimer: ReturnType<typeof setTimeout> | undefined;

export function getLocalAppVersion(): string {
  return String(import.meta.env.VITE_APP_VERSION || '').trim();
}

function resolveServerVersion(release: AppReleasePayload | null | undefined): string {
  if (!release) return '';
  return String(release.appVersion || release.AppVersion || release.version || release.Version || '').trim();
}

function showCornerMessage(message: string, duration = 0) {
  if (!ENABLE_UI_TIP) {
    // eslint-disable-next-line no-console
    console.info('[appRelease] tip (UI disabled):', message);
    return;
  }
  ElMessage({
    message,
    type: 'warning',
    placement: 'bottom-right',
    duration,
    showClose: true,
    offset: 24
  });
}

function hardReload() {
  // 强制从服务器重新拉取（部分浏览器仍支持 forceGet；不支持时等同 reload()）
  location.reload(true);
}

/**
 * Compare server release with local VITE_APP_VERSION.
 * On mismatch: soft ElMessage tip (bottom-right) and auto-reload after 5s.
 * （ENABLE_UI_TIP=false 时仅 console，不弹窗、不自动刷新）
 */
export function applyServerRelease(release: AppReleasePayload | null | undefined): void {
  const local = getLocalAppVersion();
  const server = resolveServerVersion(release);
  if (!server || !local || server === local) return;
  if (isPromptShowing) return;

  const tip = String(release?.updateTip || release?.UpdateTip || '系统已更新，即将自动刷新以加载新版本').trim();
  const notes = String(release?.updateNotes || release?.UpdateNotes || '').trim();
  const alreadyReloaded = sessionStorage.getItem(RELOAD_FLAG_KEY) === server;

  isPromptShowing = true;

  // eslint-disable-next-line no-console
  console.info('[appRelease] version mismatch', { local, server, tip, notes, alreadyReloaded });

  if (alreadyReloaded) {
    const stuck = [tip, notes, '当前页面版本仍不一致，请确认前端是否已部署。'].filter(Boolean).join(' ');
    showCornerMessage(stuck, 8000);
    isPromptShowing = false;
    return;
  }

  const text = [tip, notes, `${COUNTDOWN_SECONDS} 秒后自动刷新`].filter(Boolean).join(' · ');
  showCornerMessage(text, (COUNTDOWN_SECONDS + 1) * 1000);

  if (!ENABLE_UI_TIP) {
    // eslint-disable-next-line no-console
    console.info(`[appRelease] would auto-reload in ${COUNTDOWN_SECONDS}s (skipped while UI tip disabled)`);
    isPromptShowing = false;
    return;
  }

  if (reloadTimer) clearTimeout(reloadTimer);
  reloadTimer = setTimeout(() => {
    sessionStorage.setItem(RELOAD_FLAG_KEY, server);
    hardReload();
  }, COUNTDOWN_SECONDS * 1000);
}

/** Fetch anonymous appRelease and apply (for WS / cold start). */
export async function checkAppReleaseFromApi(fetcher: () => Promise<AppReleasePayload | null | undefined>) {
  try {
    const data = await fetcher();
    applyServerRelease(data);
  } catch (error) {
    console.warn('[appRelease] check failed:', error);
  }
}
