# Sailflow 流程菜单挂载说明

**种子已内置：** 启动时 `DbInitializer.EnsureSailMenus` 会幂等写入本文菜单树（`SeedIds.MenuSail*`）。已有库重启 API 即可补录；也可仍在 **系统管理 → 菜单管理** 手工调整。

前端页面在 `web/src/widgets/sailflow`，后端 API 在 `backend/saiflow`（`/api/sail/Probe`、`/api/Wf2Xml` 等）。

现行能力与接入说明见 [sailflow-features.md](./sailflow-features.md)；复刻范围与阶段勾选见 [sailflow-nflowable-feature-replication.md](./sailflow-nflowable-feature-replication.md)。

---

## 1. 菜单树（建议）

```text
流程引擎                    ← 目录
├── 我的待办                ← S1 办理闭环
├── 我的已办
├── 抄送我的                ← S3
├── 发起流程
├── 我的流程
├── 任务管理                ← S3 管理端
├── 流程实例管理            ← S3
├── 流程定义                ← 设计入口
├── 流程表单                ← S2 form-create
├── 规则集                  ← 特色
├── AI 模型                 ← 特色
├── 知识库                  ← 特色（上传/删除）
├── 回调死信                ← S5 Outbox
├── 流程模板库              ← S5
├── 请假样例                ← S5 业务样例
├── 流程分类                ← S6
├── 流程报表                ← S6
├── 流程设计器              ← 建议隐藏；从「流程定义」点「设计」进入
├── 表单设计器              ← 建议隐藏；从「流程表单」进入
└── 流程详情                ← 建议隐藏；从待办/已办跳入
```

| 侧栏展示 | 说明 |
|----------|------|
| 要 | 待办、已办、抄送、发起、我的流程、任务管理、实例管理、流程定义、流程表单、规则集、AI 模型、知识库、回调死信、模板库、请假样例、分类、报表 |
| 隐藏 | 流程设计器、表单设计器、流程详情、流程编辑（全屏四步向导；从「流程定义」点「修改/新建」进入） |

---

## 2. 字段对照（按行录入）

类型：`1` 目录 · `2` 菜单 · `3` 按钮（本批不挂按钮）

组件写法与现网一致：可填 `widgets/.../*.vue`（路由转换会映射到 elegant 的 `_widgets_sailflow_views_*`）。

### 2.1 目录

| 标题 | 路由名 Name | 路径 Path | 组件 Component | 类型 | 排序 | 隐藏 |
|------|-------------|-----------|----------------|------|------|------|
| 流程引擎 | `sail` | `/sail` | `layout.base` | 1 | 25 | 否 |

> `Name` 会生成 `i18nKey = route.sail`；标题请用中文「流程引擎」，前端已支持无词条时回退标题。

### 2.2 子菜单（父级 = 上表「流程引擎」）

| 标题 | 路由名 Name | 路径 Path | 组件 Component | 类型 | 排序 | 隐藏 |
|------|-------------|-----------|----------------|------|------|------|
| 我的待办 | `sail_task_todo` | `/sail/task-todo` | `widgets/sailflow/views/task/SailTaskTodoPage.vue` | 2 | 1 | 否 |
| 我的已办 | `sail_task_done` | `/sail/task-done` | `widgets/sailflow/views/task/SailTaskDonePage.vue` | 2 | 2 | 否 |
| 抄送我的 | `sail_task_copy` | `/sail/task-copy` | `widgets/sailflow/views/task/SailTaskCopyPage.vue` | 2 | 3 | 否 |
| 发起流程 | `sail_process_create` | `/sail/process-create` | `widgets/sailflow/views/process/SailProcessCreatePage.vue` | 2 | 4 | 否 |
| 我的流程 | `sail_process_my` | `/sail/process-my` | `widgets/sailflow/views/process/SailProcessMyPage.vue` | 2 | 5 | 否 |
| 任务管理 | `sail_task_manager` | `/sail/task-manager` | `widgets/sailflow/views/task/SailTaskManagerPage.vue` | 2 | 6 | 否 |
| 流程实例管理 | `sail_process_manager` | `/sail/process-manager` | `widgets/sailflow/views/process/SailProcessManagerPage.vue` | 2 | 7 | 否 |
| 流程定义 | `sail_processes` | `/sail/processes` | `widgets/sailflow/views/SailProcessListPage.vue` | 2 | 8 | 否 |
| 流程表单 | `sail_forms` | `/sail/forms` | `widgets/sailflow/views/form/SailFormListPage.vue` | 2 | 9 | 否 |
| 规则集 | `sail_rulesets` | `/sail/rulesets` | `widgets/sailflow/views/SailRuleSetPage.vue` | 2 | 10 | 否 |
| AI 模型 | `sail_ai_models` | `/sail/ai-models` | `widgets/sailflow/views/SailAiModelPage.vue` | 2 | 11 | 否 |
| 知识库 | `sail_knowledge` | `/sail/knowledge` | `widgets/sailflow/views/SailKnowledgeBasePage.vue` | 2 | 12 | 否 |
| 回调死信 | `sail_outbox` | `/sail/outbox` | `widgets/sailflow/views/ops/SailOutboxPage.vue` | 2 | 13 | 否 |
| 流程模板库 | `sail_templates` | `/sail/templates` | `widgets/sailflow/views/SailTemplateGalleryPage.vue` | 2 | 14 | 否 |
| 请假样例 | `sail_leave` | `/sail/leave` | `widgets/sailflow/views/demo/SailLeavePage.vue` | 2 | 15 | 否 |
| 流程分类 | `sail_categories` | `/sail/categories` | `widgets/sailflow/views/ops/SailCategoryPage.vue` | 2 | 16 | 否 |
| 流程报表 | `sail_report` | `/sail/report` | `widgets/sailflow/views/ops/SailReportPage.vue` | 2 | 17 | 否 |
| 流程设计器 | `sail_designer` | `/sail/designer` | `widgets/sailflow/views/SailDesignerPage.vue` | 2 | 18 | **是** |
| 表单设计器 | `sail_form_editor` | `/sail/form-editor` | `widgets/sailflow/views/form/SailFormEditorPage.vue` | 2 | 19 | **是** |
| 流程详情 | `SailProcessInstanceDetail` | `/sail/process-detail` | `widgets/sailflow/views/process/SailProcessDetailPage.vue` | 2 | 20 | **是** |
| 流程编辑 | `sail_process_edit` | `/sail/process-edit` | `widgets/sailflow/views/model/form/SailProcessEditPage.vue` | 2 | 21 | **是** |

