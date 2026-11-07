# Sailflow ← n_flowable 重大功能复刻规划

> 状态：规划稿（2026-10-31）；S0–S6 代码齐套，**待浏览器 UAT**  
> 对照源：`bee-bpmn/nflow/n_flowable`（前端产品面）  
> 落地宿主：`sleveen-admin`（`web/src/widgets/sailflow` + `backend/saiflow`）  
> 引擎约束：**不得更换** `sailflow` 核心引擎（`core/pure` / SailClient）；仅做 Host 适配与前端复刻  
> **现行能力说明（用户/接入/开发）：** [sailflow-features.md](./sailflow-features.md)  
> 关联：[sailflow-menu-mount.md](./sailflow-menu-mount.md)、[biz-module-paradigm.md](./biz-module-paradigm.md)、[sailflow-assignee-candidate-design.md](./sailflow-assignee-candidate-design.md)（待办人配置）、sailflow《架构设计-项目依赖与分层》《特性设计-回调与事件》

---

## 0. 一句话目标

把 **n_flowable 已成型的「待办办理 + 发起/详情 + 表单设计/运行 + 流程管理壳」** 尽可能完整复刻到 sleveen-admin 的 Sailflow 前端产品面，**保留并强化** 当前 Sailflow 特色（BPMN `sf:` 设计器、规则集、AI 活动、知识库、Hook/Outbox），**后端仍走 Sailflow 引擎与现有 Host 适配层**，禁止引入 NFlow Pure / Flowable 引擎替换。

---

## 1. 硬约束与原则

| # | 约束 | 说明 |
|---|------|------|
| C1 | **引擎不换** | 禁止把 `mooSQL.NFlow.Pure` / Activiti 形状引擎迁入替换 `sailflow.engine`；运行语义继续是 Slickflow/Sail 系：`Start/Run/Reject/SendBack/Withdraw/Jump/SignForward…` |
| C2 | **Host 可加厚** | `backend/saiflow` 只做 HTTP 适配：补齐分页待办、任务视图、下一跳树、表单 CRUD/字段权、实例详情、Outbox 管理等；业务查询优先落 `sailflow.biz.query` |
| C3 | **前端可整包复刻 UX** | 交互与信息架构对齐 n_flowable（芋道 BPM 壳），但 API 契约改为 Sail 形状（`ResponseResult` / `FlowCommandContext` / `TaskViewEntity`），**不**照搬 `/bpm/*` 到引擎内 |
| C4 | **设计器双轨** | **保留**现有 `widgets/sailflow` BPMN 设计器（Performers / AI / RuleTask / Variables / Multisign…）；n_flowable 的 Simple 设计器与表单设计器按需引入；**不**用芋道 BPMN 面板整体替换 Sail 设计器 |
| C5 | **特色必保** | 规则集、AI 模型、知识库、AxConfig、Hook/Gate/Outbox、模板库、变量表——菜单与能力不得因复刻被删掉 |
| C6 | **组织/权限走宿主** | 选人/选部门/角色对齐 sleveen 现有 HR/用户 API（对标 n_flowable `adapters/`），不引入芋道系统域 |

### 复刻策略（三层）

```text
UX / 页面信息架构     ← 尽可能完整复刻 n_flowable
Host API 形状         ← 适配 Sail 引擎能力（可增端点，可写 Facade DTO）
Engine / 存储语义     ← 保持 sailflow；语义映射而非协议克隆
```

---

## 2. 现状对照总览

### 2.1 成熟度快照

