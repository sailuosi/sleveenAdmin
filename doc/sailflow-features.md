# Sailflow 特性说明（产品面）

> 状态：**现行能力说明**（对照代码与菜单，2026-11-01）  
> 宿主：`sleveen-admin`（`web/src/widgets/sailflow` + `backend/sailflow`）  
> 引擎：独立解决方案 `sailflow`（`SailClient` / `core/pure`），**不**使用 NFlow Pure / Flowable  
> 关联：[sailflow-menu-mount.md](./sailflow-menu-mount.md)（菜单挂载）· [sailflow-nflowable-feature-replication.md](./sailflow-nflowable-feature-replication.md)（复刻规划与进度）· [sailflow-assignee-candidate-design.md](./sailflow-assignee-candidate-design.md)（待办人配置）· [biz-module-paradigm.md](./biz-module-paradigm.md) · [sailflow-host-capability-pack.md](./sailflow-host-capability-pack.md)（Host 能力包）  
> **引擎能力基线（演进对照）：** `sailflow/doc/sail/特性说明-引擎能力基线.md`  
> **待办人管道（引擎专项）：** `sailflow/doc/sail/特性说明-待办人管道.md`  
> **宿主 UseXxx 最佳实践（引擎）：** `sailflow/doc/sail/最佳实践-宿主能力包UseXxx.md`

本文供 **终端用户理解能力、业务接入、前端/Host 开发与联调** 共用。规划勾选与排期仍以复刻规划文档为准；本文描述「现在有什么、怎么用、边界在哪」。

---

## 1. 产品一句话

在 sleveen-admin 内提供完整的 **流程工作台**：设计 BPMN（含 AI / 规则 / 会签等 Sail 扩展）→ 设计动态表单 → 发布流程 → 发起与待办办理 → 实例追溯与运维（Outbox / 报表 / 分类），运行语义始终走 **Sail 引擎命令**（`Start/Run/Reject/SendBack/…`），前端 UX 对齐芋道式办理壳，但 API 为 Sail 形状。

---

## 2. 读者导读

| 角色 | 建议阅读 |
|------|----------|
| 业务 / 最终用户 | §3 能力地图、§4 菜单与典型路径、§8 操作语义 |
| 业务系统接入方 | §5 接入约定、§6 关键 API、§9 样例（请假） |
| 前端 / Host 开发 | §5–§7、§10 目录落点、§11 已知缺口 |
| 引擎 / 扩展开发 | sailflow 仓内《特性说明-引擎能力基线》《架构设计》《业务管道》《回调与事件》；本文只标 Host 边界 |

---

## 3. 能力地图与成熟度

图例：**可用** = 页面 + Host 已通，建议做浏览器 UAT；**设计器内** = 属性面板可配；**引擎有 / UI 弱** = API 或管道已有，产品页未齐；**暂缓** = 明确不做或后置。

### 3.1 工作台与运行

| 能力 | 成熟度 | 说明 |
|------|--------|------|
| 我的待办 / 已办 | 可用 | 分页列表，跳转详情带 `taskId` |
| 抄送我的 | 可用 | 产品表 `sf_process_instance_copy`；办理条可抄送 |
| 发起流程 | 可用 | 选定义 → 可选动态表单 → `StartProcess` |
| 我的流程 | 可用 | 按创建人分页实例 |
| 流程详情 / 办理台 | 可用 | `GetApprovalWorkbench` 聚合；表单 + 操作条 + 引用跳转 + 打印 |
| 任务管理 / 实例管理 | 可用 | 管理端列表壳 |
| 通过 / 驳回 | 可用 | `RunProcess` / `RejectProcess`（语义见 §8） |
| 下一跳选人 / 选支路 | 可用 | `GetNextActivityRoleUserTree`；可选 GroupId / GroupRole |
| 退回 / 撤回 / 重发 / 关闭 | 可用 | 退回依赖上一跳树；关闭偏管理 |
| 加签 | 可用 | `SignForwardProcess`，对齐会签扩展 |
| 抄送 | 可用 | Host 产品表独立通道（`sf_process_instance_copy`）；**勿**与待办人 ConfigPair / Notifier 混用 |
| 跳转 | 可用 | `JumpProcess` + `GetTaskActivityList`；详情办理条「跳转」选目标节点与办理人 |
| 修订 / 反转 | 可用（默认可关） | `ReviseProcess` / `ReverseProcess`；设计器按钮默认关闭 |
| 办理按钮设置 | 可用 | 节点 `sf:ButtonsSetting`；Workbench 下发；办理条 enable+显示名；见 [sailflow-buttons-capability.md](./sailflow-buttons-capability.md) |
| 转办 / 委派 | 暂缓 | 无独立产品语义封装；勿与芋道按钮一一对应 |
| 简易打印 | 可用 | 详情页 `window.print` + `@media print` |

