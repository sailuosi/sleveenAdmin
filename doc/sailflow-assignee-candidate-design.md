# Sailflow 待办人（办理人 / 候选人）配置 — 设计讨论稿

> 状态：**S0–S4 已落地**；**R1–R9 已决**（含队长细则、**同码覆盖强制诊断**）  
> 日期：2026-10-31起；2026-11-01 S0–S4；同日订决议至 R9  
> 范围：用户任务节点「谁来办」——设计期配置、发起/办理时选人、运行时解析、待办归属  
> 动机：待办人配置是流程核心能力；业务侧常出现大量「奇葩」个性化需求，需先统一概念与分层，再动代码  
> 关联：[sailflow-nflowable-feature-replication.md](./sailflow-nflowable-feature-replication.md) · [sailflow-features.md](./sailflow-features.md) · 参照项目专题 [n_flowable/doc/design/assignee-candidate-report.md](../../bee-bpmn/nflow/n_flowable/doc/design/assignee-candidate-report.md)  
> **引擎侧现行基准（扩展必读）：** `sailflow/doc/sail/特性说明-待办人管道.md`（与本文冲突时以引擎基准 + 代码为准；**§0 R1–R9**）

**修订记录**

| 日期 | 内容 |
|------|------|
| 2026-10-31 | 初稿：Sail 现状 / 参照策略 / 原则与方案 |
| 2026-10-31 | 按现场口述重写 §4：定范围 1–9、办理模式第二维、多单位组内抢办组合案例；同步 §5–§9 |
| 2026-10-31 | 新增 **§4.0 三根基**：传入办理人、办理模式（含人数/比例）、算人可注册；**Performer 必须带组别 + 组内身份** |
| 2026-10-31 | 组内身份不用 Bool：`IsLeader` → **`GroupRole`（可扩展：组织/组长/副组长/成员…）** |
| 2026-10-31 | **Client 办理人核心契约**：创建待办前「查找待办人」+ 流出前「能否离开本节点」；配置对 = 类型枚举 + 参数；内置根基 + 可扩展判定 |
| 2026-10-31 | 新增 **§11 落地分层与 API 设计草稿**：场景 → pure/ext/biz/业务注册；契约/注册/BPMN/调用点 |
| 2026-10-31 | **形态调整**：重量级逻辑进新建 **pure/`sailflow.assignee`**；engine 仅薄契约调用；Type **必须为 string**（禁枚举）；按码注册**后写覆盖**；内置实现**禁止 seal** |
| 2026-11-01 | **S0 开工**：新建 `sailflow.assignee`；`IAssigneePipeline` 插槽；Mediator/AIM 薄调用双轨；`UseAssignees` / `UseBizAssignees`；Performer 扩 GroupId/GroupRole |
| 2026-11-01 | **S1**：内置 Resolve（Specific/FixedUsers/RoleGroup/ProcessRelative/StarterPick/ApproverPick）；Leave（Race/Collaborate 真判定）；Legacy 推断适配旧 Definition/会签 |
| 2026-11-01 | **S2**：Leave GroupRace/GroupCollaborate/Captain；Task/ActivityInstance/视图落 GroupId·GroupName·GroupRole；会签子实例写入组别身份 |
| 2026-11-01 | **S3**：`sf:assigneeResolve` / `sf:leaveJudge` 解析入 Activity；`BpmnAssigneeConfigReader` + MultiSign 适配；设计器双 ConfigPair 面板；发起/办理选人带 GroupId/GroupRole |
| 2026-11-01 | **S4**：UnitMap / JobRelative / BusinessCallback / Expression / Fallback；biz 样例 `Business.ClassStudents` + `TenantCollaborateLeaveJudge` 同码覆盖；`useAssigneeBusinessCallback` |
| 2026-11-01 | 引擎仓新增独立基准文档 `特性说明-待办人管道.md`（架构/分层/API/场景红线）；本文保留为讨论与产品口述溯源 |
| 2026-11-01 | 真实实现 API 落在引擎文档 §5.6；本讨论稿 §11.3 仅保留草稿对照 |
| 2026-11-01 | **§7.5 / §8 收口**：订入 R1–R5（单位、单轨、主数据、缺配置拒绝、抄送/通知分离）；其余开放问题再议 |
| 2026-11-01 | **修订 R1**：UnitId/Code/Name 独立于 Group\*（可空）；实例可选 StartUnit\*；Leave 不读 Unit |
| 2026-11-01 | **R6**：设计默认 Resolve=`ApproverPick`（发送人自选）+ Leave=`Collaborate`；发布前非空；预埋可覆盖 |
| 2026-11-01 | **R3 补**：AssignedUserId = 用户编码（如登录账号） |
| 2026-11-01 | **R7**：组间完成 And/Or + groupCompleteCount/Rate |
| 2026-11-01 | **R8**：队长关卡必办、组员可办、关卡办完可发送 |
| 2026-11-01 | **R9**：同码覆盖诊断强制 |
| 2026-11-01 | **产品空人**：NodePolicy `assignEmptyHandlerType` 默认=2 拒绝（R4）；`=1/3/4` 显式；Fallback `onEmpty` ≠ 产品空人策略（见 todo §A / §C 宿主接线） |
| 2026-11-01 | **转办**：引擎 `TransferProcess` + 码表 `Transfer`（个人办结+本节点交人）；≠ 委派 |

---

## 0. 讨论目标

1. 说清 **Sail 今天实际怎么做**（避免把「设计器能挂角色」误当成「策略已齐」）。  
2. 对照参照项目 **用户任务 · 候选人策略**，标出可复用概念 vs 不可照搬点。  
3. 以现场口述为准，固定 **定范围（1–9）× 办理模式（抢办/协办/组内…）** 两维模型，区分产品化与扩展点。  
4. 钉死 **三根基**（§4.0）与 **Client 两大时刻契约**（§4.0.1）：复杂场景与业务扩展都挂在契约上。  
5. 给出可选架构方向与 **开放问题清单**（尤其「单位维度」），共识后再开实施任务。

**本文不包含**：接口改造、BPMN 扩展改写、前端面板改版、具体排期勾选。

---

## 1. 概念对齐（先统一叫法）

| 叫法 | 含义 | 备注 |
|------|------|------|
| **用户任务** | BPMN User Task（设计器中文常译「用户任务」） | 参照项目审批节点同属此类 |
| **办理人 / 待办人** | 任务落到具体人后的执行者（待办列表按此人过滤） | Sail 任务表 `AssignedUserId` |
| **候选人** | 尚未认领或多人可选时的候选集合 | 参照项目 candidateUsers；Sail 多用「下一跳 Users 树」再 Specific 指定 |
| **参与者 Performer** | Sail 设计器挂在节点上的 Role/User 条目 | `sf:Performer`，不等于完整策略模型 |
| **候选人策略** | 参照项目 `CandidateStrategy` 枚举 | 「按什么规则算出人」 |
| **下一办理人指定方式** | Sail `NextPerformerIntTypeEnum` | Specific / Definition / Single / Traced… 是**运行时选人来源**，不是策略枚举 |
| **配置对 ConfigPair** | `{ Type, Params }` | **Type 为 string（禁枚举）** + 配置参数；定范围与流出判定均用此形 |
| **查找待办人** | Client 契约时刻 A | **创建待办之前**解析出 `Performer[]` |
| **流出判定** | Client 契约时刻 B | **当前节点能否离开并流向下一节点**（完成条件） |

易混点：

- **发起人自选** ≠ **审批人自选**：前者发起时指定该节点办理人；后者当前审批人通过时指定**下一节点**办理人。  
- **设计器挂了 User** ≠ **运行时一定按该 User 派发**：Sail `Definition` 路径当前主要吃 **Role**。

---

## 2. Sail 现状（事实基线）

### 2.1 设计期

