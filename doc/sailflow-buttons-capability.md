# Sail 办理按钮管理与能力对齐

> 对照 n_flowable / 芋道「操作按钮设置」，在 Sail 侧用 **引擎真实命令** 建码表，设计器可按节点启停/改名，办理条按配置 + 运行态上下文显隐。  
> 日期：2026-11-01

---

## 1. 目标

| 目标 | 说明 |
|------|------|
| 能力对齐 | 按钮一一对应 `IWorkflowService` / Host 产品 API，禁止空壳按钮 |
| 节点可配 | BPMN `sf:ButtonsSetting`：enable + displayName |
| 办理消费 | `GetApprovalWorkbench.ButtonsSetting` → `SailOperationBar` |
| 与芋道差异 | 不照搬转办/委派语义；Sail 特有撤回/重发/跳转/修订/反转进码表 |

---

## 2. 按钮码表 ↔ 引擎 / Host

| Id | 默认名 | 默认启用 | 引擎 / Host | 办理上下文 | n_flowable 对照 |
|----|--------|----------|-------------|------------|-----------------|
| `Approve` | 通过 | 是 | `RunProcess` | 待办办理 | 通过 |
| `Reject` | 驳回 | 是 | `RejectProcess` | 待办办理 | 拒绝（语义不同） |
| `SendBack` | 退回 | 是 | `SendBackProcess` | 待办办理 | 退回 |
| `SignForward` | 加签 | 是 | `SignForwardProcess` | 待办办理 | 加签 |
| `Transfer` | 转办 | 是 | `TransferProcess` | 待办办理 | 转办（本节点交人、不流转） |
| `Copy` | 抄送 | 是 | `WfCopy/Create`（产品表） | 待办办理 | 抄送 |
| `Withdraw` | 撤回 | 是 | `WithdrawProcess` | 已办 | 撤回（部分） |
| `Resend` | 重发 | 是 | `ResendProcess` | 已办 | Sail 特色 |
| `Jump` | 跳转 | 是 | `JumpProcess` + `GetTaskActivityList` | 管理 | Sail 管理 |
| `Close` | 关闭 | 是 | `CloseProcess` | 管理 | 取消/关闭类 |
| `Terminate` | 终止 | **否** | `TerminateProcess` | 管理 | §B；≠ Close/Discard |
| `Discard` | 作废 | **否** | `DiscardProcess` | 管理 | §B；≠ Close/Terminate |
| `Revise` | 修订 | **否** | `ReviseProcess` | 待办/管理 | 无直接对应 |
| `Reverse` | 反转 | **否** | `ReverseProcess` | 管理 | ≠ 退回，慎用 |

**未纳入码表 / 演进中：**

| 能力 | 状态 | 说明 |
|------|------|------|
| 转办 `Transfer` | **已入码表** | `TransferProcess` + Host API + `ButtonsOperation` Handler；语义：个人办结 + 本节点交人、不流转；≠ 委派 |
| 委派 `Delegate` | **暂缓** | 办完可回原人；与转办区分；禁止空壳 |
| 取消 / 挂起 | **暂缓** | `CancelProcess` / `SuspendProcess`；本轮未进码表 |
| 发起 | 不进办理条 | 属发起页 |
| `sf:Actions` | 另轨 | BPMN 生命周期动作，**不是** UX 操作按钮 |

前端权威列表：`web/src/widgets/sailflow/bpmn/buttonsCatalog.ts`  
后端权威列表：`backend/saiflow/Utility/SailButtonsCatalog.cs`

---

## 3. 设计器落盘

- 扩展类型：`sf:ButtonsSetting`，属性 `json`（数组字符串）
- 面板名：**办理按钮设置**（UserTask / Task）
- UI：对齐 n_flowable **列表三列**——操作按钮（码表名）| 显示名称（可点改）| 启用（开关）；按 **待办办理 / 其他（已办·管理）** 分组
- **节点配置弹窗**：属性面板标题栏右侧**齿轮**打开 → `SailNodeConfigDialog`（壳）+ `SailNodeConfigPanel`（内容无 Dialog）；卡片分组覆盖待办人/办理模式/会签/参与者/表单/按钮/拒绝·空人·同人·签名·超时·字段权限/通知/动作；产品策略落 `sf:NodePolicy.json`
- **连接线配置弹窗**：选中 SequenceFlow 后同一位置齿轮打开 → `SailTransitionConfigDialog` + `SailTransitionConfigPanel`；卡片：基本信息 / 流转条件（普通·默认·条件） / 转移行为（接收者、审批/优先级/默认分支/强制合并按网关上下文显示）
- **各图元齿轮 / 侧栏补齐**：见 [sailflow-designer-sidebar-gap.md](./sailflow-designer-sidebar-gap.md)（策略 Params 选择器、Forms 扩 StartEvent、Timer 三态、消息信号、Gateway/Service/Script/Rule/SubProcess/Event 弹窗）
- 新增节点：与 Assignee 一并预埋默认 JSON（`assigneeDefaults.ts`）