### 3.2 设计与定义

| 能力 | 成熟度 | 说明 |
|------|--------|------|
| BPMN 设计器（`sf:`） | 可用 | Performers、AI、RuleTask、Variables、Multisign、Forms、通知、**办理按钮**… |
| 待办人 ConfigPair | 可用（引擎+面板） | `sf:assigneeResolve` / `sf:leaveJudge`；**策略化 Params 选择器**（侧栏按钮 + 节点弹窗共用） |
| 办理按钮设置 | 可用 | 属性面板列表 + **节点配置弹窗**卡片；对齐引擎 OA 命令码表 |
| 节点配置弹窗 | 可用 | 侧栏标题齿轮打开；`SailNodeConfigPanel`（无 Dialog）+ Dialog 壳 |
| 连接线配置弹窗 | 可用 | SequenceFlow 侧栏标题齿轮；流转条件 + GroupBehaviours |
| 各图元齿轮弹窗 | 可用 | Gateway / Service / Script / Rule / SubProcess / 事件；见 [sailflow-designer-sidebar-gap.md](./sailflow-designer-sidebar-gap.md) |
| 表单侧栏 | 可用 | Process + UserTask/StartEvent 绑 `sf:Forms`（Host 读；引擎 ConvertForms 仍空） |
| Timer 三态 | 可用 | `timeDate` / `timeDuration` / `timeCycle`；与用户任务 `sf:Boundaries` 超时分离 |
| Process 消息与信号 | 可用 | 选中流程根维护 rootElements Message/Signal |
| 设计器工具栏 | 可用 | 对齐 n_flowable：打开/下载/预览/模拟、六向对齐、缩放、撤销/恢复/重绘；token-simulation |
| 流程定义列表 | 可用 | 导入 / 模板 / 设计 / 启用停用 / 升级 / 删除 |
| 四步编辑壳 | 可用 | `/sail/process-edit`：基本信息 → 表单 → 嵌入设计器 → 更多；元数据 `sf_process_meta` |
| 流程分类 | 可用 | `sf_process_category` + AppType 绑定；拖拽树未做 |
| 流程模板库 | 可用 | `/sail/templates` + `CreateProcessByTemplate` |
| 简易流程设计器 | 暂缓 | 无无损 Sail XML 映射前不进菜单 |
| 芋道 BPMN 面板整体替换 | **禁止** | 保留现有 `widgets/sailflow` 设计器 |

### 3.3 表单

| 能力 | 成熟度 | 说明 |
|------|--------|------|
| 表单列表 CRUD | 可用 | `WfForm` / `FbForm` |
| form-create 设计器 | 可用 | `/sail/form-editor`；`installSailflow` 注册 |
| 发起 / 办理嵌入运行 | 可用 | `SailFormRender`；`AppInstanceCode` 可绑 formId |
| 字段活动权限 / 编辑性 | 可用 | §C：`FieldModes` 四码；NodePolicy 优先；见 [sailflow-field-modes.md](./sailflow-field-modes.md) |
| CUSTOM 业务页 / FormBiz | 可用 | §H：`ISailFormBizHandler` + `WfBizForm`；见 [sailflow-form-biz-handler.md](./sailflow-form-biz-handler.md) |

### 3.4 Sail 特色（相对 n_flowable）