- 选中 **User Task** → 属性面板「参与者」列表。  
- 添加对话框可选：`Role` | `User`，写入 BPMN 扩展：

```text
sf:Performers
  └─ sf:Performer  name / outerId / outerCode / outerType
```

- **没有**「策略下拉」（发起人自选、部门负责人、表单字段、表达式等）。

### 2.2 解析与派发

| 路径 | 行为 |
|------|------|
| `NextPerformerType = Specific` | 调用方传入 `NextActivityPerformers`（办理页勾选用户）→ 写入活动/任务办理人 |
| `Definition` | `GetActivityRoles` **仅收集 outerType=Role** → `GetUserListByRoles` 展开用户 |
| `Single` | 下一办理人 = 当前操作人（测试向） |
| `Traced` | 退回等，按历史办理人 |

办理壳 `SailOperationBar`：拉下一节点树上的 `Users`（角色展开结果），用户勾选后以 **Specific** 提交。

发起：`StartProcess` 若未显式传下一办理人，常见路径会把**发起人自己**塞进下一跳（与「按节点定义角色派发」不是同一故事）。

### 2.3 与「完整待办人产品」的差距（摘要）

| 能力 | Sail 现状 |
|------|-----------|
| 指定角色 → 展开用户 | 有（Definition + 资源表） |
| 指定用户（设计期钉死） | 可存 XML；**Definition 不算 User** |
| 发起人自选 / 审批人自选 | 无独立策略；部分靠 Specific 手工传人 |
| 组织类（部门/负责人/岗位） | 无策略层 |
| 表单字段 / 表达式算人 | 无统一产品面 |
| 空办理人策略（挂起 / 管理员 / 报错） | 未产品化 |
| 部署期强制「人工节点必须能解析出人」 | 无 |
| 会签 / 认领 / 候选池 vs 单办理人 | 多实例与 Performer 未统一成策略语言 |
| 办理人组别 / 组内身份 | **无**；`Performer` 仅 UserId/UserName，撑不起组内/队长 |
| 算人插件注册 | **无** |
| Client 两大时刻契约 | **无**；找人/完成散落在 Mediator/会签路径，无统一 ConfigPair 注册表 |

---

## 3. 参照项目（n_flowable / 芋道系）要点

产品语言：**用户任务上选「候选人策略」**，默认常为 **发起人自选 (35)**。

| 策略（节选） | 值 | 何时定人 |
|--------------|-----|----------|
| 指定角色 | 10 | 任务创建时按角色算人 |
| 部门成员 / 负责人 / 多级 / 岗位 | 20–23 | 组织主数据 |
| 指定成员 | 30 | 设计期钉死用户 |
| 审批人自选 | 34 | **办理通过时**选下一节点 |
| 发起人自选 | 35 | **发起时**为该节点选人 |
| 发起人本人 | 36 | 固定发起人 |
| 发起人部门负责人等 | 37–38 | 相对发起人组织 |
| 用户组 / 表单用户 / 表单部门负责人 / 表达式 | 40 / 50 / 51 / 60 | 扩展与动态 |

链路（应有行为）：

```text
设计器选策略+参数 → 写入 BPMN 扩展
    → 部署校验（人工节点必须有策略）
    → 发起：预测节点；若含 35 则 Timeline 选人并落变量
    → 到达 UserTask：Invoker 按策略算用户集合
    → 单人 assignee / 多人候选或会签集合
    → 待办按身份过滤
```

参照专题细节见 n_flowable `assignee-candidate-report.md`（含组织主数据、Account 映射等约定）。  
**Sail 引擎不是 Flowable Identity 那套**，不能原样搬 `camunda:assignee` + Long userId；能搬的是 **策略分层与闭环体验**。

---

## 4. 现场场景归纳（用户口述，讨论基线）

> 来源：业务侧长期踩坑经验。核心认识：**待办人配置至少是两维正交**——  
> **第一维：定范围（谁进入候选/办理集合）** · **第二维：办理模式（抢办 / 协办 / 组内语义）**。  
> 奇葩需求多半出在「两维组合」与「单位维度定义不清」，而不是缺某一个下拉项。  
> **上层场景（1–9、组内/队长）一律建立在 §4.0 三根基之上**；业务侧扩展只插在根基 3，不绕开 1/2。

### 4.0 三根基（引擎/协议必须先具备）

复杂定范围与办理模式是「业务可配置」；下面三项是 **本地引擎 + 载荷契约** 的根基。没有它们，组内抢办/协办、队长模式、发起人矩阵自选都落不了地。

| # | 根基 | 要求 | 现状摘要（2026-10-31） |
|---|------|------|------------------------|
| **F1** | **发送时传入办理人** | 发起/办理推进时，调用方可传入下一节点办理人列表；**传入谁就派给谁**（`NextPerformerType=Specific` + `NextActivityPerformers`）。缺省路径不得静默改写已传入列表。 | 引擎契约基本有；发起默认常塞发起人自己，产品闭环半通 |
| **F2** | **办理模式完整** | 一维：**抢办**、**协办**；二维：**组内抢办**、**组内协办**、**队长模式**。完成条件允许用 **固定人数** 或 **比例**（比「全员必办」更自由，予以采纳）。 | 一维会签（人数/比例）有骨架；抢办未产品化；组内/队长无 |
| **F3** | **算人逻辑可抽象、可动态注册** | `IAssigneeResolver`（或等价）按策略算人；内置 + 业务插件可注册，输出带结构的办理人集合（见下文字段）。 | **无**；仅 Role 展开 / Specific 透传写死路径 |

#### F2 完成条件（已决议口径）

协办 / 组内协办的「办完」不必绑死「全员」：

- **固定人数**：如集合 5 人，配 `completeCount=2` 即两人办完可推进；  
- **比例**：如 `completeRate=0.6`（或百分比）达到即推进；  
- 与现有会签 `CompareType`（人数 / 百分比）+ `CompleteOrder` **方向一致**，产品化时统一成办理模式参数，不另造一套难懂 DSL。

抢办可视为「完成条件 = 1 人」的特例，或独立认领语义；二者择一产品化即可，底层仍用同一套完成判定。

#### F1/F2 共同前提：办理人载荷必须带「组别」与「组内身份」

> **已决议**：办理人条目不能只有 `UserId/UserName`。  
> 必须有 **组别（GroupId / GroupName）** 与 **组内身份（GroupRole）**。  
> **不用 Bool「是否组长」**：身份用可扩展码，便于后续加组织者、副组长、成员等，办理模式按「哪些身份算关卡」配置，而不是写死 `isLeader==true`。

当前引擎 `Performer` 仅有 `UserId`、`UserName`（见 `Performer.cs`）——**不足以支撑根基 2 的二维语义**，扩展为最低字段集：

```text
Performer（运行时办理人条目，发送传入 / Resolver 输出 / 任务拆分输入）
  ├─ UserId          必填
  ├─ UserName        建议
  ├─ GroupId         办理组别标识（一维扁平时可空或同组常量；Leave 用）
  ├─ GroupName       组别展示（可选）
  ├─ GroupRole       组内身份（字符串码，可扩展；见下）
  ├─ UnitId          企业单位键（string，可空；展示/筛选，Leave 不读）
  ├─ UnitCode        单位编码（string，可空）
  └─ UnitName        单位名称（string，可空）
```

> **R1 修订（2026-11-01）：** Group\* ≠ Unit\*。企业单位**不得**只塞进 GroupId。流程实例另可选 `StartUnit*`（发起单位）。详见 §7.5。

**GroupRole 建议内置码（可增不可滥改语义）：**

| 码 | 含义 | 典型用途 |
|----|------|----------|
| `Organizer` | 组织者 | 发起协调、非审批关卡（可选） |
| `Leader` | 组长 / 队长 | 队长模式默认关卡身份 |
| `DeputyLeader` | 副组长 | 可与 Leader 一并列入关卡，或代理 |
| `Member` | 成员 | 默认；组内抢办/协办的普通办理人 |

