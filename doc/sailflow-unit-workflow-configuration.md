# Sailflow 单位流程配置（宿主实现）

编制日期：2026-11-15。落地仓库：`sleveen-admin`（`backend/sailflow` + `web/src/widgets/sailflow`）。

> **引擎约束**：归属/发布/发起解析在宿主完成；软删与创建人列可落引擎 `FormEntity`/`ProcessEntity`（同版本 NuGet 覆盖，不升版本号）。  
> 对照：minehub「发布即生效」单位流程；菜单见 [sailflow-menu-mount.md](./sailflow-menu-mount.md)；分类/表单见 [sailflow-category-form-resource.md](./sailflow-category-form-resource.md)。

---

## 1. 方案结论

采用 **「公共流程默认使用，单位有差异时再自定义」**：

- 单位未定制时，发起走已启用且授权的公共流程（`AuthType=PUBLIC`）。
- 需要差异时，单位流程管理员从模板/公共流程 **整图复制** 为本单位草稿（`AuthType=UNIT`），改节点后启用；发布即生效。
- 配置粒度：`businessType`（稳定业务标识）+ 办理单位。
- 角色：平台超管维护公共/模板；单位流程管理员（`Sys_Post` + `Sys_PostScope` + 职责 `sail_unit_admin`）管本单位。

**本期不做**：运行数据隔离、节点字段权限强制、手工「业务生效配置」三选一 UI。

---

## 2. AuthType 字段合同（归属）

不新建 `sf_resource_scope`。流程 / 表单 / 分类统一用已建 Auth 四件套：

| 资源 | 字段 |
|------|------|
| 流程定义 | `wf_process.p_auth_*`（主源）；`sf_process_auth` 仅作迁移双写/回填 |
| 表单 | `fm_form.ff_auth_*`（FormEntity） |
| 分类 | `sf_process_category.auth_type` / … |

**软删**（`Deleted` int，0/空=正常，1=已删）：`wf_process.p_deleted`、`fm_form.ff_deleted`、`sf_process_category.deleted`；另有 `Creator`/`CreatorName`/`DeleteTime`。

| 字段 | 合同 |
|------|------|
| **AuthType** | `PUBLIC` \| `UNIT` \| `TEMPLATE`；空 = 存量待分配（仅平台可见，不自动升 PUBLIC） |
| **AuthId** | `UNIT` 时 = 人力单位 Id（`HR_Organize` 且 `O_OrgFlag=1`）；PUBLIC/TEMPLATE **不得**把集团节点写入 AuthId 冒充 UNIT |
| **AuthCode** | PUBLIC/TEMPLATE：可选共享单位 Id 列表（逗号分隔，空=全集团启用单位）；UNIT：可选 `O_OrgNo` |
| **AuthLevel** | 密级预留；本期不参与归属与发起路由 |

| AuthType | 用途 | 可发起 |
|----------|------|--------|
| `TEMPLATE` | 复制起点 | 否 |
| `PUBLIC` | 平台公共流程 | 是（启用后） |
| `UNIT` | 单位私有流程 | 是（启用后，仅该单位生效路由） |
| （空） | 待分配 | 否 |

`sf_process_meta.business_type`：稳定业务标识（如 `leave`），不加流水；归属只写 Auth*，不进 meta。

---

## 3. 发布即生效与发起解析

### 3.1 发布

宿主在 `UpdateProcessUsingState(UsingState=1)` 成功后：

1. 写入 `sf_published_release`（processId/version/businessType/unitId/快照）。
2. 若非 `PAUSED`，更新 `sf_effective_source`：
   - `AuthType=PUBLIC` → `unit_id` 空的公共路由；
   - `AuthType=UNIT` → 该 `AuthId` 的单位路由。
3. `AuthType=TEMPLATE` 不写发起路由。

停用（UsingState=0）时：标记 release 不可用；单位路由失效后 Resolve 走回落。

### 3.2 Resolve(businessType, unitId)

```text
1. 读单位路由 sf_effective_source(unitId, businessType)
   - PAUSED → 拒绝
   - UNIT 且 release 仍可用 → 用该 ProcessId/Version
2. 沿 HR 组织祖先找 UNIT 路由（可选启用的祖先单位流程）
3. 回落公共路由 unit_id 空 + AuthType=PUBLIC 对应 release
4. 皆无 → 拒绝发起
```

发起页与 `POST api/WfBizStart/Start` 共用 Resolve；禁止仅凭前端任意 ProcessId 绕过（Host 校验定义仍为当前生效版本，或业务 Start 只收 businessType+unit）。

### 3.3 在途实例

新申请绑定发布快照对应的 ProcessId/Version；改草稿/再发布不改写历史实例所用版本。

---

## 4. 角色与授权

| 操作 | 平台管理员 | 单位流程管理员 | 普通用户 |
|------|------------|----------------|----------|
| 维护 TEMPLATE / PUBLIC | 可以 | 只读可复制（在共享范围内） | 使用已授权公共发起 |
| 设计/启用本单位 UNIT | 可以 | 仅 ManagedUnits | 无 |
| 读写他单位 UNIT | 可以 | 403 | 403 |
| 待分配（空 AuthType）分配 | 可以 | 无 | 无 |

ManagedUnits：`Sys_Post` + `Sys_PostScope`（行政组织单位卡片）；职责编号建议 `sail_unit_admin`。包含下级沿用岗位类型 `WT_Have` 语义（若有）。

---

## 5. 双入口与 CopyToUnit

| 入口 | 筛选 | 能力 |
|------|------|------|
| 流程模板中心 `/sail/templates` | `AuthType in (TEMPLATE, PUBLIC)` | 维护模板/公共；「创建单位草稿」 |
| 单位流程配置 `/sail/processes` | 组织树 + UNIT（平台另见 PUBLIC/待分配） | 设计、启用、审批人配置 |

CopyToUnit：源须已启用且 `AuthType` 为 TEMPLATE 或 PUBLIC → 新 ProcessId，`AuthType=UNIT`，`AuthId=目标单位`，复制 BPMN + meta（含 businessType）。

---

## 6. 验收口径（本期）

1. PUBLIC + `businessType=leave` 启用 → A 单位发起走公共定义。
2. B 单位 CopyToUnit → UNIT/AuthId=B → 启用后 B 走单位定义，A 仍公共。
3. 停用 B 现行版 → B 新发起回落公共；在途仍原版本。
4. 非本单位管理员读写他单位 → 403。
5. `Start(businessType, unitId)` 与发起页 Resolve 一致。

---

## 7. 附录：后续不做清单

- 实例/任务/历史/附件按单位完整隔离
- 节点字段权限运行时强制
- 手工 EffectiveSource 三选一 UI
- 改 sailflow 核心消费 Auth*