| 能力域 | n_flowable | 当前 Sailflow@sleveen | 差距性质 |
|--------|------------|------------------------|----------|
| BPMN 设计（专业画布） | bpmn-js + 芋道属性面板 | **已有且更深**（`sf:` AI/规则/会签/通知…） | 保留增强，非替换 |
| 简易流程设计 | SimpleProcessDesignerV2 | 无 | 可选复刻（P2） |
| 流程模型/定义管理 | 分类、四步向导、发布/停用 | 列表 + 设计跳转（弱） | 管理壳需补强 |
| 表单设计 | form-create designer | 仅 FormPicker 绑 ID | **重大缺口** |
| 表单运行时 | form-create + 字段权 stub | 无 | **重大缺口** |
| 待办 / 已办 / 抄送 | 完整列表页 | 无 UI；Host 仅 Top10 | **重大缺口** |
| 发起流程 | 选定义 + 填表 + 选人 | 无 | **重大缺口** |
| 流程详情 + 操作条 | 通过/驳回/退回/转办/委派/加签/抄送/撤回/打印 | 无 UI；Host 已有 Run/Reject/SendBack/… | **重大缺口**（API 大半已有） |
| 实例管理 / 报表 | 有 | 无 | 缺口 |
| 分类 / 用户组 / 监听器 / 表达式 | 有 | 无（Sail 语义不同） | 按需映射或产品表新建 |
| AI / 规则集 / 知识库 | 无（或极弱） | **已有** | **特色保留** |
| Hook / Outbox / 死信 | 无对等产品页 | 引擎已有；UI 待做 | **特色补齐** |

### 2.2 代码落点

| 侧 | n_flowable | Sailflow / sleveen |
|----|------------|--------------------|
| 前端包 | `pcweb/src/widgets/nflow` | `web/src/widgets/sailflow` |
| 页面入口 | `Page/BPO_NFlow_*` | 现 `views/Sail*Page.vue`；复刻后建议 `views/{task,process,form,model}/` + Page 壳 |
| API 客户端 | `api/*` + `bpmRequest`（`code===0`） | `api/*.ts` + `requestAPI6`（`ResponseResult`） |
| Host | `HHNY.NET.NFlow` Controllers | `backend/saiflow/Controller/*` |
| 引擎 | `mooSQL.NFlow.Pure` | **`sailflow` 独立解决方案（禁止替换）** |

### 2.3 操作语义映射（办理条）

n_flowable 按钮 → Sail Host（已有或待补）：

| UX 操作 | n_flowable API | Sail 对应 | 备注 |
|---------|----------------|-----------|------|
| 通过 / 同意 | `PUT /bpm/task/approve` | `POST Wf2Xml/RunProcess` | runner + 条件/下一跳/办理人 |
| 驳回 | `reject` | `RejectProcess` | Sail 驳回语义与芋道「删实例」不同，UI 文案按 Sail |
| 退回 | `return` + `list-by-return` | `SendBackProcess` + 待补「可退回节点」查询 | 需 Host 暴露上一跳/可退节点 |
| 转办 | `transfer` | 待补或映射 `ReviseProcess` / 设办理人 API | 以引擎现有能力为准，缺则 Host 封装 |
| 委派 | `delegate` | 待核实引擎；无则阶段后期 | 不强行伪造语义 |
| 加签 | `create-sign` | `SignForwardProcess` | 会签/加签对齐 `sf:multisign` |
| 撤回 | `withdraw` | `WithdrawProcess` | |
| 重新发送 | — | `ResendProcess` | Sail 特色保留入口 |
| 跳转 | — | `JumpProcess` | 管理员能力；进按钮码表 |
| 关闭 | — | `CloseProcess` | 进按钮码表 |
| 修订 / 反转 | — | `ReviseProcess` / `ReverseProcess` | 默认关闭；节点可开 |
| 取消实例 | `cancel-by-*` | `CloseProcess` / `DeleteInstance`（慎用） | 产品语义单独定义 |
| 抄送 | `copy` | 产品表新建（引擎无内置抄送箱） | 对齐 n_flowable 产品表思路 |
| **节点按钮设置** | `buttonsSetting` | `sf:ButtonsSetting` + Workbench | 见 [sailflow-buttons-capability.md](./sailflow-buttons-capability.md) |
| 发起 | `process-instance/create` | `StartProcess` | |
| 待办分页 | `todo-page` | 待补：`GetReadyTasks` 分页包装 | 引擎 `WorkflowQueryService` 已有 |
| 已办分页 | `done-page` | `QueryCompletedTasks` 增强 + 用户过滤 | |
| 审批详情聚合 | `get-approval-detail` | **待建 Facade**：任务视图 + 表单 conf + 字段权 + 下一跳树 + 流程图高亮 | 前端最依赖的聚合接口 |