未填时默认 `Member`。业务可注册扩展码（如 `Secretary`），完成条件通过「关卡身份集合」引用，引擎不写死 Bool。

载荷形状从「扁平列表」升级为「可分组列表」（逻辑等价于 `Map<GroupId, Performer[]>`，序列化仍可用数组 + GroupId）：

```text
NextActivityPerformers[activityId] = [
  { userId, userName, groupId: "U1", groupRole: "Leader" },
  { userId, userName, groupId: "U1", groupRole: "Member" },
  { userId, userName, groupId: "U2", groupRole: "Leader" },
  { userId, userName, groupId: "U2", groupRole: "DeputyLeader" },
  …
]
```

与办理模式的关系：

| 模式 | 如何用组别 / 组内身份 |
|------|----------------------|
| 一维抢办 / 协办 | `GroupId` 可空或全员同一组；`GroupRole` 可忽略或全 `Member` |
| 组内抢办 | 按 `GroupId` 分组；**组内任意一人**办完 → 该组完成；组间再 AND/OR |
| 组内协办 | 按组；组内按人数/比例完成 → 该组完成（可限定参与身份，如仅 `Member|Leader`） |
| 队长模式 | 按组；**R8**：关卡与组员均可有待办；**仅关卡办完**使组完成并可发送；`gateRoles` 默认 `{Leader}` 可配 |

**约定**：

1. 结构化自选（场景 8 + 多单位）发出的矩阵：有组内抢办语义时须带 **GroupId**；有企业单位语境时填 **Unit\***（可空）；二者重叠时可双写 `GroupId=UnitId`。身份默认可选 `Member`。  
2. Resolver（F3）有组语义须填齐 Group\*；有单位语境建议填 Unit\*（允许空）。禁止靠「逗号拼接 AssignedUserIds」事后猜组。  
3. 任务/活动实例落库须能还原 Group\*（Leave）与 Unit\*（展示）；实例可选 StartUnit\*。

#### 三根基与上层场景的分工

```text
F1 传入办理人（含组别 / GroupRole）
F2 办理模式执行器（抢办/协办/组内*/队长 + 人数|比例；关卡按身份码）
F3 Resolver 注册（定范围 1–9 算人）
        │
        ▼
挂到 Client 两大时刻契约（§4.0.1）；业务扩类型码与 Params，不改派发内核
```

### 4.0.1 Client 办理人核心契约（两大关键时刻）

> **已决议方向**：办理人相关最硬的两段逻辑，必须落在 **`SailClient` 下的办理人核心契约**（与现有 `addGate` / Hook 注册风格对齐），而不是散落在 Host 控制器或各业务项目里改引擎私有方法。

引擎在用户任务生命周期里只保证调用这两点；**「怎么找人」「能不能走」** 全部由契约实现 + 节点上的 **配置对** 驱动。

```text
                    ┌─────────────────────────────────────┐
  到达用户任务       │  时刻 A · 查找待办人                  │
  创建 Task 之前 ──► │  ResolveAssignees(ctx, configPair)   │
                    │  → Performer[]（含 GroupId/GroupRole）│
                    └─────────────────┬───────────────────┘
                                      ▼
                               创建待办 / 写入指派
                                      │
                    ┌─────────────────▼───────────────────┐
  某人办理提交后     │  时刻 B · 流出判定                    │
  （或每次完成检查） │  CanLeaveActivity(ctx, configPair)   │
                    │  → Allow | Deny(+原因)                │
                    └─────────────────┬───────────────────┘
                                      │ Allow
                                      ▼
                               离开本节点 → 下一节点
                               （下一节点再走时刻 A）
```

| 时刻 | 契约职责 | 输入（示意） | 输出 | 对应产品维 |
|------|----------|--------------|------|------------|
| **A · 查找待办人** | 创建待办前算出办理人集合 | 流程/节点/实例上下文、发起或 Specific 传入、**定范围配置对** | `Performer[]` | 第一维定范围（§4.1）+ F1 透传 |
| **B · 流出判定** | 判断当前节点是否已满足离开条件 | 本节点任务完成快照、组别/身份、**办理模式配置对** | 是否可流出 + 诊断 | 第二维办理模式（§4.2）+ F2 |

二者正交：**A 不决定「几人办完能走」；B 不负责「下一跳找谁」**（下一跳找人是下一节点的时刻 A，或发送时 Specific 预填）。

#### 配置对：Type（string）+ 配置参数

节点（或转移）上每一处办理人相关配置，统一为：

```text
ConfigPair = {
  Type:   <string 码>,          // 禁止 enum；内置用静态 string 常量比较
  Params: <JSON / 字典>         // 该类型专属参数，未知键由实现自行解释
}
```

用户任务上至少两对（可同面板分两栏）：

| 配置对 | Type 语义 | Params 示例 |
|--------|-----------|-------------|
| **AssigneeResolve**（时刻 A） | 定范围策略码：FixedUsers / RoleGroup / ProcessRelative / UnitMap / StarterPick / BusinessCallback / … | 用户列表、角色 Id、相对类型、单位维度字段、回调名… |
| **LeaveJudge**（时刻 B） | 办理模式/完成判定码：Race / Collaborate / GroupRace / GroupCollaborate / Captain / … | `completeCount`、`completeRate`、`gateRoles`、`groupJoin=AND|OR`… |

设计器只编辑 ConfigPair；运行时 Client 按 `Type` **查注册表**得到处理器，再喂 `Params`。  
**禁止**把 Params 结构写死进引擎 switch 的每一个业务分支——扩展类型只注册新 Type + 自己的 Params 约定。

#### 内置根基能力 vs 扩展判定

| 层 | 时刻 A（查找） | 时刻 B（流出） |
|----|----------------|----------------|
| **必须内置（根基）** | Specific 透传（F1）；按角色/群组展开；固定人；流程相对（发起人/上一发送人）；空结果策略入口 | 抢办（完成=1）；协办（人数/比例）；按 GroupId 的组内抢办/协办；队长（`gateRoles` 默认 Leader）；组间 AND |
| **可扩展** | 单位映射、岗位相对、业务回调(9)、表达式/规则集、租户自定义 Type | 自定义完成公式、外部会签服务回写、特殊「组织者确认」等 |

扩展方式（对齐 Client）：

```text
SailClient
  .UseAssignees()                                       // 注入 Pipeline + 注册内置（sailflow.assignee）
  .useAssigneeResolver(IAssigneeResolver)               // 按 Type:string 注册；后写覆盖
  .useLeaveJudge(IActivityLeaveJudge)
```

内置在 **`sailflow.assignee`**，类 **不 seal**；业务可用同名 Type 覆盖或继承后再注册。engine **只调** `IAssigneePipeline`，**不得** fork NodeMediator 写判定。

#### 与三根基、两维场景的映射

| 概念 | 落点 |
|------|------|
| F1 传入即派发 | 时刻 A 的内置 Type（如 `Specific` / `StarterPick` 已写入的列表）直接返回传入 `Performer[]` |
| F2 办理模式 | 时刻 B 的 Type + Params（人数/比例/组/关卡身份） |
| F3 算人可注册 | 时刻 A 的注册表（内置 + 扩展） |
| 定范围 1–9 | 主要是 AssigneeResolve 的 Type 清单 |
| 抢办/协办/组内/队长 | 主要是 LeaveJudge 的 Type 清单 |
| GroupId / GroupRole | A 的输出字段；B 的判定输入 |

### 4.1 第一维：办理人「定范围」策略族

