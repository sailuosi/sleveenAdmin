# 业务表单生命周期 Handler（§H）

> 一业务一类：`ISailFormBizHandler`。流转仍走 OA 按钮 → 引擎命令；Handler 只做业务单读写与状态。

## 契约

| 方法 | 时机 |
|------|------|
| `OnInit` | 打开发起默认值 |
| `OnQuery` | 按 AppInstanceId 回填 |
| `OnSubmit` | 首次提交落库，写出 AppInstanceId / Conditions |
| `OnStarted` | StartProcess 成功后回写实例 Id |
| `OnConsent` | 同意办理成功后（ButtonsOperation） |
| `OnBackToModify` | 退回成功后 |
| `OnCancel` | Close / Terminate / Discard / Reject 成功后 |
| `OnFinish` | 流程完成 / 驳回结束 / 关闭（Lifecycle Hook） |

`FormCode` 与 `FlowCommandContext.AppName` 对齐。

## 注册

```csharp
services.AddSailFormBiz(); // Leave + Dynamic + Registry + Host
// SailClientBuilder:
.UseFormBizLifecycleHook() // 替代 UseLeaveStatusHook
```

新增 DIY：实现 `ISailFormBizHandler`（或继承 `SailFormBizHandlerBase`）→ `services.AddScoped<ISailFormBizHandler, XxxHandler>()`。

## API

| 接口 | 说明 |
|------|------|
| `POST api/WfBizForm/Submit` | OnSubmit → StartProcess → OnStarted |
| `POST api/WfBizForm/Init` | OnInit |
| `POST api/WfBizForm/Query` | OnQuery |
| `GET api/WfBizForm/Probe` | 已注册 FormCode |
| `POST api/WfLeave/Create` | 委托 FormCode=`SailLeave` |

未注册 FormCode → 明确错误（`FormBizHandlerMissing`），不静默空转。

## 内置

| FormCode | Handler |
|----------|---------|
| `SailLeave` | `LeaveFormBizHandler`（sf_demo_leave） |
| `Dynamic` | `DynamicFormBizHandler`（无专用表） |

## 红线

- 不进 pure 引擎内核  
- 不直接改活动图 / 待办人管道  
- 与转办 / 退回正交：那些是办理动词  