---

## 3. 目标信息架构（菜单）

在现有「流程引擎」下扩展（兼容 [sailflow-menu-mount.md](./sailflow-menu-mount.md)）：

```text
流程引擎
├── 工作台
│   ├── 我的待办              ★ 复刻重点
│   ├── 我的已办              ★
│   ├── 抄送我的              ★（产品表）
│   └── 任务管理              （管理员）
├── 流程运行
│   ├── 发起流程              ★
│   ├── 我的流程              ★
│   ├── 流程实例管理
│   └── 流程详情              （隐藏/静态路由，从列表跳入）
├── 设计中心
│   ├── 流程定义              （现有加强：发布/版本/删除）
│   ├── 流程设计器            （现有，隐藏全屏）
│   ├── 简易流程设计          （可选 P2，移植 Simple）
│   ├── 流程表单              ★ form-create
│   └── 表单设计器            （Dialog/全屏，隐藏菜单）
├── 配置
│   ├── 流程分类              （产品表）
│   ├── 用户分组              （可选）
│   ├── 规则集                ★ 特色保留
│   ├── AI 模型               ★ 特色保留
│   ├── 知识库                ★ 特色保留并补上传
│   └── 回调/死信 Outbox      ★ 特色补齐
└── 样例
    └── 请假（或其它业务样例） （对标 OA Leave，接 Start + 状态回写）
```

路由建议前缀统一 `/sail/...`；详情页固定路由名（如 `SailProcessInstanceDetail`），便于待办 `push({ name, query })`。

---

## 4. 前端模块规划（落点）

目标目录（在现有 widget 上增量，避免推倒）：

```text
web/src/widgets/sailflow/
├── install.ts                 # 注册 form-create / FcDesigner / 权限指令桩
├── adapters/                  # 宿主桥：用户/部门/角色/Dict（对标 nflow adapters）
├── api/
│   ├── process.ts             # 已有设计态
│   ├── runtime.ts             # 新增：待办/发起/Run/Reject…
│   ├── form.ts                # 新增：表单 CRUD + 字段权
│   ├── instance.ts            # 新增：实例列表/详情 Facade
│   ├── category.ts / copy.ts / outbox.ts …
│   ├── ruleset.ts / ai.ts …   # 特色保留
├── components/
│   ├── bpmn/…                 # 现有设计器全部保留
│   ├── form/                  # form-create 封装
│   └── runtime/               # 操作条、时间线、流程图高亮只读查看器
├── views/
│   ├── task/{todo,done,copy,manager}/
│   ├── process/{create,my,manager,detail}/
│   ├── form/{index,editor}/
│   ├── model/                 # 流程定义管理加强（可仍用 SailProcessListPage 演进）
│   ├── SailDesignerPage.vue   # 保留
│   ├── SailRuleSetPage.vue    # 保留
│   ├── SailAiModelPage.vue    # 保留
│   ├── SailKnowledgeBasePage.vue
│   └── ops/OutboxDeadLetterPage.vue
└── Page/                      # 可选：与菜单扫描规则对齐的薄壳（对标 BPO_*）
```

**从 n_flowable 直接参考/移植的优先文件（改 API，不改引擎）：**