| # | 场景名 | 含义 | 数据依赖 | 典型坑 / 备注 |
|---|--------|------|----------|----------------|
| **1** | **固定人** | 钉死具体人：张三、李四 | 用户主数据 | 最常见、最简单；人员调动后模型要改或要「岗位化」迁移到 3 |
| **2** | **流程相对人** | 相对流程办理关系：发起人、上一节点发送人/办理人等 | 流程实例上下文（发起人、转移历史） | 与组织无关；退回/撤回后再提交时「上一节点」是否重算要约定 |
| **3** | **岗位相对** | 人力资源视角的岗位关系（如某岗、汇报线、编制岗） | **人事/岗位主数据** | 规则可固化，但没有人力数据就落不了地；与 2 的差别是「相对 HR」，不是「相对流程」 |
| **4** | **单位相对 · 固定人** | 按单位映射固定办理人：公司 A→张三，公司 B→李四 | 单位维度 + 映射表 | **最大坑：单位维度怎么定**（法人、行政组织、成本中心、项目部、加盟店？表单字段还是发起人所属？多组织兼职取哪条？） |
| **5** | **群组** | 固定落到某个群组（用户组/角色组） | 群组成员关系 | 接近参照「角色/用户组」；要明确群组维护入口 |
| **6** | **单位相对 + 群组** | 4 的扩展：先定单位，再取该单位下某群组 | 单位维度 + 群组在单位下的成员 | 映射从「单位→人」变为「单位→群组→人」；单位坑同 4 |
| **7** | **缺省 / 兜底策略** | 相对单位等规则**未命中**时走的后备 | 同上 + 兜底配置 | 与「空办理人政策」绑定：未命中是报错、挂起，还是走缺省人/缺省组 |
| **8** | **发起人自选** | 启动时由发起人指定**多个环节**的办理人；常用于后续领导节点不让领导再选 | 发起页选人 + 实例变量 | 业务动态、难配死规则时的逃生舱；要支持按节点分别选、必填校验 |
| **9** | **业务动态取人** | 表单**不直接**给出办理人；业务侧按逻辑查子表/关联表再设人 | 业务 API / 子表查询 | 例：当前节点上下文是「班级」，下一节点「学生签字」→ 查班级学员名单设办理人。**产品内置枚举不够，必须有扩展算人（业务回调）** |

抽象一下，第一维其实是四类「相对坐标系」：

```text
绝对钉死     → 1 固定人 · 5 群组
流程相对     → 2 发起人 / 上一发送人 …
组织/人力相对 → 3 岗位 · 4/6 单位(+群组) · 7 未命中兜底
运行时补全   → 8 发起人自选 ·（另：审批人自选，口述未单列，建议保留）
业务相对     → 9 业务回调 / 子表查询
```

### 4.2 第二维：办理模式（集合如何变成任务）

定范围只得到 **带组别 / 组内身份的办理人集合**（§4.0）；第二维决定怎么变成待办与如何算完成：

| 模式 | 含义（现场口径） | 完成条件 |
|------|------------------|----------|
| **抢办** | 集合内任意一人办理即可推进（或签 / 认领后办） | 等价完成人数=1（或认领后单人办） |
| **协办** | 集合内多人按规则办理（会签） | **固定人数** 或 **比例**（不必全员） |
| **组内抢办** | 按 `GroupId` 分组；**组内**任意一人办即可，算该组完成 | 组内人数=1；组间再 AND/OR |
| **组内协办** | 按组；组内多人按规则办完，该组才完成 | 组内 **固定人数** 或 **比例**（可按身份过滤） |
| **队长模式** | 按组；以配置的 **关卡身份**（默认 `Leader`）办完为组完成（详见 §8） | 依赖 `GroupRole`，非 Bool |

组间关系通常还要再定一层（常被漏配）：

- **所有组完成后**才到下一节点（并行多组 AND）；或  
- 任一组合即可（少见，需显式配置）。

### 4.3 组合案例（已出现过的「怪需求」）

**组内抢办 + 发起人自选（多单位）：**

1. 发起人发送时选择 **多个单位**；  
2. 每个单位下再选择 **多个人**（可标组内身份，如 Leader / Member）；  
3. **每个单位内任意一人审批即可**（组内抢办）；  
4. **所有单位都审批完成后**，流程才到下一节点（组间 AND）。

这不是「单一候选池抢办」能描述的，模型上至少需要：

```text
发起人自选(8)
  → 产出：Performer[] 带 Unit*（各单位展示）+ GroupId（本场景常=UnitId 双写）+ GroupRole
办理模式：组内抢办 + 组间全部完成（Leave 只读 GroupId）
经由：F1 传入整表 → F2 按组拆任务并判完成
```

实施时往往对应：**多路并行人工分支 / 多实例按组拆分 / 或显式「单位会签」结构**，而不是只改一个扁平 `AssignedUserId`。

### 4.4 从场景反推的建模含义

| 启示 | 说明 |
|------|------|
| **先有三根基 + Client 两大时刻** | F1/F2/F3 挂到时刻 A（查找）/ B（流出）；场景 1–9 = Resolve Type；组内/队长 = LeaveJudge Type |
| **两维正交** | UI/模型不要把「定范围」和「抢办协办」揉成一个下拉；对应两套 ConfigPair |
| **办理人必须可分组、可标身份** | Performer 带 GroupId + **GroupRole**（勿用 Bool）；Leave 只认 Group\* |
| **单位是一等公民（业务维）** | 独立 **UnitId/Code/Name**（可空）；与 Group 正交；实例可选 StartUnit\*（R1 修订） |
| **完成条件要自由** | 协办/组内协办用人数或比例，不强制全员；队长关卡按身份码集合配置 |
| **缺省策略是一等公民** | 7 不是异常日志，是配置项 |
| **业务动态必须可插拔** | 9 → F3 扩展契约（上下文 → Performer[]，含组/身份） |
| **自选可带结构** | 8 不止「选一堆人」，还可能「选多个单位，每单位多人 + 组内身份」 |
| **与参照项目映射** | 1≈指定成员；5≈角色/用户组；2≈发起人本人/上一办理人；8≈发起人自选；3/4/6/7/9 超出芋道默认枚举，要 Sail 自建或扩展 |

### 4.5 其它仍常见的交叉（保留）

- 金额等条件走**网关**分流，勿塞进算人策略。  
- 同一人多节点重复出现 → 自动去重/免批（「更多设置」）。  
- 退回后再提交：相对人（2）与单位映射（4）是否重算。  
- 办理人 vs 抄送/通知人边界。

结论倾向：**先落 Client 两大时刻契约 + 配置对 + 内置根基注册**；再夯实 Performer（组别 / GroupRole）；再 **高频场景（1/2/5/8 + 抢办协办）产品化**；单位族与业务动态以**扩展 Type** 挂上，而不是改内核。

---

## 5. 设计原则（建议先共识）

0. **Client 两大时刻契约优先**  
   - 时刻 A「查找待办人」、时刻 B「流出判定」必须是 `SailClient` 办理人核心契约；  
   - 配置统一为 **ConfigPair = Type + Params**；内置根基能力默认注册，扩展只加 Type。  

1. **三根基落在契约上，而不是旁路脚本**  
   - F1 → A 的透传 Type；F2 → B 的 LeaveJudge Type；F3 → A 的注册表；  
   - **Performer 必含 GroupId、GroupRole**（§4.0）。  

2. **策略与运行时指定分离**  
   - 节点上两套 ConfigPair；Specific / 自选是 A 的一种 Type 或 Params 来源，补全最终办理人。  

3. **定范围 × 办理模式 正交**  
   - A 只产出 `Performer[]`；B 只根据完成快照 + LeaveJudge Params 回答能否离开。  

4. **单一真源写在流程定义上**  
   - 两套 ConfigPair 进 BPMN 扩展（或版本绑定 meta），不靠前端临时状态。  

5. **算人与流出必须可解释**  
   - 调试可见：Resolve Type/Params、Leave Type/Params、组别/身份、命中/未命中、Deny 原因。  

