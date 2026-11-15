# 分类、表单与资源管理（宿主）

编制日期：2026-11-15。对应清单 §5。

## 入口

| 页面 | 说明 |
|------|------|
| 流程分类 | 公共/单位分入口；左 `OrgTreeSidePane` + 组织树筛选；编码 `cat_*` / `cat_u_{orgNo}_*` |
| 流程表单 | 同上；`FormEntity.Auth*`；软删；引用 `sf_process_meta.form_id` 时禁止删 |
| 单位流程列表 | 挂侧栏；按单位筛 AuthId；删除为流程逻辑删除 |

## API

- `GET /api/WfCategory/List?unitId=`、`NextSort`、`Save`、`Delete`（逻辑删）
- `GET /api/WfForm/GetList?unitId=`、`Save`（Auth*）、`Delete`（逻辑删 + 引用校验）
- `GET /api/WfUnitScope/Context`：`orgs`/`units`（管理范围树，种子清 Parent，防拍平）

## 前端组件

- `@sail/components/OrgTreeSidePane.vue`：可收起、拖宽 220–480、localStorage
- `@sail/components/SailOrgFilterTree.vue`：扁平建树
- `@sail/components/ResourceScopeFields.vue`：归属字段

## 列表布局

- 工作台（待办/已办/抄送/我的）：pageSize=20 + orderCol
- 管理列表（流程/表单）：pageSize=30 + orderCol
- 报表 Top 表：序号列 + 底部分页
