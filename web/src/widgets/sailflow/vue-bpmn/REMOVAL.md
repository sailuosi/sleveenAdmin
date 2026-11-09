# 移除 bpmn-js 门槛（并行期结束后）

Vue 设计器路径稳定、入口全部切到默认 `/sail/designer-vue`（或改回 `/sail/designer` 指向 Vue）后，再执行：

1. 删除 `web/src/widgets/sailflow/bpmn/createModeler.ts` 及 Sf* PropertiesProvider、token-simulation 相关
2. 删除或改写仅服务 bpmn-js 的 `assigneeDefaults` / `eventNameDefaults` / `installNodeConfigHeaderGear`（保留 Vue 版 `designer/vueDefaults.ts`）
3. 从 `web/package.json` 移除：`bpmn-js`、`bpmn-js-properties-panel`、`@bpmn-io/properties-panel`、`bpmn-js-token-simulation`、`bpmn-js-i18n-zh`、`diagram-js`、`camunda-bpmn-moddle`、`min-dash` 等仅旧设计器依赖
4. 删除 `vite.config.ts` 中 `ids` shim
5. 删除 `/sail/designer` 旧页或改为重定向到 Vue 页
6. 产品包依赖扫描：`vue-bpmn/` 与 `designer/` 无 bpmn-js import

## 并行期验收

- [ ] `/sail/designer-vue` 新建 → 保存 → 校验
- [ ] 打开含 `sf:` 的流程，编辑拓扑后保存扩展不丢（见 `scripts/check-sf-roundtrip.mjs`）
- [ ] UserTask「完整配置」对话框可读写并落库
- [ ] SequenceFlow「完整配置」可读写
- [ ] ProcessDesign「用 Vue 设计器打开」可用；iframe 默认仍为 bpmn-js
- [ ] Vue 路径无新增 bpmn-js import（旧路径可保留）
