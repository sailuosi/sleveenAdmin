# Sailflow 宿主能力包写法（最佳实践）

> 状态：现行（2026-11-01）  
> 落点：`backend/sailflow`（`SailflowHostSetup` + `Hooks/*` + `Integration/UnifiedReport/*`）  
> 引擎契约全文：[sailflow/doc/sail/最佳实践-宿主能力包UseXxx.md](../../sailflow/doc/sail/最佳实践-宿主能力包UseXxx.md)  
> 关联：[sailflow-features.md](./sailflow-features.md) · [sailflow-portal-todo-push.md](./sailflow-portal-todo-push.md) · [PLAN.md](../PLAN.md)

---

## 1. 原则

在 sleveen-admin 挂 Sail 能力时：

1. **优先 `SailClientBuilder.UseXxx()`**，把开关、建表、Hook/Notifier、Drain 收进扩展。  
2. **`SailflowHostSetup` 组合根只保留链式清单**，避免 `Build()` 后大段 `if` + 手写 `addHook`。  
3. 引擎语义仍走 Gate / Hook / Notifier 三轨；本仓只做 Host 适配与主数据对接。

---

## 2. 现行组合根（摘录）

`backend/sailflow/src/Hosting/SailflowHostSetup.cs`：

```csharp
var client = SailConfigBizExtensions.CreateBuilder(configuration)
    .UseBizJson()
    .UseBizRules()
    .UseBizAction()
    .UseBizAssignees()
    .UseDebugConsoleHook()                 // 开发核验 Hook
    .UseLeaveStatusHook()                  // 请假样例 Status
    .UsePortalScheduleTaskNotifier(sp)     // UnifiedReport:Enabled 自判定
    .UseSleveenOrgResource()               // 具化 IDeptService/IResourceService → Locator
    .ConfigureServices(s =>
    {
        s.UseDatabase(…);
        s.UseBizLocalize();
        // 不用引擎 UseResource；组织人员由 UseSleveenOrgResource 注入
        s.UseForm();
        s.UseSchema(…);
        s.UseBizAI(…);
    })
    .Build();

client.UseSleveenOrgLookups(); // R3 Assignee Lookup 覆盖
```

---

## 3. 本仓已落地的能力包

| 扩展 | 代码 | 启用条件 | 行为摘要 |
|------|------|----------|----------|
| `UseDebugConsoleHook` | `Hooks/SailDebugConsoleHookExtensions.cs` | 始终（开发用） | pending `AddHook`：流程推进 + TaskCreated/Completed → 控制台 JSON |
| `UseLeaveStatusHook` | `Hooks/SailLeaveStatusHookExtensions.cs` | 始终（样例） | PostBuild：`EnsureTable` + Sync Hook（Completed/Rejected/Closed → 回写 Status） |
| `UsePortalScheduleTaskNotifier` | `Integration/UnifiedReport/PortalScheduleTaskNotifierExtensions.cs` | `UnifiedReport:Enabled` | 启用时 `UseHookOutboxDrain(autoStart:true)` + PostBuild 建流水表 + AsyncOutbox Notifier |
| `UseSleveenOrgResource` | `Resource/SleveenOrgResourceSailExtensions.cs` | 始终 | 具化 `IDeptService`/`IResourceService`（SysOrg/SysUser/AuthVisitorBag）并注入 Locator；替代引擎 `UseResource` |
| `UseSleveenOrgLookups` | `Assignee/SleveenAssigneeSailExtensions.cs` | 始终 | Build 后挂 UnitMap / RoleGroup / JobRelative / deptBoss（R3 Assignee） |

可选保留同文件 `client.useXxx` 薄封装，供测试或热补挂；**产品默认路径用 Builder**。

---

## 4. 新增能力包检查单

在本仓加一项 Host 能力时：

1. 新建 `*Extensions.cs`（Hooks 或 Integration 域目录）。  
2. 实现 `SailClientBuilder.UseXxx(...)`；开关读 `Configuration/*.json` / `IOptions`，**扩展内判定**。  
3. 需要 `SailClient` / 建表 → `SailFeaturePhase.PostBuild`；纯 Hook 无依赖 → `builder.AddHook(...)`。  
4. 需要 MS DI → 扩展参数传 `IServiceProvider`（与门户包一致）。  
5. 在 `SailflowHostSetup` **只加一行** `.UseXxx(…)`。  
6. 更新本文表格 + 必要时专题文档（如门户推送）。  

**禁止：** 在 HostSetup 外层再写 `if (enabled) { new XxxNotifier; client.addNotifier… }`。

---

## 5. 与其它文档的分工

| 文档 | 读什么 |
|------|--------|
| 引擎《最佳实践-宿主能力包UseXxx》 | 生命周期、模板、Review 清单 |
| [特性设计-回调与事件](../../sailflow/doc/sail/特性设计-回调与事件.md) | Gate/Hook/Notifier 语义 |
| [业务管道特性设计](../../sailflow/doc/sail/业务管道特性设计.md) | Builder / Collection / FeaturePhase |
| 本文 | sleveen 落点、现网扩展表、组合根样例 |
| [sailflow-portal-todo-push](./sailflow-portal-todo-push.md) | 门户推送配置与联调 |

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-11-01 | 初版：对齐 Debug / Leave / Portal 三包与 HostSetup 链式写法 |