6. **空办理人 / 未命中单位不可静默**  
   - 与场景 **7** 或 Resolve 的空结果 Params 绑定：拒绝 / 挂起 / 缺省人(组)。  

7. **用户身份、单位、人力主数据先约定**  
   - UserId；单位 → GroupId；HR 表；与 **GroupRole** 勿混。  

8. **扩展挂注册表，禁止改派发内核**  
   - `useAssigneeResolver` / `useLeaveJudge`；场景 9、自定义完成公式均走扩展 Type。  

9. **Type 只用 string，扩展靠注册覆盖**  
   - 禁止分发用 enum；内置静态 string 常量；同码后注册覆盖；内置类不 seal。

---

## 6. 可选架构方向（供讨论，不选型拍板）

### 方案 A — 最小增强（贴着现有 Performer）

- 补齐：`Definition` 同时消费 `User`；部署校验「至少有一个 Performer」。  
- 发起/办理：规范化 Specific 必填场景。  
- **优点**：改动小。  
- **缺点**：仍无策略语言，个性化只能堆 Specific + 外部脚本。

### 方案 B — Client 薄调用 + `sailflow.assignee` + 配置对（推荐主线）

- **engine** 仅 `IAssigneePipeline` 薄调用；重量在 **pure/`sailflow.assignee`**。  
- 节点两套 **ConfigPair**：Type 为 **string**（禁 enum）+ Params。  
- 注册按码、**后写覆盖**；内置实现 **不 seal**。  
- 时刻 A → `Performer[]`；时刻 B → Allow/Deny。  
- **优点**：改动面与 engine 解耦；业务可覆写内置；与分层铁律一致。  
- **缺点**：多一个 pure 包；要碰启动/办理/设计器/会签收口。

### 方案 C — 规则引擎 / 表达式优先

- 仅作为 **扩展 Type**（Resolve 或 Leave 的一种），不算替代契约。  
- **优点**：极端个性化灵活。  
- **缺点**：单独作为默认路径门槛高。

**建议讨论立场**：核⼼走 **B（Client 两大时刻 + 配置对）**；长尾用 **C 作扩展 Type**；A 仅作过渡止血。

---

## 7. 建议的产品分层（讨论稿）

```text
┌──────────────────────────────────────────────────┐
│ 设计器：用户任务 · 两套 ConfigPair                 │
│  ① AssigneeResolve：Type + Params（定范围）        │
│  ② LeaveJudge：Type + Params（抢办/协办/组内/队长） │
└────────────────────┬─────────────────────────────┘
                     ▼
┌──────────────────────────────────────────────────┐
│ engine：IAssigneePipeline 薄调用（时刻 A / B）      │
└────────────────────┬─────────────────────────────┘
                     ▼
┌──────────────────────────────────────────────────┐
│ sailflow.assignee：Registry[type:string]           │
│  内置（非 seal）← 可被业务同码覆盖                  │
└────────┬─────────────────────────────┬───────────┘
         │ Resolve                       │ CanLeave
         ▼                               ▼
   创建待办（组/身份）                 是否允许流出
```

与现网组件映射（现状 → 目标）：

| 现状 | 目标角色 |
|------|----------|
| Gate/Hook 注册风格 | assignee 包 `UseAssignees` / `useAssigneeResolver` / `useLeaveJudge` |
| 会签通过率写在 AIM | **迁出**到 assignee LeaveJudge；engine 只调 Pipeline |
| `Performer` 仅 UserId/UserName | engine 扩 GroupId / GroupRole；策略在 assignee |
| `sf:Performer` / MultiSign | assignee 适配器 → ConfigPair（type:string） |
| `SailOperationBar` 选人 | StarterPick / ApproverPick 载荷；矩阵 + 身份 UI |

### 7.5 已决决议（2026-11-01，与引擎 §0 R1–R9 对齐）

> 权威条文以引擎 `特性说明-待办人管道.md` **§0** 为准；此处为宿主/产品侧镜像，便于对照开放问题。

| 码 | 主题 | 决议（宿主视角） |
|----|------|------------------|
| **R1** | Group\* vs Unit\* | **正交，禁止 GroupId 冒充单位。** ① Group\*：办理分组 + 组内身份；Leave **只读 Group\***。② **UnitId/UnitCode/UnitName**（string，**可空**）：企业单位展示/筛选；落 Performer→任务/活动/视图。③ 流程实例可选 **StartUnitId/Code/Name**（发起单位，供实例列表筛），**不覆盖**节点 Unit\*。④ 重叠时可双写 `GroupId=UnitId` 并仍填 Unit\*。 |
| **R2** | 单轨定型 | 定型期**收掉双轨**，不背历史债。运行时只走 Assignee 管道；缺配置不得回落 Definition/旧会签。 |
| **R3** | 主数据与用户身份键 | 在 **sleveen-admin** 接组织/人员（如 `SysUser`/Org）；覆盖或继承 Lookup。sailflow 不吸收宿主用户表。**`AssignedUserId` / `Performer.UserId` = 用户编码（如登录账号 `Account`）**，选人/派发/待办过滤同一套；不默认存 `Id` GUID 或人事工号。 |
| **R4** | 缺配置 | 运行时**禁止暗默认** → **拒绝** + ReasonCode。设计预埋见 **R6**（写入 BPMN 后运行时只认落盘配置）。显式 `Fallback`/`onEmpty` 除外。 |
| **R5** | 三线分离 | 待办人 ConfigPair ≠ 通知（Task Notifier / 回调与事件）≠ 抄送（独立通道 + 自有事件 / 现 Host 表 `sf_process_instance_copy`）。设计器勿混面板。 |
| **R6** | 设计默认 | **流程设计能力**。发布前 Resolve+Leave 非空。预埋可覆盖。出厂默认：Resolve=**`ApproverPick`（发送人自选）**，Leave=**`Collaborate`**。勿与 `StarterPick`（发起人自选）混淆。设计器新增 UserTask / 模板种子写入；项目可改默认（如改 Race）。 |
| **R7** | 组间完成 | 组间不仅 And/Or，还可按 **已完成组数量/比例**（`groupCompleteCount` / `groupCompleteRate`）；默认 And。组内仍用 `completeCount`/`completeRate`。 |
| **R8** | 队长细则 | **关卡必办、组员可办、关卡办完可发送**；`gateRoles` 默认 Leader 可配；组员有待办但不能单独使组完成。 |
| **R9** | 同码覆盖诊断 | **强制**：注册时日志 + 运行可标明 Type 已被覆盖（实现类型名等）；禁止静默覆盖。 |

**实施进度（2026-11-01）：** Unit\*/StartUnit\*、R6 预埋+发布校验、R7 Count/Rate、R8 队长口径、R9 覆盖诊断、R2/R4 单轨+ReasonCode（含 API `reasonCode`）、sleveen UnitMap→SysUser.Account、前端 Runner/选人键=Account、Claims 覆盖操作人、办理条跳转、Host `[Authorize]` 已落地。

**宿主角色/岗位（无独立 SysRole·HR 表）：**
- **RoleGroup**：优先 `AuthVisitorBag`（Code/Id）或 `AuthDutyItem`→VisitorBag 展开成员（账号/组织）；未命中再回落引擎资源角色 + Account 重映射。
- **JobRelative**：`Params.maps` 优先；无 maps 时用同 Code 的 `AuthVisitorBag` 账号项作轻量岗占位；真 HR 编制表就绪后改 `LookupByJob`。

---

## 8. 开放问题（讨论会请逐条表态）

> **已决条目**标 ✅ 并指向 §7.5；其余仍开放。

### 8.1 产品默认

1. 新建用户任务 **默认定范围**：✅ **`ApproverPick`（发送人自选）** + Leave **`Collaborate`**（R6）；项目/模板可覆盖。  
2. 未配置 / 算人为空：✅ **发布前禁止空 Type**（R6）+ **运行时拒绝**（R4）；禁止静默当发起人/管理员；显式 Fallback 除外。