| 源（nflow） | 目标用途 |
|-------------|----------|
| `views/task/todo|done|copy|manager` | 列表壳 |
| `views/processInstance/create|detail|*` | 发起 + 详情 + 操作条 + 时间线 + 打印 |
| `views/form/**` + `install.ts` form-create | 表单设计/运行 |
| `adapters/**` | 选人桥思路（实现换 sleveen API） |
| `components/SimpleProcessDesignerV2/**` | 仅 P2 可选 |
| `components/bpmnProcessDesigner/**` | **默认不移植**（避免冲掉 `sf:` 设计器） |

---

## 5. Host / 引擎侧工作包（引擎不换）

### 5.1 已有可直接挂前端的 API

`Wf2Xml`：`StartProcess`、`RunProcess`、`RejectProcess`、`SendBackProcess`、`WithdrawProcess`、`ResendProcess`、`JumpProcess`、`GetTaskActivityList`、`ReviseProcess`、`SignForwardProcess`、`CloseProcess`、`QueryCompletedTasks`、`GetTaskToDoListTop`、`GetTaskDoneListTop`、`QueryReadyActivityInstance`、`QueryCompletedTransitionInstance`、设计态全套、`GetFormAll`。

### 5.2 必须新增的 Host 端点（调用现有 Service，不改引擎内核）

| 优先级 | 端点意图 | 建议实现来源 |
|--------|----------|--------------|
| P0 | 分页待办 / 已办（当前用户） | `WorkflowQueryService.GetReadyTasks` / `GetCompletedTasks` |
| P0 | 任务视图 `GetTaskView` | `TaskManager` / QueryService |
| P0 | 审批详情聚合 `GetApprovalWorkbench` | 组合：TaskView + ProcessInstance + FormConf + FieldACL + NextTree + ReadyActivities |
| P0 | 下一跳 / 办理人树 | `GetNextActivityTree` / `GetNextActivityRoleUserTree` |
| P0 | 可退回节点列表 | `GetPreviousActivityTree` 或 SendBack 候选 |
| P1 | 表单 CRUD（元数据 + conf JSON） | `core/ext/form` FormManager 扩展 + Controller |
| P1 | 字段活动权限读写 | `GetFieldActivityEditByForm` + Save |
| P1 | 实例分页（我的 / 管理） | biz.query 或 QueryService |
| P1 | 流程定义管理：启用状态 / 版本升级 | `UpgradeProcess` / `UpdateProcessUsingState` 等已有能力暴露 |
| P2 | 抄送箱 CRUD + 分页 | 新建产品表（对标 NFLOW copy） |
| P2 | 流程分类 | 新建产品表或复用字典 |
| P2 | Outbox 死信查询/重试/批量 | `HookOutboxManager` 已有 → Host Controller |
| P3 | 转办/委派若引擎缺口 | 仅在引擎**已有命令**上封装；禁止为了 UX 改写 pure 调度内核 |

### 5.3 明确禁止

- 替换 `SailClient` / `WorkflowService` 实现为 NFlow `IProcessEngine`
- 在 `core/pure` 引入芋道表结构或 `/bpm` 合同作为引擎契约
- 删除或旁路 AI / Rule / Form / Hook 插槽

---

## 6. 分阶段任务（建议实施顺序）

对齐 n_flowable M0–M5 节奏，但阶段编号用 **S**（Sail），避免与引擎 Phase1–3b Hook 文档混淆。

### S0 — 脚手架与边界（约 3–5 天）

**目标：** 可挂菜单的空壳 + 适配层约定 + 依赖就位。

- [ ] `installSailflow`：注册 form-create / FcDesigner（版本与 n_flowable 对齐评估）
- [ ] `adapters/`：UserSelect / DeptSelect / 当前用户；对接 sleveen 登录态
- [ ] 路由：待办/已办/发起/我的流程/详情（静态名）/表单；菜单文档更新
- [ ] `api/runtime.ts`、`api/form.ts`、`api/instance.ts` 骨架
- [ ] 文档：本文件 + 菜单补充；明确「引擎不换」检查清单进 PR 模板
- [ ] 鉴权：Sail Controller 从 `AllowAnonymous` 过渡到宿主登录（至少写操作）

