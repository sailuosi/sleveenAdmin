import type { ClientRequest, IncomingMessage } from 'node:http';
import type { ProxyOptions } from 'vite';
import { bgRed, bgYellow, green, lightBlue } from 'kolorist';
import { consola } from 'consola';
import { createServiceConfig } from '../../src/utils/service';

/** 浏览器 → Vite → API 时，Kestrel 只能看到 127.0.0.1；必须把真实客户端 IP 带给后端（与 Nginx proxy_set_header 同理） */
function forwardClientIpToApi(proxyReq: ClientRequest, req: IncomingMessage) {
  const raw = req.headers['x-forwarded-for'];
  const xff = Array.isArray(raw) ? raw[0] : raw;
  const fromChain = xff ? String(xff).split(',')[0].trim() : '';
  const socketAddr = req.socket?.remoteAddress ? String(req.socket.remoteAddress) : '';
  const clientIp = fromChain || socketAddr;
  if (!clientIp) return;
  proxyReq.setHeader('X-Forwarded-For', clientIp);
  proxyReq.setHeader('X-Real-IP', clientIp);
}

/**
 * Set http proxy
 *
 * @param env - The current env
 * @param enable - If enable http proxy
 */
export function createViteProxy(env: Env.ImportMeta, enable: boolean) {
  const isEnableHttpProxy = enable && env.VITE_HTTP_PROXY === 'Y';

  if (!isEnableHttpProxy) return undefined;

  const isEnableProxyLog = env.VITE_PROXY_LOG === 'Y';

  const { baseURL, proxyPattern, other } = createServiceConfig(env);

  const proxy: Record<string, ProxyOptions> = createProxyItem({ baseURL, proxyPattern }, isEnableProxyLog);

  other.forEach(item => {
    Object.assign(proxy, createProxyItem(item, isEnableProxyLog));
  });

  // 可选：ccflow 接口代理，与 zaihai 项目一致。需在 .env.test 等中配置 VITE_CCflowTarget
  const ccflowTarget = (env as Record<string, string | undefined>).VITE_CCflowTarget;
  if (ccflowTarget) {
    proxy['/ccflowapi'] = {
      target: ccflowTarget,
      changeOrigin: true,
      configure: (_proxy, _options) => {
        _proxy.on('proxyReq', (proxyReq, req, _res) => {
          forwardClientIpToApi(proxyReq, req);
        });
      },
      rewrite: path => path.replace(/^\/ccflowapi/, '')
    };
  }

  return proxy;
}

function createProxyItem(item: App.Service.ServiceConfigItem, enableLog: boolean) {
  const proxy: Record<string, ProxyOptions> = {};

  proxy[item.proxyPattern] = {
    target: item.baseURL,
    changeOrigin: true,
    // 大批量附件上传：避免 Vite 代理默认超时导致 Network Error
    timeout: 0,
    proxyTimeout: 0,
    configure: (_proxy, options) => {
      _proxy.on('proxyReq', (proxyReq, req, _res) => {
        forwardClientIpToApi(proxyReq, req);
        if (!enableLog) return;

        const requestUrl = `${lightBlue('[proxy url]')}: ${bgYellow(` ${req.method} `)} ${green(`${item.proxyPattern}${req.url}`)}`;

        const proxyUrl = `${lightBlue('[real request url]')}: ${green(`${options.target}${req.url}`)}`;

        consola.log(`${requestUrl}\n${proxyUrl}`);
      });
      _proxy.on('error', (_err, req, _res) => {
        if (!enableLog) return;
        consola.log(bgRed(`Error: ${req.method} `), green(`${options.target}${req.url}`));
      });
    },
    rewrite: path => path.replace(new RegExp(`^${item.proxyPattern}`), '')
  };

  return proxy;
}