### 8.2 主数据、身份、**单位维度**（场景 4/6/7 的前提）

3. 选人 UI 默认数据源：Sail 资源用户 / 宿主系统用户 / 人事主数据？ → **倾向宿主系统用户**（R3），细目再议  
4. 任务 `AssignedUserId` 存什么：✅ **用户编码（如登录账号）**（R3）；非 GUID/工号默认。  
5. **「单位」到底是什么？** ✅ **独立 Unit\***（R1 修订）；引擎不解释组织树语义，只存 string 三字段。业务含义（法人/班级/项目部…）由该节点 Resolve/宿主填入；**不等于** GroupId。  
6. 单位→人 / 单位→群组 的映射：✅ 主数据与映射在**宿主/业务**；引擎仅 `UnitMap`/`JobRelative` 等骨架 + Lookup 覆盖（R3）；输出填 Unit\*，需要组内抢办时再写 Group\*。

### 8.3 第一维策略分期

7. **P0** 是否接受：固定人(1)、流程相对-发起人/上一发送人(2)、群组(5)、发起人自选(8)、缺省(7 最小版)？  
8. **P1**：岗位相对(3)、单位相对固定人(4)、单位+群组(6)——是否绑定宿主 HR/组织就绪后再做？  
9. **P2 / 扩展**：业务动态(9) 的回调契约形态（HTTP / 规则集 Code / 程序集插件）？  
10. 「审批人自选」（办中选下一跳）是否单列进 P0？口述未强调但办理页已有 Specific 雏形。

### 8.4 第二维办理模式与完成条件

11. P0 是否只做 **抢办 + 协办（扁平集合）**，组内/队长放 P1？（**字段 GroupId / GroupRole 仍建议随 F1 一并扩展，避免二次改载荷**）  
12. **组内抢办 + 多单位自选**（§4.3）引擎落点倾向：  
    - 按 GroupId 拆多实例（每组一 MI 分支）；还是  
    - 显式并行子流程/多用户任务；还是  
    - 先产品上限制「仅支持一种结构」？  
13. 组间默认是否 **全部完成 (AND)**？是否开放 OR？ → ✅ **R7**：And/Or 快捷 + **数量/比例**（`groupCompleteCount`/`groupCompleteRate`）；默认 And。  
14. 协办完成条件产品面：固定人数、比例是否都进 P0？默认值倾向「全员」还是「须显式配置」？  
15. **队长模式**细则：✅ **R8** — 关卡必须办理；其他人也可办理（有待办）；关卡办完后该组完成/可发送；`gateRoles` 默认 `{Leader}`，可配（如含副组长）。  

### 8.5 Client 契约与引擎机制

16. engine 插槽命名：`IAssigneePipeline` 是否采纳？未挂载失败文案是否统一 `UseAssignees()`？ ✅ **已采纳**（S0）  
17. ConfigPair 是否 **仅存在于 assignee**（engine 请求不带 Type，由 Pipeline 读定义）？ ✅ **原则采纳**（实现见引擎 §5.6）  
18. **内置 Type 字符串最小集**是否确认（见 §11.2）？比较是否一律 `OrdinalIgnoreCase`？ ✅ **已落地**（S1–S4 Type 集 + IgnoreCase）  

19. 现有会签 `MultiSignDetail`：✅ **定型后不双轨**（R2）；导入可转写 ConfigPair，运行时只走 LeaveJudge。  
20. `Performer` GroupId/GroupRole 落库位置？ ✅ Group\* 已落任务/活动/视图；**Unit\* 目标态同落**（可空）；实例可选 **StartUnit\***（R1 修订）。  
21. F1：已传列表绝不改写；未传则走节点 Resolve？ → 原则已采纳；与 R4 空结果拒绝一并实施时复核。  
22. `Definition` 只认 Role → ✅ **废止运行时回落**，统一进 assignee Resolve（R2）。  
23. 业务覆盖内置后，是否要日志/诊断标记「Type X 已被覆写」？ → ✅ **强制**（**R9**）

### 8.6 文案与信息架构

24. 属性标题用「待办人配置」还是「参与者」？两套 ConfigPair 是否分两个折叠面板？ → **再议**  
25. 通知人、抄送是否严禁与待办人配置混用同一套 UI？ ✅ **严禁混用**（R5）  
26. UI：「组内身份」下拉是否直接展示 组织者/组长/副组长/成员？ → **再议**

---

## 9. 建议的讨论产出物（仍不写代码）

讨论收敛后，建议只产出：

1. **engine 薄管道 + `sailflow.assignee` 重量包** 落层确认（§11.0）。  
2. **Type=string、后写覆盖、内置非 seal** 三条铁律确认。  
3. **两大时刻契约** + **ConfigPair** + **内置 Type 字符串常量清单**。  
4. **Performer 字段合同**（GroupId / GroupRole）。  
5. **GroupRole / gateRoles**；单位维度；队长细则；空办理人政策。  
6. **BPMN 扩展草图**；实施切片 **§11.7**。  
7. 同步更新 sailflow《架构设计-项目依赖与分层》索引后再开 **S0**。

---

## 10. 附录：现状代码锚点（只读索引）

| 区域 | 路径 |
|------|------|
| Client 注册风格（参照） | `sailflow/.../Client/SlickClient.cs` → `addGate` / `addHook` |
| 设计器参与者 | `web/src/widgets/sailflow/bpmn/provider/sfPerformersProvider.ts` |
| Performer 描述 | `.../bpmn/descriptors/performers.json` |
| 选人对话框 | `.../components/PerformerPickerDialog.vue` |
| 办理选下一办理人 | `.../components/runtime/SailOperationBar.vue` |
| 运行时指定类型 | `sailflow/.../Common/NextPerformerIntTypeEnum.cs` |
| 办理人实体（仅 Id/Name） | `sailflow/.../Common/Performer.cs` ← **待扩 GroupId / GroupRole** |
| 节点算人（角色） | `.../Xpdl/ProcessModelBPMN.cs` → `GetActivityPerformers` / `GetActivityRoles` |
| 角色扩用户 | `.../Common/PerformerBuilder.cs` |
| 写入办理人（时刻 A 落点附近） | `.../Core/Pattern/NodeMediator.cs` → `GenerateActivityAssignedUserInfo` |
| 会签完成（时刻 B 应收口） | 多实例 / `sf:MultiSignDetail` |
| 参照策略枚举 | n_flowable `SimpleProcessDesignerV2/src/consts.ts` → `CandidateStrategy` |
| 分层铁律 | `sailflow/doc/sail/架构设计-项目依赖与分层.md` |

---

## 11. 落地分层与 API 设计草稿（讨论稿）

> 对齐：`doc/sail/架构设计-项目依赖与分层.md`。  
> **形态决议（重量级、改动面大）**：办理人逻辑不进 engine 实现堆；**engine 仅留最薄契约调用**；重量实现落在新建 pure 子项目 **`sailflow.assignee`**。  
> **真实实现 API** 见引擎仓 `sailflow/doc/sail/特性说明-待办人管道.md` **§5.6**（勿在本讨论稿继续堆实现细节）。

### 11.0 形态决议（先读）

| 决议 | 口径 |
|------|------|
| **engine 极薄** | Mediator / 活动管理器只调 `IAssigneePipeline.Resolve…` / `CanLeave…`，**不含** Type 分发、内置策略、Params 解释 |
| **重量在 `sailflow.assignee`** | ConfigPair、注册表、两大时刻处理器、内置 Type 实现、旧 MultiSign/Performers 适配均在此包 |
| **Type = string** | **禁止** C# `enum` 作为分发键；BPMN/JSON/注册一律字符串；内置提供 `static const string` 供比较 |
| **按码注册，后写覆盖** | 同 Type 再次 `use*` → **覆盖前者**；业务可覆写内置实现，不必改 assignee 源码 |
| **内置实现不 seal** | 内置 Resolver/LeaveJudge 类 **禁止 `sealed`**，允许业务继承改局部行为后再按同码注册覆盖 |

