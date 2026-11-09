# Vue BPMN（源码副本）

自研 BPMN 设计器源码副本，作为 Sail 流程设计器的第 2 套实现（与 bpmn-js 并行）。

| 项 | 值 |
|----|-----|
| 来源仓 | `bpmnjs`（vue-bpmn-workspace） |
| 源 commit | `7d125f5` |
| 产品包 | `core/` ← `@vue-bpmn/core`；`vue/` ← `@vue-bpmn/vue` |

## 约束

- **禁止**在本目录产品代码中依赖 `bpmn-js` / `diagram-js` / `@bpmn-io/*`
- 宿主通过 Vite 别名 `@vue-bpmn/core`、`@vue-bpmn/vue` 引用源码
- 同步更新：从源仓重新复制 `packages/core/src` 与 `packages/vue-bpmn/src`，并更新本文件 commit

## 用法

```ts
import { BpmnModeler } from '@vue-bpmn/vue';
```

## 相关

- 路由：`/sail/designer-vue`（并行第 2 实现；默认仍为 `/sail/designer` + bpmn-js）
- sf 扩展 round-trip：`pnpm exec tsx src/widgets/sailflow/vue-bpmn/scripts/check-sf-roundtrip.mjs`
- 移除 bpmn-js 门槛：[REMOVAL.md](./REMOVAL.md)