| 能力 | 成熟度 | 说明 |
|------|--------|------|
| 规则集 | 可用 | 菜单 + 设计器绑定；办理详情可跳转 `?code=` |
| AI 模型 / AI 活动 | 可用 | `/sail/ai-models`；办理详情 `?configUUID=` |
| 知识库 | 可用 | 增删列表 + 向量检索 |
| Hook / Gate / Outbox | 可用 | 死信台查询与重试；管道细节见引擎文档 |
| AxConfig / 变量表 / 通知等 | 设计器内 | 随 BPMN 扩展配置 |
| 流程报表 | 可用 | `WfReport/Overview` 概览 |
| 请假样例 | 可用 | `WfLeave` + Status Hook 回写，不引 NFlow EventBus |

---

## 4. 信息架构（菜单与路由）

侧栏建议树与录入字段见 [sailflow-menu-mount.md](./sailflow-menu-mount.md)。摘要：

```text
流程引擎
├── 我的待办 / 已办 / 抄送我的
├── 发起流程 / 我的流程
├── 任务管理 / 流程实例管理
├── 流程定义 / 流程表单
├── 规则集 / AI 模型 / 知识库
├── 回调死信 / 流程模板库 / 请假样例
├── 流程分类 / 流程报表
└──（隐藏）流程设计器 / 表单设计器 / 流程详情 / 流程编辑
```

| 场景 | 路径 | 备注 |
|------|------|------|
| 设计器 | `/sail/designer?processId=&version=` | blank 全屏 |
| 办理详情 | `/sail/process-detail?taskId=&processInstanceId=` | 固定路由名便于 `push` |
| 四步编辑 | `/sail/process-edit` | 从定义列表「修改/新建」进入 |
| 表单设计 | `/sail/form-editor` | 从流程表单进入 |

联调：未挂菜单时可用 `builtin` 同 path 直接打开；挂菜单后注意同 path 双注册覆盖问题（见菜单文档）。

---

## 5. 架构边界与硬约束

```text
UX / 页面          ← widgets/sailflow（可对齐 n_flowable 信息架构）
Host API           ← backend/sailflow（ResponseResult / FlowCommandContext）
Engine / 存储      ← sailflow（语义映射，禁止换引擎）
组织 / 登录        ← sleveen 宿主（adapters；勿引入芋道系统域）
```

| ID | 约束 |
|----|------|
| C1 | **不得**用 `mooSQL.NFlow.Pure` / Activiti 形状引擎替换 `sailflow.engine` |
| C2 | Host 只做 HTTP 适配与产品表；重查询优先 `sailflow.biz.query` |
| C3 | 前端契约为 Sail（`requestAPI6` + `ResponseResult`），**不**把 `/bpm/*` 合同塞进引擎 |
| C4 | 设计器双轨：保留 `sf:` BPMN；Simple 设计器仅可选且暂缓 |
| C5 | 规则集 / AI / 知识库 / Outbox 等特色菜单不得因「复刻」删除 |
| C6 | 选人/部门/角色走 sleveen API（`adapters/`） |

依赖方向（引擎仓）：Host → biz → ext → pure → mooSQL。新增业务样例若走 Biz 栈，遵循 [biz-module-paradigm.md](./biz-module-paradigm.md)；流程运行时页可保留 widgets/sailflow 专有结构。

---

## 6. API 速查（Host）

统一前缀 `/api/...`；成功体为 `ResponseResult`（前端 `sailResult` 解包）。

### 6.1 运行与办理 — `Wf2Xml`

| 意图 | 方法 |
|------|------|
| 发起 / 通过 / 加签 / 跳转 / 修订 | `StartProcess` `RunProcess` `SignForwardProcess` `JumpProcess`（候选 `GetTaskActivityList`）`ReviseProcess` |
| 退回 / 撤回 / 重发 / 驳回 / 关闭 / 反转 | `SendBackProcess` `WithdrawProcess` `ResendProcess` `RejectProcess` `CloseProcess` `ReverseProcess` |
| 待办 / 已办分页 | `QueryReadyTasks` `QueryCompletedTasks` |
| 任务视图 / 审批台 | `GetTaskView/{id}` `GetApprovalWorkbench` |
| 下一跳 / 可退回 | `GetNextActivityRoleUserTree` `GetPreviousActivityTree` |
| 实例 | `QueryProcessInstances` `GetProcessInstance/{id}` `DeleteInstance` |
| 图高亮辅助 | `QueryReadyActivityInstance` `QueryCompletedTransitionInstance` |