设计器带查询参数（列表页已拼好）：

```text
/sail/designer?processId={ProcessId}&version={Version}
```

详情页参数：

```text
/sail/process-detail?taskId={Id}&processInstanceId={ProcessInstanceId}
```

---

## 3. 与现有 builtin 路由的关系

当前 `web/src/router/routes/builtin.ts` 已注册同名路径，**可直接访问联调**：

| Path | 布局 | 说明 |
|------|------|------|
| `/sail/forms` 等侧栏页 | **base**（有菜单壳） | 待办/表单/定义/报表… |
| `/sail/designer` | blank 全屏 | 流程设计器 |
| `/sail/form-editor` | blank 全屏 | 表单设计器 |
| `/sail/process-detail` | blank 全屏 | 办理详情 |

**挂菜单后建议：**

1. 菜单目录用 `layout.base`；子菜单组件填 `widgets/sailflow/views/...`。
2. 若菜单与 builtin **同 path 双注册**，可能互相覆盖；侧栏页挂通后可从 `builtin.ts` 删对应项。
3. 设计器 / 表单设计器 / 办理详情建议菜单里 **隐藏**，继续走 blank。

---

## 4. 页面 ↔ 文件 ↔ API（核对用）

| 菜单标题 | 页面文件 | 主要 API |
|----------|----------|----------|
| 我的待办 | `views/task/SailTaskTodoPage.vue` | `POST /api/Wf2Xml/QueryReadyTasks` |
| 我的已办 | `views/task/SailTaskDonePage.vue` | `POST /api/Wf2Xml/QueryCompletedTasks` |
| 发起流程 | `views/process/SailProcessCreatePage.vue` | `GetProcessListSimple` + `StartProcess` |
| 我的流程 | `views/process/SailProcessMyPage.vue` | `POST /api/Wf2Xml/QueryProcessInstances` |
| 流程详情 | `views/process/SailProcessDetailPage.vue` | `GetApprovalWorkbench` + `RunProcess` / `RejectProcess` |
| 流程表单 | `views/form/SailFormListPage.vue` | `/api/WfForm/GetList` / `Delete` |
| 表单设计器 | `views/form/SailFormEditorPage.vue` | `/api/WfForm/Get` / `Save` |
| 流程定义 | `SailProcessListPage.vue` | `GetProcessListSimple`；Probe：`/api/sail/Probe/ping`、`init-db` |
| 流程设计器 | `SailDesignerPage.vue` | `QueryProcessFile` / `SaveProcessFile` / `ValidateProcess` 等 |
| 规则集 | `SailRuleSetPage.vue` | `/api/RuleSet/*` |
| AI 模型 | `SailAiModelPage.vue` | `/api/Wf2Ai/*` |
| 知识库 | `SailKnowledgeBasePage.vue` | `Get/Save/Delete/SearchKnowledgeBase*` |
| 回调死信 | `ops/SailOutboxPage.vue` | `/api/WfOutbox/Query` · `ResetDead*` |
| 流程模板库 | `SailTemplateGalleryPage.vue` | `CreateProcessByTemplate` |
| 请假样例 | `demo/SailLeavePage.vue` | `/api/WfLeave/Create` · `Query`（Hook 回写 Status） |
| 流程分类 | `ops/SailCategoryPage.vue` | `/api/WfCategory/*`（绑定写 AppType） |
| 流程报表 | `ops/SailReportPage.vue` | `GET /api/WfReport/Overview` |
| 流程定义（管理） | `SailProcessListPage.vue` | `UpdateProcessUsingState` / `UpgradeProcess` / `DeleteProcess*` |

---

## 5. 挂载后自检

1. 重新登录（刷新 `login-menu-tree`）。
2. 侧栏出现「流程引擎」及可见子项；点「我的待办」能出列表。
3. 发起流程 → 待办出现 → 办理通过/驳回 → 已办可查。
4. 列表点「设计」打开 `/sail/designer?processId=...&version=...`。
5. 若组件 404：检查 `Sm_Component` 路径，或先用 builtin 路径联调（无需菜单）。

---

## 6. 可选：i18n 词条

```ts
sail: '流程引擎',
sail_task_todo: '我的待办',
sail_task_done: '我的已办',
sail_process_create: '发起流程',
sail_process_my: '我的流程',
sail_processes: '流程定义',
sail_rulesets: '规则集',
sail_ai_models: 'AI 模型',
sail_knowledge: '知识库',
sail_outbox: '回调死信',
sail_templates: '流程模板库',
sail_leave: '请假样例',
sail_categories: '流程分类',
sail_report: '流程报表',
sail_designer: '流程设计器',
```

未补词条时，界面已回退显示「标题」中文，不影响挂载。
