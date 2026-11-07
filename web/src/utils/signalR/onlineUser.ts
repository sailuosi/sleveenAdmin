import * as SignalR from '@microsoft/signalr';
import { fetchAppRelease } from '@/service/api';
import { checkAppReleaseFromApi } from '@/utils/appReleaseCheck';
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
import { getToken } from '@/utils/auth.js';

let connection: SignalR.HubConnection | null = null;
let starting = false;
let visibilityBound = false;
let retryTimer: ReturnType<typeof setTimeout> | undefined;

function isPrivateHost(hostname: string) {
  return (
    /^10\./.test(hostname) ||
    /^192\.168\./.test(hostname) ||
    /^172\.(1[6-9]|2\d|3[0-1])\./.test(hostname) ||
    hostname === 'localhost' ||
    hostname === '127.0.0.1'
  );
}

/**
 * Resolve SignalR hub HTTP base (no trailing slash).
 * Align with requestAPI6: in prod, rewrite private-host API URL to same-origin path
 * so browser can reach Hub through IIS/gateway instead of dead internal IP.
 */
function resolveHubBaseUrl(): string {
  const raw =
    import.meta.env.VITE_WebSocket_URL ||
    import.meta.env.VITE_SERVICE_BASE_URL ||
    '';

  if (!raw) {
    return typeof window !== 'undefined' ? window.location.origin : '';
  }

  try {
    const parsed = new URL(raw, typeof window !== 'undefined' ? window.location.origin : undefined);
    const pathPrefix = parsed.pathname.replace(/\/$/, '');

    // Prod + private IP in env → same-origin (e.g. http://gateway/MineOneAPI)
    if (!import.meta.env.DEV && typeof window !== 'undefined' && isPrivateHost(parsed.hostname)) {
      return `${window.location.origin}${pathPrefix}`;
    }

    // HTTPS page + http hub → avoid mixed content
    if (
      typeof window !== 'undefined' &&
      window.location.protocol === 'https:' &&
      parsed.protocol === 'http:'
    ) {
      return `${window.location.origin}${pathPrefix}`;
    }

    return `${parsed.origin}${pathPrefix}`.replace(/\/$/, '');
  } catch {
    return String(raw).replace(/\/$/, '');
  }
}

function resolveAccessToken(): string {
  const raw = getToken() || '';
  return String(raw).replace(/^Bearer\s+/i, '').trim();
}

function triggerVersionCheck() {
  return checkAppReleaseFromApi(() => fetchAppRelease());
}

function bindVisibilityCheck() {
  if (visibilityBound || typeof document === 'undefined') return;
  visibilityBound = true;
  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
      void triggerVersionCheck();
      // Tab 回到前台时尝试恢复 Hub
      void ensureOnlineUserHub();
    }
  });
}

function scheduleRetry() {
  if (retryTimer) clearTimeout(retryTimer);
  retryTimer = setTimeout(() => {
    void ensureOnlineUserHub();
  }, 5000);
}

function ensureConnection(): SignalR.HubConnection | null {
  if (connection) return connection;

  const hubUrl = `${resolveHubBaseUrl()}/hubs/onlineUser`;
  // eslint-disable-next-line no-console
  console.info('[signalR] hub url:', hubUrl);

  connection = new SignalR.HubConnectionBuilder()
    .configureLogging(SignalR.LogLevel.Information)
    .withUrl(hubUrl, {
      accessTokenFactory: () => resolveAccessToken(),
      // 优先 WebSockets，失败再回落（IIS 未开 WS 时至少能 negotiate 暴露错误）
      transport: SignalR.HttpTransportType.WebSockets | SignalR.HttpTransportType.ServerSentEvents | SignalR.HttpTransportType.LongPolling
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: () => 5000
    })
    .build();

  connection.keepAliveIntervalInMilliseconds = 15 * 1000;
  connection.serverTimeoutInMilliseconds = 60 * 1000;

  connection.onreconnecting(() => {
    ElNotification({
      title: '提示',
      message: '服务器已断线...',
      type: 'error',
      position: 'bottom-right'
    });
  });

  connection.onreconnected(() => {
    void triggerVersionCheck();
  });

  connection.onclose(() => {
    starting = false;
    if (resolveAccessToken()) {
      scheduleRetry();
    }
  });

  connection.on('OnlineUserList', () => {});

  return connection;
}

/** Start onlineUser hub after login. Triggers anonymous appRelease check on first connect. */
export async function ensureOnlineUserHub(): Promise<void> {
  bindVisibilityCheck();

  if (!resolveAccessToken()) return;

  const conn = ensureConnection();
  if (!conn) return;

  if (conn.state === SignalR.HubConnectionState.Connected) {
    return;
  }

  if (
    conn.state === SignalR.HubConnectionState.Connecting ||
    conn.state === SignalR.HubConnectionState.Reconnecting ||
    starting
  ) {
    return;
  }

  starting = true;
  try {
    await conn.start();
    // eslint-disable-next-line no-console
    console.info('[signalR] onlineUser connected');
    await triggerVersionCheck();
  } catch (err) {
    console.warn('[signalR] onlineUser connect failed:', err);
    // 暂时屏蔽前端提示，改为控制台输出（与 appReleaseCheck ENABLE_UI_TIP 同步联调）
    // eslint-disable-next-line no-console
    console.warn(
      '[signalR] 实时连接失败：WebSocket/SignalR 未连通，版本检查将在切回页面时重试。请确认 IIS 已启用 WebSocket，且 VITE_WebSocket_URL 可达。'
    );
    scheduleRetry();
  } finally {
    starting = false;
  }
}

export async function stopOnlineUserHub(): Promise<void> {
  if (retryTimer) {
    clearTimeout(retryTimer);
    retryTimer = undefined;
  }
  if (!connection) return;
  try {
    await connection.stop();
  } catch {
    /* ignore */
  }
  starting = false;
}

export function getOnlineUserHub(): SignalR.HubConnection | null {
  return connection;
}
