import json5 from 'json5';

/**
 * Create service config by current env
 *
 * @param env The current env
 */
export function createServiceConfig(env: Env.ImportMeta) {
  const { VITE_SERVICE_BASE_URL, VITE_OTHER_SERVICE_BASE_URL } = env;

  let other = {} as Record<App.Service.OtherBaseURLKey, string>;
  try {
    other = json5.parse(VITE_OTHER_SERVICE_BASE_URL);
  } catch {
    // eslint-disable-next-line no-console
    console.error('VITE_OTHER_SERVICE_BASE_URL is not a valid json5 string');
  }

  const httpConfig: App.Service.SimpleServiceConfig = {
    baseURL: VITE_SERVICE_BASE_URL,
    other
  };

  const otherHttpKeys = Object.keys(httpConfig.other) as App.Service.OtherBaseURLKey[];

  const otherConfig: App.Service.OtherServiceConfigItem[] = otherHttpKeys.map(key => {
    return {
      key,
      baseURL: httpConfig.other[key],
      proxyPattern: createProxyPattern(key)
    };
  });

  const config: App.Service.ServiceConfig = {
    baseURL: httpConfig.baseURL,
    proxyPattern: createProxyPattern(),
    other: otherConfig
  };

  return config;
}

function normalizeServiceBaseUrl(rawBaseUrl: string) {
  if (!rawBaseUrl) return rawBaseUrl;

  // Align with zaihai deployment style: production requests use same-origin relative paths.
  if (import.meta.env.DEV) return rawBaseUrl;

  try {
    const parsed = new URL(rawBaseUrl, window.location.origin);
    if (/^https?:/i.test(parsed.protocol)) {
      const path = parsed.pathname.replace(/\/$/, '');
      return path || '/';
    }
    return rawBaseUrl;
  } catch {
    return rawBaseUrl;
  }
}

/**
 * get backend service base url
 *
 * @param env - the current env
 * @param isProxy - if use proxy
 */
export function getServiceBaseURL(env: Env.ImportMeta, isProxy: boolean) {
  const { baseURL, other } = createServiceConfig(env);

  const otherBaseURL = {} as Record<App.Service.OtherBaseURLKey, string>;

  other.forEach(item => {
    otherBaseURL[item.key] = isProxy ? item.proxyPattern : item.baseURL;
  });

  return {
    baseURL: isProxy ? createProxyPattern() : normalizeRuntimeBaseUrl(baseURL),
    otherBaseURL
  };
}

/**
 * Get proxy pattern of backend service base url
 *
 * @param key If not set, will use the default key
 */
function createProxyPattern(key?: App.Service.OtherBaseURLKey) {
  if (!key) {
    return '/proxy-default';
  }

  return `/proxy-${key}`;
}

function normalizeRuntimeBaseUrl(rawBaseUrl: string) {
  if (!rawBaseUrl) return rawBaseUrl;

  // Match zaihai behavior in deployment: browser uses same-origin relative path.
  if (import.meta.env.DEV) return rawBaseUrl;

  try {
    const parsed = new URL(rawBaseUrl);
    const path = parsed.pathname.replace(/\/$/, '');
    return path || '/';
  } catch {
    return rawBaseUrl;
  }
}