依赖方向（避免环）：

```text
biz / 宿主 ──UseAssignees / use*──► sailflow.assignee ──► engine（上下文/Performer/任务语义）
engine 仅持有 IAssigneePipeline 插槽（或等价 Locator），由 assignee 在 Use 时注入实现
```

> `Performer` 扩字段（GroupId/GroupRole）仍属引擎共享模型，可留在 **engine.Common**；assignee 消费并填充。分发与策略逻辑不回流 engine。

### 11.1 分层落位总原则

```text
宿主 / 客户业务程序集
  → useAssigneeResolver / useLeaveJudge（可覆盖内置同名 Type）
sailflow.biz
  → UseBizAssignees()：UseAssignees（挂 pipeline + 注册内置）+ 产品扩展 Type
core/ext（可选）
  → 表单/规则/脚本等扩展 Type 的实现 + UseXxx
core/pure/assignee   ★ 新建 sailflow.assignee
  → 契约细节、ConfigPair、Registry、内置 Type（可继承）、Pipeline 实现
core/pure/engine
  → Performer 字段、时刻 A/B 的一处调用、IAssigneePipeline 插槽（未挂载则明确失败）
```

| 放哪 | 放什么 | 不放什么 |
|------|--------|----------|
| **pure/engine** | `Performer`(+Group*)；**`IAssigneePipeline`**（仅 Resolve/CanLeave 两入口）；Mediator/AIM **一行调用**；未挂载快速失败文案 | Type 注册表、内置策略、Params 解析、会签通过率业务改写 |
| **pure/assignee**（新） | `ConfigPair`；`IAssigneeResolver` / `IActivityLeaveJudge`；`AssigneeTypeCodes`（静态 string）；Registry（后写覆盖）；**非 seal** 内置实现；`AssigneePipeline`；`UseAssignees`；旧 XML 适配 | MQ/脚本/NRules 等重依赖 |
| **ext/** | 依赖表单/规则/HTTP 的扩展 Type | 改 engine 调度 |
| **biz** | `UseBizAssignees` 组合；单位映射等产品 Type | 被 engine 引用 |
| **宿主业务** | 覆写内置 Type，或注册 `Business.*` | fork engine / assignee 内核调度 |
| **FE** | ConfigPair 面板；Type 下拉为 **string** | 判定逻辑 |

口诀：

1. **会频繁改的办理人逻辑** → `sailflow.assignee`，不是 engine。  
2. **engine 只呼叫管道，不认识具体 Type 码表实现。**  
3. **绑 SDK** → ext；**产品默认** → biz；**客户规则** → 业务 `use*`（可覆盖）。

### 11.2 场景 → Type（string）→ 落层

> Type 列均为 **string 字面量**；代码侧用 `AssigneeTypeCodes.Xxx` 常量，**不要**生成 enum。

#### 时刻 A · AssigneeResolve

| 场景 | Type 字符串（建议） | 落层 | Params 要点 |
|------|---------------------|------|-------------|
| F1 / Specific | `"Specific"` | **assignee 内置** | 吃 runner 已传列表 |
| 1 固定人 | `"FixedUsers"` | **assignee 内置** | `users: […]` |
| 2 流程相对 | `"ProcessRelative"` | **assignee 内置** | `relative: Starter\|PrevSender\|…`（值也是 string） |
| 5 群组/角色 | `"RoleGroup"` | **assignee 内置**（调 `IResourceService`） | `roleIds[]` |
| 8 发起人自选 | `"StarterPick"` | **assignee 内置** + FE 采集 | `required`、`allowMultiGroup` |
| 审批人自选 | `"ApproverPick"` | 同上 | 办理页写入 Specific 载荷 |
| 7 缺省 | Params.`onEmpty` 或 `"Fallback"` | **assignee 内置** | `onEmpty: Error\|Suspend\|Resolve` + 可选下一 Type **string** |
| 3 岗位相对 | `"JobRelative"` | **ext/biz** | HR |
| 4/6 单位 | `"UnitMap"` / `"UnitRoleGroup"` | **biz/ext** | 单位维度 |
| 9 业务动态 | `"Business.ClassStudents"` 等 | **业务注册**（可覆盖） | 自定 |
| 表达式/规则 | `"Expression"` / `"RuleSet"` | **ext** | 脚本/规则 Code |

#### 时刻 B · LeaveJudge

| 模式 | Type 字符串 | 落层 | Params 要点 |
|------|-------------|------|-------------|
| 抢办 | `"Race"` | **assignee 内置** | 可选 `claimFirst` |
| 协办 | `"Collaborate"` | **assignee 内置** | `completeCount` / `completeRate`；`merge` |
| 组内抢办 | `"GroupRace"` | **assignee 内置** | `groupJoin` |
| 组内协办 | `"GroupCollaborate"` | **assignee 内置** | 组内 count/rate + `groupJoin` |
| 队长 | `"Captain"` | **assignee 内置** | `gateRoles: ["Leader"]`（string[]） |
| 自定义 | 业务 string | **业务 / ext** | 可覆盖内置同名码 |

业务覆写示例：注册自己的 `"Collaborate"` 实现 → **覆盖** assignee 默认协办，engine 无感。

### 11.3 API 形态草稿（C#）

#### 11.3.1 engine：仅管道契约 + 调用

```csharp
// saiflow.engine — 极薄，不出现具体 Type 分发

/// <summary>办理人运行时管道：由 sailflow.assignee 注入。</summary>
public interface IAssigneePipeline
{
    PerformerList ResolveForTaskCreate(AssigneeInvokeRequest request);
    LeaveJudgeResult CanLeave(LeaveInvokeRequest request);
}

// NodeMediator / ActivityInstanceManager 内：
//   var pipe = Sail.GetAssigneePipelineRequired(); // 未挂载则提示 UseAssignees()
//   var performers = pipe.ResolveForTaskCreate(...);
//   if (!pipe.CanLeave(...).Allow) { /* 不流出 */ }
```

`AssigneeInvokeRequest` / `LeaveInvokeRequest` 可放 engine（最小 DTO：节点 Id、实例 Id、runner、**已解析的 ConfigPair 或原始扩展 XML 字符串**）。若希望 engine 完全不认识 ConfigPair，则请求里只带 `activityId`，由 Pipeline 自己读定义——**推荐后者**，engine 更干净。

#### 11.3.2 assignee：ConfigPair、Type 常量、处理器（禁 enum / 禁 seal 内置）

```csharp
// saiflow.assignee

public class ConfigPair   // 不必 seal；便于测试派生
{
    public string Type { get; set; }                      // 必须是 string
    public IDictionary<string, object> Params { get; set; }
}

/// <summary>内置 Type 码 —— 静态 string，禁止改成 enum。</summary>
public static class AssigneeResolveTypeCodes
{
    public const string Specific = "Specific";
    public const string FixedUsers = "FixedUsers";
    public const string ProcessRelative = "ProcessRelative";
    public const string RoleGroup = "RoleGroup";
    public const string StarterPick = "StarterPick";
    public const string ApproverPick = "ApproverPick";
    public const string Fallback = "Fallback";
}

public static class LeaveJudgeTypeCodes
{
    public const string Race = "Race";
    public const string Collaborate = "Collaborate";
    public const string GroupRace = "GroupRace";
    public const string GroupCollaborate = "GroupCollaborate";
    public const string Captain = "Captain";
}

public static class GroupRoles
{
    public const string Organizer = "Organizer";
    public const string Leader = "Leader";
    public const string DeputyLeader = "DeputyLeader";
    public const string Member = "Member";
}