前端封装：`web/src/widgets/sailflow/api/runtime.ts`。

### 6.2 定义与设计 — `Wf2Xml` / `WfProcessEdit`

| 意图 | 方法 |
|------|------|
| 列表 / 文件读写 / 校验 | `GetProcessListSimple` `QueryProcessFile` `SaveProcessFile` `ValidateProcess` |
| 创建 / 更新 / 删除 | `CreateProcess` `UpdateProcess` `DeleteProcess*` |
| 启用停用 / 升级 | `UpdateProcessUsingState` `UpgradeProcess` |
| 模板创建 | `CreateProcessByTemplate` `InitNewBPMNFile` |
| 角色用户表单枚举 | `GetRoleAll` `GetUserAll` `GetFormAll` |
| 四步元数据 | `WfProcessEdit`（Save/Get 等） |

### 6.3 表单 — `WfForm`

`GetList` / `Get` / `Save` / `Delete` · `GetFieldPermissions` / `SaveFieldPermissions`。

### 6.4 产品能力

| 域 | Controller | 典型能力 |
|----|------------|----------|
| 抄送 | `WfCopy` | `Create` `MyPage` |
| Outbox | `WfOutbox` | `Query` · `ResetDead*` |
| 分类 | `WfCategory` | CRUD + 与 AppType 绑定 |
| 报表 | `WfReport` | `Overview` |
| 请假样例 | `WfLeave` | `Create` `Query` + Hook 回写 Status |
| 规则集 | `RuleSet` | CRUD / 执行相关 |
| AI / 知识库 | `Wf2Ai` | 模型配置、KB Save/Delete/Search |
| 变量 | `WfVariable` | 流程变量维护 |
| 上传 | `FineUpload` | 设计器附件等 |
| 探针 | `sail/Probe` | `ping` · `init-db`（建表含表单/抄送/请假/分类等） |

### 6.5 鉴权现状（接入必读）

业务 Controller 已统一 **`[Authorize]`**（JWT 登录态）；仅 `api/sail/Probe`（`ping` / `init-db`）保持匿名。服务端 `SailAuthIdentity` 用 Claims（Account / realName）**覆盖** `FlowCommandContext` 操作人；「我的」待办/已办在客户端传了 UserId/EndedUserId 时强制为当前账号（空 UserId 仍留给任务管理全量）；抄送创建人、请假发起人同理。

---

## 7. 前端模块落点

```text
web/src/widgets/sailflow/
├── install.ts                 # form-create / FcDesigner
├── adapters/                  # 当前用户等宿主桥
├── api/                       # process / runtime / form / category / outbox / …
├── bpmn/                      # 设计器 modeler + sf: providers
├── components/
│   ├── runtime/               # SailOperationBar · SailFormRender
│   └── …Dialog                # 选人、表单、规则集、模板…
└── views/
    ├── task/                  # 待办 已办 抄送 任务管理
    ├── process/               # 发起 我的 管理 详情
    ├── form/                  # 列表 + 设计器
    ├── model/form/            # 四步编辑壳
    ├── ops/                   # Outbox 分类 报表
    ├── demo/                  # 请假样例
    └── Sail*Page.vue          # 定义 / 设计器 / 规则 / AI / KB / 模板
```

安装：宿主 `main` 调用 `installSailflow(app)`。

---

## 8. 办理操作语义（用户可见文案）

Sail 与芋道 BPM 按钮同名时，**语义可能不同**，产品文案必须以 Sail 为准：

| 操作 | Sail 行为要点 | 勿误解为 |
|------|---------------|----------|
| 通过 | `RunProcess`，可带条件、下一跳活动与办理人 | — |
| 驳回 | `RejectProcess` | 芋道「驳回即删/结束整单」的默认预期 |
| 退回 | `SendBackProcess` + 选可退节点 | 与驳回不同：回到历史节点继续 |
| 撤回 / 重发 | `WithdrawProcess` / `ResendProcess` | Sail 特色路径，办理条按状态显隐 |
| 加签 | `SignForwardProcess` | 对齐 `sf:multisign` / 会签，非简单抄送 |
| 关闭 | `CloseProcess` | 管理向结束；慎用删除实例 |
| 抄送 | 写产品抄送表 | 引擎无内置抄送箱 |