**验收：** 菜单可进空白页；Probe ping 正常；设计器旧页面不回归。

---

### S1 — 办理闭环（最高优先，约 1.5–2.5 周）

**目标：** 设计好的流程能 **发起 → 待办 → 详情 → 通过/驳回 → 已办/我的流程**。

前端：

- [ ] 待办 / 已办列表（分页、关键字、跳转详情带 `taskId`）
- [ ] 发起流程：选流程定义 →（无表单时先变量/意见）→ `StartProcess`
- [ ] 流程详情页：基本信息 + 任务列表/时间线 + 只读 BPMN 高亮（`QueryReady*` / 完成变迁）
- [ ] 操作条 v1：通过（`RunProcess`）、驳回（`RejectProcess`）、意见必填
- [ ] 我的流程列表

Host：

- [ ] 分页待办/已办（用户过滤）
- [ ] `GetApprovalWorkbench`（或等价拆分接口一次到位）
- [ ] `GetNextActivityRoleUserTree` 暴露（通过时选人/选支路）

**验收脚本：** 模板流程 deploy/保存 → Start → 用户 A 待办可见 → Run 到下一节点 → 用户 B 待办 → Reject 或办结 → 已办可查。

**不对齐强求：** 加签/抄送/打印可放到 S3。

---

### S2 — 表单设计与运行（约 2 周）

**目标：** 对齐 n_flowable M4 体验：设计 conf → 绑到节点/流程 → 发起与审批时渲染。

前端：

- [ ] 表单列表 CRUD 页 + Dialog/全屏 `fc-designer`
- [ ] 设计器内 FormPicker 升级：可选「动态表单」并预览
- [ ] 发起页 / 审批操作条嵌入 `<form-create>`；提交写入 `FlowCommandContext` 条件或业务变量
- [ ] 字段只读/隐藏：按活动权限渲染

Host / ext：

- [ ] 表单实体存 conf / fieldsJson（扩展现有 Form 表或兼容列）
- [ ] CRUD API；`GetFormAll` 返回足够元数据
- [ ] 字段活动权限 API 完整读写
- [ ] Workbench 聚合返回 `formConf` + `fieldPermissions`

**验收：** NORMAL 表单发起带变量；审批节点按权限只读；CUSTOM 页路径可后置。

**特色兼容：** AI/规则节点不依赖表单；表单节点与 `sf:forms` 绑定策略写清（FormId 关联）。

---

### S3 — 办理增强（对标 M5 操作，约 2 周）

前端操作条 v2（逐项联调）：

- [ ] 退回（选目标节点）
- [ ] 撤回 / 重新发送
- [ ] 加签（`SignForwardProcess`）
- [ ] 跳转 / 关闭（管理员）
- [ ] 转办（若 Host 已封装）
- [ ] 抄送（产品表 + 抄送箱页）
- [ ] 打印模板（可先简化：详情打印；完整 WangEditor 后置）
- [ ] 任务管理 / 实例管理页

Host：

- [ ] 可退回节点、抄送表、管理端分页
- [ ] 操作审计字段（意见、操作者）与时间线 DTO

**验收：** 每个按钮有至少一条浏览器路径；失败有明确 Message。

---

### S4 — 流程定义管理壳加强（约 1 周，可与 S1 并行部分）

- [x] 流程定义列表：导入/模板/设计/删除/启用停用/版本升级（Host `UpdateProcessUsingState` / `UpgradeProcess` / `DeleteProcess*`）
- [x] 分类（产品表 + AppType 绑定；拖拽树未做）
- [x] 设计器入口参数保持 `processId+version`
- [x] 四步编辑壳 `/sail/process-edit`（基本信息 / 表单设计 / 流程设计嵌入 SailDesigner / 更多设置），元数据落 `sf_process_meta`；「发布」= 保存 + `UpdateProcessUsingState(1)`

