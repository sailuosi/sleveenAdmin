import process from 'node:process';
import { URL, fileURLToPath } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import { setupVitePlugins } from './build/plugins';
import { createViteProxy, getBuildTime } from './build/config';

export default defineConfig(configEnv => {
  const viteEnv = loadEnv(configEnv.mode, process.cwd()) as unknown as Env.ImportMeta;

  const buildTime = getBuildTime();

  const enableProxy = configEnv.command === 'serve' && !configEnv.isPreview;

  return {
    base: viteEnv.VITE_BASE_URL,
    resolve: {
      alias: [
        {
          // token-simulation@0.36: import Ids from 'ids'；ids@3 无 default
          find: /^ids$/,
          replacement: fileURLToPath(
            new URL('./src/widgets/sailflow/bpmn/shims/ids-default.js', import.meta.url)
          )
        },
        { find: '~', replacement: fileURLToPath(new URL('./', import.meta.url)) },
        { find: '@', replacement: fileURLToPath(new URL('./src', import.meta.url)) },
        { find: '/@', replacement: fileURLToPath(new URL('./src', import.meta.url)) },
        {
          find: '@s',
          replacement: fileURLToPath(new URL('./src/widgets/sysext', import.meta.url))
        },
        {
          find: '@p',
          replacement: fileURLToPath(new URL('./src/widgets/portaldesign', import.meta.url))
        },
        {
          find: '@sail',
          replacement: fileURLToPath(new URL('./src/widgets/sailflow', import.meta.url))
        },
        {
          find: '@vue-bpmn/core',
          replacement: fileURLToPath(
            new URL('./src/widgets/sailflow/vue-bpmn/core/index.ts', import.meta.url)
          )
        },
        {
          find: '@vue-bpmn/vue',
          replacement: fileURLToPath(
            new URL('./src/widgets/sailflow/vue-bpmn/vue/index.ts', import.meta.url)
          )
        },
        {
          find: '@ho',
          replacement: fileURLToPath(new URL('./src/widgets/holdOrgPan', import.meta.url))
        }
      ],
      dedupe: ['vue']
    },
    css: {
      preprocessorOptions: {
        scss: {
          api: 'modern-compiler',
          additionalData: `@use "@/styles/scss/global.scss" as *;`
        }
      }
    },
    plugins: setupVitePlugins(viteEnv, buildTime, configEnv.command === 'serve'),
    define: {
      BUILD_TIME: JSON.stringify(buildTime)
    },
    server: {
      host: '0.0.0.0',
      port: 9527,
      open: true,
      proxy: createViteProxy(viteEnv, enableProxy),
      // Windows 上监视public 下大量静态文件易触发 EBUSY，静态资源无需 HMR watch
      watch: {
        ignored: ['**/public/**']
      }
    },
    preview: {
      port: 9725
    },
    build: {
      reportCompressedSize: false,
      sourcemap: viteEnv.VITE_SOURCE_MAP === 'Y',
      commonjsOptions: {
        ignoreTryCatch: false
      }
    },
    optimizeDeps: {
      include: [
        'bpmn-js',
        'bpmn-js/lib/Modeler',
        'bpmn-js-properties-panel',
        'bpmn-js-token-simulation',
        'diagram-js',
        'highlight.js',
        'steady-xml'
      ]
    }
  };
});