示例：

```xml
<bpmn:userTask id="Activity_Approve" name="审批">
  <bpmn:extensionElements>
    <sf:ButtonsSetting json='[{"id":"Approve","enable":true,"displayName":"同意"},{"id":"Reject","enable":false,"displayName":"驳回"},...]' />
  </bpmn:extensionElements>
</bpmn:userTask>
```

未配置节点：运行时按码表默认合并（缺项补齐，不因旧图缺字段崩溃）。

---

## 4. 运行时规则

显隐 = **运行态上下文** ∧ **节点 enable**：

| 上下文 | 条件（产品） | 受控按钮 |
|--------|--------------|----------|
| handle | 当前用户待办且活动 Ready/Running | Approve Reject SendBack SignForward Transfer Copy Revise；有 BackSrc 时另露 Resend（返送退回人） |
| done | 本人已办且流程仍运行 | Withdraw；有 BackSrc 时可 Resend |
| admin | 流程运行中且有任务上下文 | Jump Close Reverse |

文案：优先 `displayName`，否则码表默认名；有 `HasBackSrc` 时办理台可将 Approve→「正常发送」、Resend→「返送退回人」。

聚合接口：`POST api/Wf2Xml/GetApprovalWorkbench` → `ButtonsSetting[]` + `HasBackSrc`。  
统一办理：`POST api/Wf2Xml/ButtonsOperation`（码表 Id → Handler；Copy 仍走产品表）。

---

## 5. 与 n_flowable 差异摘要

| 项 | n_flowable | Sail |
|----|------------|------|
| 存储 | 多条 `ButtonsSetting` 扩展元素 | 单条 `sf:ButtonsSetting.json` |
| 按钮集 | 通过/拒绝/转办/委派/加签/退回 | Sail 全量 OA 命令集（上表） |
| 转办/委派 | 有 | 转办 **须有**（todo §F）；委派暂缓；禁止无引擎命令的空壳 |
| 显示控制 | 仅看 enable | enable + 办理态上下文 |
| 生命周期 Action | 另轨 | `sf:Actions` 另面板，勿混 |

---

## 6. 验收清单

1. 设计器选 UserTask → 可见「办理按钮设置」；关掉「驳回」→ 保存 XML 含 `Reject.enable:false`
2. 发布后待办打开详情 → 无驳回按钮；通过仍可用
3. 改显示名「通过」→「同意」→ 办理条文案为「同意」
4. 开启「修订」→ 待办条出现修订，调用 `ReviseProcess`
5. 开启「反转」→ 管理上下文可见，调用 `ReverseProcess`
6. 旧流程无 `ButtonsSetting` → 仍显示默认按钮集

---

## 7. 代码索引

| 路径 | 职责 |
|------|------|
| `web/.../bpmn/buttonsCatalog.ts` | 码表 / merge / 显隐工具 |
| `web/.../bpmn/descriptors/buttons.json` | Moddle |
| `web/.../bpmn/provider/sfButtonsProvider.ts` | 属性面板 |
| `web/.../components/runtime/SailOperationBar.vue` | 办理条 |
| `web/.../views/process/SailProcessDetailPage.vue` | 下发 props |
| `backend/saiflow/Utility/SailButtonsCatalog.cs` | 解析 + 默认 |
| `backend/saiflow/Dto/ApprovalWorkbenchDto.cs` | `ButtonsSetting` |
| `doc/sailflow-features.md` | 产品成熟度表 |

---

## 8. 修订记录

| 日期 | 说明 |
|------|------|
| 2026-11-01 | 初版：码表、设计器、Workbench、办理条、Reverse API、本文档 |