**验收：** 管理员可完成「建流程 → 设计 → 绑表单 → 启用 → 业务可发起」无需改库。

---

### S5 — 特色能力产品化（约 1–1.5 周）

与复刻并行或紧随其后，避免「复刻后特色变隐藏」：

- [x] 知识库：新增 / 删除 / 列表 / 向量检索（`Save/DeleteKnowledgeBaseDocument`）
- [x] Outbox 死信台：列表、过滤、单条/批量/全部重试（`WfOutboxController` + `/sail/outbox`）
- [x] 规则集 / AI：办理详情解析 BPMN 引用并跳转（`?code=` / `?configUUID=`）
- [x] 设计器模板库页 `/sail/templates` + 菜单
- [x] 业务样例请假：`WfLeave` + `SailLeaveStatusHook`（StartProcess + Status 回写，不引 NFlow EventBus）

---

### S6 — 可选增强（排期后置）

- [ ] SimpleProcessDesignerV2 移植（**暂缓**：Sail BPMN 设计器已为主路径；无无损映射 POC 前不进菜单）
- [x] 流程报表分析（`WfReport/Overview` + `/sail/report`，独立查询）
- [x] 流程分类产品表（`sf_process_category` + AppType 绑定 + `/sail/categories`）
- [x] 办理详情简易打印（`window.print` + `@media print`）
- [ ] 用户分组 / 监听器 / 表达式管理（仅当 Sail 运行时确有消费点）
- [ ] 真并行会签与引擎债：走 sailflow 自身 Multisign 语义演进，不照搬 NFlow MI 方案

---

## 7. 功能复刻矩阵（执行勾选）

| ID | 功能 | 复刻深度 | 阶段 | 依赖 |
|----|------|----------|------|------|
| F01 | 我的待办 | 完整 UX | S1 | 分页 API |
| F02 | 我的已办 | 完整 UX | S1 | 分页 API |
| F03 | 发起流程 | 完整 UX | S1/S2 | Start + 表单 |
| F04 | 我的流程 | 完整 UX | S1 | 实例查询 |
| F05 | 流程详情 | 完整 UX | S1 | Workbench |
| F06 | 通过 / 驳回 | 完整 | S1 | Run/Reject |
| F07 | 下一跳选人/选支路 | 完整 | S1 | NextTree |
| F08 | 退回 / 撤回 / 重发 | 完整 | S3 | SendBack 等 |
| F09 | 加签 | 对齐 Sail 会签 | S3 | SignForward |
| F10 | 转办 / 委派 | 能则完整，否则标明引擎缺口 | S3 | 调研 |
| F11 | 抄送箱 | 完整（产品表） | S3 | 新表 |
| F12 | 任务/实例管理 | 完整 UX | S3 | 管理查询 |
| F13 | 表单设计器 | 完整（form-create） | S2 | Form CRUD |
| F14 | 表单运行时 + 字段权 | 完整 | S2 | ACL API |
| F15 | BPMN 设计器 | **保留增强** | — | 现有 |
| F16 | 简易设计器 | 暂缓 | S6 | 映射评估 |
| F17 | 流程分类 | 完整最小 | S6 | 产品表+AppType |
| F18 | 规则集 / AI / KB | **保留+补齐** | S5 | 现有 |
| F19 | Outbox 死信台 | 完整（特色） | S5 | Host |
| F20 | 业务样例 | 完整最小闭环 | S5 | Start+回写 |
| F21 | 打印 | 简易完成 | S6 | window.print |

---

## 8. 风险与决策点

