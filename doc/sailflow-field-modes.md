# 节点字段编辑性（FieldModes）

> 产品缺口 §C。引擎不下发业务字段语义；Host / 办理台只传 **string mode**，由表单侧自行渲染。

## 四码（权威）

| mode | 含义 |
|------|------|
| `editable` | 可编辑 |
| `readonly` | 只读 |
| `hidden` | 不展示 |
| `masked` | 脱敏展示（内置：只读 + 掩码；**不**把掩码写回提交值） |

旧码映射：`WRITE`→`editable`，`READ`→`readonly`，`NONE`→`hidden`，`DESENSITIZE`→`masked`。

## 落盘

节点 `sf:NodePolicy.json` → `fieldsPermission: [{ field, title, permission }]`。  
`permission` 存四码之一（设计器加载时会把旧 READ/WRITE/NONE 归一）。

## Workbench

`GetApprovalWorkbench` 返回：

- `FieldModes`: `{ [fieldName]: mode }`
- `FieldPermissions`: `[{ FieldName, Title, Mode, IsReadOnly, IsNotVisible }]`（兼容旧布尔）

**优先级：** `NodePolicy.fieldsPermission` 非空 → 只用节点配置；否则回退 Form 活动权限表（`IsReadOnly` / `IsNotVisible` → 四码）。

## 内置动态表单

`SailFormRender` + `applyFieldPermissions` 消费上述结构。业务表单请读 `FieldModes`（或 `FieldPermissions[].Mode`）自行适配，勿依赖引擎脱敏。

## 配置示例

```json
{
  "fieldsPermission": [
    { "field": "mobile", "title": "手机号", "permission": "masked" },
    { "field": "reason", "title": "事由", "permission": "editable" },
    { "field": "salary", "title": "薪资", "permission": "hidden" }
  ]
}
```
