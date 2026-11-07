# Sailflow 待办推送集成平台

> 对齐统一日程（`UnifiedReportClient` / `portal/schedule/*`），在 SleveenAdmin 宿主经引擎 **Task Notifier + AsyncOutbox** 推送。
>
> 实现代码均在 [`backend/saiflow/src/Integration/UnifiedReport/`](../backend/saiflow/src/Integration/UnifiedReport/)。  
> **注册写法：** 构建链 `.UsePortalScheduleTaskNotifier(sp)`（扩展内读 `Enabled`，连带 Drain）；见 [sailflow-host-capability-pack.md](./sailflow-host-capability-pack.md)。

## 架构

```
TaskManager.Insert/Complete/… → FireTaskLifecycle
  → Hook Outbox（同事务）
  → HookOutboxDrain → PortalScheduleTaskNotifier
  → UnifiedReportClient → 集成平台 addSchedules / updateSchedules
  → sf_todo_portal_record 流水
```

| 本地事件 | 平台动作 | `state` |
|----------|----------|---------|
| `Created` | `addSchedules` | `0` |
| `Completed` / `Cancelled` | `updateSchedules` | `1` |

一期 **不调** `deleteSchedules`（与 szkfpt 生产一致）。

## 配置

配置文件：[`backend/core/SleveenAdmin.Web.Api/Configuration/UnifiedReport.json`](../backend/core/SleveenAdmin.Web.Api/Configuration/UnifiedReport.json)

| 键 | 说明 |
|----|------|
| `Enabled` | `false` 时扩展直接 no-op（不注册 Notifier / 不启 Outbox Drain） |
| `BaseUrl` | Token / 用户上报主机 |
| `ScheduleBaseUrl` | 日程 API 根（空则用 BaseUrl）→ `{}/portal/schedule/addSchedules` |
| `ClientId` / `BasicAuthorization` | OAuth client_credentials |
| `AppId` / `AppName` / `Type` | TaskInfo 业务系统标识 |
| `PcUrlBase` | 前端 hash 根，拼 `sail/process-detail?taskId=&processInstanceId=` |
| `PhUrlBase` | 可选移动端链接前缀 |
| `*ScheduleOperationCode` | Header `OperationCode` |

生产环境覆盖凭证与 URL；仓库内保持 `Enabled: false` 与空 ClientId。

## 字段映射

| TaskInfo | 来源 |
|----------|------|
| `appScheduleId` | `TaskId` |
| `receiveUserCode` / `Name` | `AssignedUserId` / `AssignedUserName`（Account） |
| `createUserCode` / `Name` | 任务 `CreatedUserId` / `CreatedUserName` |
| `sourceType` | 固定 `"0"`（流程） |
| `pcUrl` | `{PcUrlBase}sail/process-detail?taskId={id}&processInstanceId={pi}` |
| `name` | `{AppName}-{ActivityName}` |

须保证集成平台侧为本应用登记的账号与 **Account** 一致（szkfpt 侧推的是数字 userId，语义不同）。

## 与 szkfpt 对照

| 项 | szkfpt | 本实现 |
|----|--------|--------|
| 触发 | TodoCenter launch/complete | 引擎 Task 生命周期 |
| 异步 | `@Async` afterCommit | Hook Outbox + Drain |
| 流水表 | `primary_todo_portal_record` | `sf_todo_portal_record` |
| 打开链 | `todo/enterprise-open?todoid=` | 直达办理页（中转页二期） |
| 用户码 | 数字 `userId` | Account |

## 引擎补点

`TaskManager` 在转办关闭原待办、取消委托、撤回、退回时会 `FireTaskLifecycle(Cancelled)`，避免集成平台残留待办。

## 验收

1. `UnifiedReport.Enabled=true` 并填好凭证；重启 API；`POST api/sail/Probe/init-db` 确保 `sf_todo_portal_record` 存在。
2. 发起流程产生 Ready 待办 → 平台出现待办，`appScheduleId=taskId`；流水 `op_type=推送待办`、`is_success=Y`。
3. 办理通过 → 平台同条 `state=1`；流水 `更新待办`。
4. 转办：原待办 Cancelled→update；新责任人 Created→add。
5. 人为改错 `ScheduleBaseUrl` 使推送失败 → Outbox 死信台（`/sail/outbox`）可见；流水 `is_success=N`；修正后可重置重试。

## 二期（未做）

- 门户中转页 `enterprise-open` + `open-context`
- APP / 短信通知轨
- 推送流水管理端查询 UI
