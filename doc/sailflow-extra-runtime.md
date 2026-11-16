# ExtraJson 运行时闭环

> 状态：已落地（2026-11-16）  
> 配置 UI：`web/src/widgets/sailflow/views/model/form/ExtraSettings.vue`  
> 存库：`sf_process_meta.extra_json`

## 消费链

| 配置 | Host 落点 |
|------|-----------|
| `allowWithdrawTask` | Workbench 裁剪 Withdraw；`SailExtraOperationGate` / Dispatcher |
| `allowCancelRunningProcess` | 发起人 Close=撤销；详情头 + 我的流程 |
| `titleSetting` | `SailProcessExtraCreateHook` → `pi_title` |
| `summarySetting` | CreateHook → Conditions → PersistConditions → `__sail_summary__` |
| `processIdRule` | CreateHook + `sf_process_id_seq` → BizCode / `__sail_process_no__` |
| `autoApprovalType` | `SailProcessExtraDedupHook` → AutoApprove Outbox |
| HTTP triggers | `SailProcessExtraHttpTriggerHook` BestEffort POST |
| `printTemplateSetting` | Workbench.Extra → 详情页模板打印 |

挂载：`SailflowHostSetup` → `.UseProcessExtraRuntime(sp)`。

## 关键类型

- `SailProcessExtraService` / `SailProcessExtraDto`
- `ApprovalWorkbenchDto.Extra` / `.Summary`