public interface IAssigneeResolver
{
    string Type { get; }   // string，与 ConfigPair.Type 比较（建议OrdinalIgnoreCase）
    PerformerList Resolve(AssigneeResolveContext ctx);
}

public interface IActivityLeaveJudge
{
    string Type { get; }
    LeaveJudgeResult CanLeave(LeaveJudgeContext ctx);
}

// 内置实现：禁止 sealed，留继承活口
public class SpecificAssigneeResolver : IAssigneeResolver { /* … */ }
public class CollaborateLeaveJudge : IActivityLeaveJudge { /* … */ }
// 业务可：class MyCollaborate : CollaborateLeaveJudge { … } 再 useLeaveJudge 同码覆盖
```

#### 11.3.3 注册表：仅按 string 码；后注册覆盖

```csharp
public sealed class AssigneeCapabilityRegistry
{
    private readonly Dictionary<string, IAssigneeResolver> _resolvers =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IActivityLeaveJudge> _judges =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(IAssigneeResolver resolver)
    {
        // 后写覆盖前者 —— 业务覆写内置的正式途径
        _resolvers[resolver.Type] = resolver;
    }

    public void Register(IActivityLeaveJudge judge)
    {
        _judges[judge.Type] = judge;
    }

    public IAssigneeResolver GetResolver(string type) =>
        _resolvers.TryGetValue(type, out var r) ? r
        : throw new InvalidOperationException($"No IAssigneeResolver for type '{type}'.");

    public IActivityLeaveJudge GetLeaveJudge(string type) =>
        _judges.TryGetValue(type, out var j) ? j
        : throw new InvalidOperationException($"No IActivityLeaveJudge for type '{type}'.");
}
```

```csharp
// SailClient 扩展（定义在 assignee 包）
public static SailClient useAssigneeResolver(this SailClient c, IAssigneeResolver r);
public static SailClient useLeaveJudge(this SailClient c, IActivityLeaveJudge j);
// 委托重载：useAssigneeResolver(string type, Func<…>) 内部包装，同样后写覆盖

/// <summary>注入 Pipeline 到 engine 插槽，并注册全部内置（非 seal）实现。</summary>
public static SailClient UseAssignees(this SailClient c);
```

`UseAssignees` 顺序建议：先注册内置 → 再由 `UseBizAssignees` / 宿主 `use*` 覆盖。  
**不要**在注册重复时抛错（已决议：**覆盖**）。

#### 11.3.4 biz / 业务

```csharp
// biz
public static SailClientBuilder UseBizAssignees(this SailClientBuilder b)
{
    b.UseAssignees();           // pipeline + 内置
    // b.useAssigneeResolver(new UnitMapResolver()); // 产品扩展
    return b;
}

// 宿主：覆盖内置 Collaborate，或加新码
sail.useLeaveJudge(new TenantCollaborateLeaveJudge()); // Type 仍为 "Collaborate"
sail.useAssigneeResolver(new ClassStudentsResolver());  // Type = "Business.ClassStudents"
```

### 11.4 BPMN / Meta 扩展草稿

`type` 属性 **永远是字符串**（与常量相等即可）：

```xml
<sf:AssigneeResolve type="StarterPick">
  <sf:param name="required" value="true" />
</sf:AssigneeResolve>
<sf:LeaveJudge type="GroupRace">
  <sf:param name="groupJoin" value="And" />
</sf:LeaveJudge>
```

```text
sf:assigneeResolve = {"type":"RoleGroup","params":{"roleIds":["R_MGR"]}}
sf:leaveJudge      = {"type":"Collaborate","params":{"completeRate":0.6}}
```

旧 `sf:Performers` / `sf:MultiSignDetail` → **assignee 包内适配器**转成 ConfigPair；engine 不读旧结构。

### 11.5 引擎调用点（仅薄调用）

| 时机 | 今日锚点 | engine 内目标代码形态 |
|------|----------|------------------------|
| **时刻 A** | `GenerateActivityAssignedUserInfo` 等 | `pipeline.ResolveForTaskCreate(req)` → 用返回列表建任务；**删除** Role-only 硬编码分支 |
| **时刻 B** | `CheckPassRateMatchedInfo` 等 | `pipeline.CanLeave(req)`；Allow 才转移 |
| 未挂载 | — | `GetAssigneePipelineRequired()` 提示调用 `UseAssignees()` |

Type 查找、Params、组完成、队长关卡 —— **全部在 assignee.Pipeline 内**完成。

### 11.6 前端 / Host API（壳层）

| 能力 | 落点 | 说明 |
|------|------|------|
| 设计器 ConfigPair | FE provider | Type 下拉值为 **string**；允许自定义字符串 |
| Type 元数据 | Host 聚合 registry | 返回已注册码（含被覆盖后的最终实现描述，若实现自报） |
| 选人矩阵 | 发起/办理 + runner | `Performer[]` 含 groupId / groupRole |
| 预览算人 | Host → Pipeline | 不经过改 engine |

### 11.7 建议实施切片

| 切片 | 内容 | 层 |
|------|------|----|
| **S0** | 新建 `sailflow.assignee`；`IAssigneePipeline` 插槽；Mediator 薄调用；Registry（覆盖语义）；`UseAssignees` 空心 | engine 插槽 + assignee 骨架 |
| **S1** | Performer 扩字段；内置 Resolve/Leave（**非 seal**）+ Type **string 常量**；旧会签适配 | assignee |
| **S2** | Group* / Captain + 落库还原组/身份 | assignee |
| **S3** | BPMN ConfigPair 解析；FE 双面板；发起/办理 Group* 矩阵 | engine convert + assignee Adapt + FE | ✅ |
| **S4** | UnitMap / JobRelative / BusinessCallback / Expression / Fallback；biz 覆写样例 | assignee + biz | ✅ |

### 11.8 一页总图

```text
[设计器 type:string + params] ──► BPMN
[发起/办理 Performer[]] ──► FlowCommandContext
                │
                ▼
        engine（薄）
        IAssigneePipeline.Resolve / CanLeave
                │ 插槽注入
                ▼
        sailflow.assignee
        Registry[type:string] ──后写覆盖──► 业务 use*
                │
                ├── 内置（可继承，非 seal）：Specific / Race / Collaborate / …
                ├── ext Type：Expression / RuleSet / …
                └── 业务 Type 或 同码覆写
```

### 11.9 项目文件落位（草案）

```text
src/core/pure/assignee/
  sailflow.assignee.csproj     → PackageId: saiflow.assignee
  ProjectReference → sailflow.engine
  src/
    ConfigPair.cs
    AssigneeTypeCodes.cs       // Resolve + Leave 静态 string
    GroupRoles.cs
    Abstractions/              // IAssigneeResolver, IActivityLeaveJudge, contexts
    Registry/AssigneeCapabilityRegistry.cs
    Pipeline/AssigneePipeline.cs   // : IAssigneePipeline
    Builtin/Resolvers/         // 非 sealed
    Builtin/LeaveJudges/       // 非 sealed
    Hosting/SailAssigneeExtensions.cs  // UseAssignees, useAssigneeResolver, …
    Adapt/LegacyMultiSignAdapter.cs
```

解决方案文件夹 **core** 增加 `assignee`；`sailflow.biz` 引用 `assignee` 并 `UseBizAssignees`。

> 已落地目录与签名以 `特性说明-待办人管道.md` §5.6 / §5.6.8 为准。  
> **定型决议**以引擎文档 **§0 R1–R5** 与本文 **§7.5** 为准；代码双轨收口 / 缺配置拒绝 / 宿主 Org 接入为后续实施项。

---

**下一篇文档动作**：实现侧细节维护在引擎 `特性说明-待办人管道.md`；本讨论稿保留场景与决议溯源；**再议**项见 §8（文案等）。