意见：当前为办理条可选文本；附件可接 sleveen 统一附件范式（未强制）。

---

## 9. 典型闭环（验收路径）

1. **环境**：登录 → 必要时 `Probe/init-db` → 菜单或 builtin 打开「流程引擎」。  
2. **设计**：流程定义 → 新建/模板 → 设计器保存 BPMN →（可选）绑动态表单与字段权 → 启用。  
3. **发起**：发起流程 → 填表/变量 → `StartProcess`。  
4. **办理**：用户 A 待办可见 → 详情通过（选下一办理人）→ 用户 B 待办 → 驳回或退回或办结。  
5. **追溯**：已办 / 我的流程可查；详情引用可跳规则集或 AI；需要时打印。  
6. **特色**：规则/AI 节点随流程执行；Outbox 死信可查可重试；请假样例验证 Hook 回写。

浏览器级 UAT 仍建议按上列路径全量点验（规划标注「待浏览器 UAT」）。

---

## 10. 业务接入要点

1. **启动参数**：业务单号 → `AppInstanceId`；可选 `AppName` / `ProcessId`|`ProcessCode`+`Version`；动态表单常把 formId 放 `AppInstanceCode`（与发起页约定一致）。  
2. **条件变量**：写入 `FlowCommandContext.Conditions`（及引擎约定的业务变量），供网关与表单字段同步策略使用。  
3. **下一办理人**：`NextActivityPerformers` + 可选 `GroupId`/`GroupRole`；设计期可用 ConfigPair 解析（见待办人文档）。  
4. **状态回写**：优先 **Sync Hook / Outbox**；挂载用 **`builder.UseLeaveStatusHook()`** 一类能力包，勿在组合根手写 `useFlowHook`（见 [sailflow-host-capability-pack.md](./sailflow-host-capability-pack.md)）。  
5. **组织数据**：经 `adapters` 与现有 HR/用户 API，避免在 Host 复制一套组织服务。  
6. **表结构**：产品表（抄送、分类、请假、表单元数据）由 Probe/init-db 与 Host 维护，**不**改 pure 调度内核。  
7. **新 Host 能力**：一项一个 `UseXxx`，开关/建表/Drain 内聚；组合根只链式声明（引擎文：[最佳实践-宿主能力包UseXxx.md](../../sailflow/doc/sail/最佳实践-宿主能力包UseXxx.md)）。

---

## 11. 已知缺口与后续优先项

| 项 | 状态 | 建议 |
|----|------|------|
| 全量浏览器 UAT | 未关单 | 按 §9 脚本验收后把规划「待 UAT」改为已验收 |
| Host 鉴权 | **已落地** | 业务 Controller `[Authorize]`；仅 `Probe`（ping/init-db）保持匿名 |
| 办理条跳转 | **已落地** | 详情页 `showJump`；`GetTaskActivityList` 候选 + `JumpProcess` |
| 转办 / 委派 | 未产品化 | 仅当引擎命令清晰后再封装，禁止伪造语义；**不**进按钮码表 |
| 办理按钮设置 | **已落地** | 码表含 Run/Reject/SendBack/Sign/Copy/Withdraw/Resend/Jump/Close/Revise/Reverse；见 buttons-capability |
| 分类拖拽树 | 未做 | 列表+绑定已够最小可用 |
| Simple 设计器 | 暂缓 | 先做 XML 映射 POC，失败则放弃主路径 |
| 用户分组 / 监听器 / 表达式管理页 | 未做 | 仅当运行时确有消费点再开 |
| 办理附件 | 意见已有 | 接宿主附件范式 |
| 真并行会签债 | 引擎演进 | 走 Sail Multisign，不照搬 NFlow MI |
| Unit\* / StartUnit\* 落库 | **已落地** R1 | 任务 UnitId·Code·Name；实例 StartUnit\*；与 Group\* 正交 |
| Assignee 双轨收口 | **已落地** R2 | 挂管道后不回落 Definition/旧会签；Legacy 不再推断 RoleGroup |
| 缺配置 ReasonCode + FE | **已落地** R4 | `WfExecutedResult.ReasonCode` + API ExtraData + 办理条提示 |
| 设计默认 + 发布校验 | **已落地** R6 | 预埋 ApproverPick+Collaborate；校验码 5；**保存前强制校验** |
| 组间 Count/Rate | **已落地** R7 | `groupCompleteCount` / `groupCompleteRate`；默认 And |
| 队长细则 | **已落地** R8 | Captain + gateRoles；组员可办、关卡办完组完成 |
| 同码覆盖诊断 | **已落地** R9 | 注册覆盖日志 + IsOverridden 运行诊断 |
| Org/User 覆盖 Lookup | **已落地** R3 | UnitMap→SysOrg/SysUser；RoleGroup→AuthVisitorBag/AuthDutyItem（无 SysRole 表）；JobRelative→maps 或访客袋 Code 占位；身份键 Account |

