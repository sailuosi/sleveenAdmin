# 算人策略文案映射（szkfpt → Sail）

> 不改引擎 Resolve 契约；仅设计器 label / 文档对照。Type 仍用 Sail string。

## 高频业务向

| 业务说法 | Sail `AssigneeResolve.Type` | 主要 Params |
|----------|------------------------------|-------------|
| 角色 | `RoleGroup` | `roleIds` |
| 部门领导 | `deptBoss` | 可选 `roleIds`；否则 `Sail:DeptLeaderRoleIds` |
| 固定用户 | `FixedUsers` | `userIds` |
| 发起人自选 | `StarterPick` | 通常空 |
| 审批人自选 | `ApproverPick` | 通常空（出厂默认） |
| 表单字段 / 表达式 | `Expression` | `expression` / `userIdsField` |

## szkfpt `BpmTaskCandidateStrategyEnum` 对照

| szkfpt | Sail Type | 备注 |
|--------|-----------|------|
| `ROLE` | `RoleGroup` | |
| `DEPT_LEADER` | `deptBoss` | |
| `DEPT_MEMBER` | `RoleGroup` | 用部门成员角色袋近似 |
| `POST` | `JobRelative` | |
| `USER` | `FixedUsers` | |
| `START_USER_SELECT` | `StarterPick` | |
| `APPROVE_USER_SELECT` | `ApproverPick` | |
| `FORM_USER` / `EXPRESSION` | `Expression` | |
| `START_USER` | `ProcessRelative` | `relative=Starter` |

前端常量：`web/src/widgets/sailflow/bpmn/assignee/types.ts` → `RESOLVE_TYPE_OPTIONS` / `SZKFPT_STRATEGY_TO_SAIL`。  
表单：`SailAssigneeParamsForm.vue`（业务字段默认展开，高级 JSON 折叠）。