| 风险 | 影响 | 应对 |
|------|------|------|
| 芋道「驳回=结束实例」vs Sail `RejectProcess` | 用户预期不一致 | UI 文案与确认框按 Sail；文档写清 |
| `get-approval-detail` 字段多，一次做不完 | S1 延期 | Workbench 分阶段字段；前端兼容空 formConf |
| form-create 与现有 `sf:forms` 双模型 | 绑定混乱 | 约定：运行时以 Form 表 conf 为准；设计器 FormId 关联 |
| 照搬 Simple 设计器无法生成合法 Sail XML | 无用功 | S6 先做映射 POC，失败则放弃主路径 |
| 大段移植 nflow 导致样式/权限指令污染宿主 | 回归 | 严格 `adapters/` + `installSailflow` 隔离；不引入芋道 layout |
| 为抄送/分类改 pure | 违 C1 | 产品表只放 Host/biz |
| 并行开发冲掉 AI 设计器 | 特色回退 | PR 门禁：设计器 providers 变更需列出特色回归项 |

### 需产品拍板（实施前）

1. 抄送、分类是否必须与 n_flowable 同级（建议：**要**，否则待办产品不完整）。  
2. Simple 设计器是否进正式菜单（建议：S6 POC 后再定）。  
3. 办理意见、附件是否复用 sleveen 统一附件（建议：意见先做；附件接现有附件范式）。

---

## 9. 验收总标准（项目级）

1. **闭环：** 管理员用 Sail 设计器发布流程 → 用户发起（可带表单）→ 待办办理通过/驳回/退回 → 已办与我的流程可追溯 → 详情图高亮正确。  
2. **表单：** 可设计、可绑定、可按节点权限展示。  
3. **特色：** 规则集 / AI 模型 / 知识库 / 设计器 AI·Rule 节点 / Outbox 死信台均可从「流程引擎」到达且可用。  
4. **引擎：** 依赖图仍为 Host → sailflow.biz → ext → pure；无 NFlow Pure 引用。  
5. **宿主范式：** 新增业务样例若落 Biz 栈，遵循 [biz-module-paradigm.md](./biz-module-paradigm.md)；流程运行时页可保留 widgets/sailflow 专有结构。

---

## 10. 建议排期与人力（粗估）

| 阶段 | 人周（1 全栈或 1FE+1BE） | 里程碑 |
|------|-------------------------|--------|
| S0 | 0.5–1 | 壳与约定 |
| S1 | 2–3 | **可演示办理闭环** |
| S2 | 2 | 表单可用 |
| S3 | 2 | 操作条齐全 |
| S4 | 1 | 定义管理壳 |
| S5 | 1–1.5 | 特色补齐 |
| S6 | 按需 | 可选 |

**建议首发对外演示卡点：S1 完成**；对内齐套目标：S1–S5。

---

## 11. 近期启动清单（第一周）

1. 冻结本文件为复刻范围基线；打开「引擎不换」C1–C6 为团队共识。  
2. Host：实现待办/已办分页 + `GetTaskView` + NextTree 三件套。  
3. 前端：搭 `task/todo`、`process/create`、`process/detail` 三页 + 操作条 v1。  
4. 用现有 `samples/bpmn` 一条双人审批模板跑通脚本与浏览器。  
5. 并行评估 form-create 依赖进 `web/package.json` 的版本与样式隔离。  
6. 更新 [sailflow-menu-mount.md](./sailflow-menu-mount.md) 增加工作台/运行菜单表（实施 S0 时改）。

---

## 12. 参考索引

### n_flowable

- `doc/progress/README.md`（M0–M5 / M-A / Shell UX）
- `doc/design/yudao-frontend-nflow-shell-impl.md`
- `doc/design/api-gap-bpm.md`
- `doc/design/nflow-menu-mount.md`
- `pcweb/src/widgets/nflow/**`

### sailflow / sleveen

- `sailflow/doc/sail/架构设计-项目依赖与分层.md`
- `sailflow/doc/sail/业务管道特性设计.md`
- `sailflow/doc/sail/特性设计-回调与事件.md`
- `sailflow/doc/sail/progress/2026-10-31-phase*.md`
- `sleveen-admin/backend/saiflow/**`
- `sleveen-admin/web/src/widgets/sailflow/**`
- `sleveen-admin/doc/sailflow-menu-mount.md`

