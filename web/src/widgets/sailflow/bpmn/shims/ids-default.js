/**
 * bpmn-js-token-simulation@0.36 使用 `import Ids from 'ids'`，
 * 而 ids@3 仅提供 named export `{ Ids }`。此 shim 补默认导出。
 * 使用相对路径直达 dist，避开 package exports 限制。
 */
export { Ids as default, Ids } from '../../../../../node_modules/ids/dist/index.js';