待办人管道 S0–S4 已落地；**R1–R9** 见 [sailflow-assignee-candidate-design.md](./sailflow-assignee-candidate-design.md) §7.5 与引擎 `特性说明-待办人管道.md` §0。

**三线边界：** 待办人 ConfigPair（谁来办）≠ Task Notifier（推送，属回调与事件）≠ 抄送（Host 独立表/事件）。

---

## 12. 文档与代码索引

| 用途 | 位置 |
|------|------|
| 菜单录入 | [sailflow-menu-mount.md](./sailflow-menu-mount.md) |
| 复刻范围与阶段勾选 | [sailflow-nflowable-feature-replication.md](./sailflow-nflowable-feature-replication.md) |
| 待办人 / 候选人 | [sailflow-assignee-candidate-design.md](./sailflow-assignee-candidate-design.md) |
| 办理按钮管理与能力对齐 | [sailflow-buttons-capability.md](./sailflow-buttons-capability.md) |
| 前端包 | `web/src/widgets/sailflow/**` |
| Host | `backend/sailflow/**` |
| Host 能力包写法 | [sailflow-host-capability-pack.md](./sailflow-host-capability-pack.md) |
| 引擎能力基线 | `sailflow/doc/sail/特性说明-引擎能力基线.md` |
| 引擎宿主 UseXxx 实践 | `sailflow/doc/sail/最佳实践-宿主能力包UseXxx.md` |
| 引擎分层 / Hook | `sailflow/doc/sail/架构设计-项目依赖与分层.md` 等 |
| 对照 UX 源 | 参照产品前端 widgets（只参考页面，不迁引擎） |

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-11-01 | 初版：复审复刻规划与现状代码后编制特性说明（用户 / 接入 / 开发共用） |
| 2026-11-01 | 对齐待办人 **R1–R5**：缺口表增双轨收口/缺配置拒绝/Org 接入；明确抄送与 Assignee/Notifier 分离 |
| 2026-11-01 | **修订 R1**：缺口增 Unit\*/StartUnit\*；文案改为 Group\*≠Unit\* |
| 2026-11-01 | **R6**：设计默认 ApproverPick+Collaborate；发布前非空 |
| 2026-11-01 | **R3 补**：AssignedUserId = 用户编码（如登录账号） |
| 2026-11-01 | **R7**：组间完成 And/Or + Count/Rate |
| 2026-11-01 | **R8**：队长关卡必办、组员可办、关卡办完可发送 |
| 2026-11-01 | **R9**：同码覆盖诊断强制 |
| 2026-11-01 | **办理按钮设置**：sf:ButtonsSetting + Workbench + 码表对齐引擎 OA 命令 |
| 2026-11-01 | R1–R9 主干落地；缺口表改为已落地；保存前校验；Account 选人 |
| 2026-11-01 | Host 能力包：`UseDebugConsoleHook` / `UseLeaveStatusHook` / `UsePortalScheduleTaskNotifier`；链到最佳实践文档 |