---

## 13. 实施进度（滚动）

| 阶段 | 状态 | 说明 |
|------|------|------|
| S0 脚手架 | **完成** | builtin 路由、runtime API、adapters、installSailflow |
| S1 办理闭环 | **完成（待浏览器 UAT）** | 待办/已办/发起/我的/详情+通过驳回 |
| S2 表单 | **完成（待浏览器 UAT）** | WfForm CRUD、fc-designer、发起/办理嵌入 form-create |
| S3 办理增强 | **完成（待浏览器 UAT）** | 退回/加签/抄送/撤回重发关闭；任务·实例管理；抄送箱 |
| S4 | **完成（待浏览器 UAT）** | 启用/停用/升级/删除；设计器入口参数保持 |
| S5 | **完成（待浏览器 UAT）** | KB；Outbox；办理关联；模板库；请假样例 |
| S6 落地 | **完成（待浏览器 UAT）** | 报表；分类；打印；Simple 设计器暂缓 |

### 2026-10-31 落地清单

- Host：`Wf2XmlController.Runtime` 增补分页待办、TaskView、下一跳/上一跳、ApprovalWorkbench、实例分页；`FromExecuted` 回传 `NewId`/`ExtraData`
- Query：`WorkflowQueryService.QueryTasksPaged` / `QueryProcessInstancesPaged`；`ProcessInstanceQuery` 增 `CreatedUserId`/`ProcessState`
- FE：`api/runtime.ts`、`SailTaskTodo/DonePage`、`SailProcessCreate/My/DetailPage`、`SailOperationBar`
- 路由：`/sail/task-todo|task-done|process-create|process-my|process-detail`
- **S2：** `IFormService` Save/Get/Delete；`WfFormController`；`FbForm` 经 Probe/init-db 建表；`/sail/forms`、`/sail/form-editor`；发起绑表单（AppInstanceCode=formId）；Workbench 回传 FormConf
- **S3：** 操作条退回/加签/抄送/撤回/重发/关闭；Workbench 回传 PreviousNodes；`sf_process_instance_copy` + `WfCopyController`；`/sail/task-copy|task-manager|process-manager`
- **S4：** `UpdateProcessUsingState` / `UpgradeProcess` / `DeleteProcess*`；流程列表启用停用/升级/删除
- **S5：** `WfOutboxController` + `/sail/outbox`；`Save/DeleteKnowledgeBaseDocument` + 知识库页增删
- **S5 齐套：** 办理详情规则/AI 只读关联；`/sail/templates`；`/sail/leave` + `sf_demo_leave` + Sync Hook 回写 Status
- **S6：** `/sail/report` · `/sail/categories`；详情打印；Simple 设计器暂缓不进主路径
- 联调入口：登录后直接打开上述路径（builtin blank）；菜单按 [sailflow-menu-mount.md](./sailflow-menu-mount.md) 补录；**需再跑一次 Probe/init-db** 建抄送/请假/分类表

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-10-31 | 初稿：全面扫描 n_flowable 与当前 Sailflow 后形成 S0–S6 复刻规划 |
| 2026-10-31 | 启动落实：S0/S1 Host+前端办理闭环首批提交 |
| 2026-10-31 | S4/S5：定义管理壳 + Outbox 死信台 + 知识库增删 |
| 2026-10-31 | S5 齐套：关联链 / 模板库 / 请假样例 |
| 2026-10-31 | S6：报表 / 分类 / 打印；Simple 设计器暂缓 |
| 2026-11-01 | 复审现状；新增现行特性说明 [sailflow-features.md](./sailflow-features.md)（规划仍管范围/勾选，特性文档管「有什么/怎么用」） |
