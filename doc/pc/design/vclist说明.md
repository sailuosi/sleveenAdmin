# vclist 说明

> **状态**：vclist **唯一**权威说明（查询管道、最大化工具栏、表格/卡片双视图）  
> **目录**：`doc/pc/design/`  
> **组件**：`web/src/ui/common/vclist/vclist.vue`  
> **相关**：代码生成覆盖区见 [代码生成规范与融合.md](./代码生成规范与融合.md)；模块范式见 [biz-module-paradigm.md](./biz-module-paradigm.md)；mooSQL 见 [mooSQL-integration.md](../../mooSQL-integration.md)。  
> **合并自**：原《列表查询API说明》《vclist列表卡片模式改造规范》，并补充最大化与工具栏 Teleport。  
> **说明**：标准管道为 vclist `conditions` + `PageQueryDto` → mooSQL `QueryPara`；简化 `BizQueryInput` 见 §1.4 注。

---

## 目录

1. [查询管道（PageQueryDto / qlist）](#1-查询管道pagequerydto--qlist)
2. [最大化与工具栏 Teleport](#2-最大化与工具栏-teleport)
3. [表格 / 卡片双视图](#3-表格--卡片双视图)

---

## 1. 查询管道（PageQueryDto / qlist）

### 1.1 目标与边界

列表页的查询不是「前端自己拼 SQL」，而是一条固定管道：

```
VC 组装 PageQueryDto
  → POST biz/{slug}/…/qlist
  → input.ToQueryPara()
  → para.OnBuildSQL（权限 / 主从表 / 默认排序）
  → BizService.LoadPageList(para)
  → SooRepository.GetPageList(para)
  → EntityTranslator.PatchSQLByQueryPara
  → PageOutput<T> { Items, Total }
```

本文约定这条管道上的**请求体、操作符、排序优先级、前端条件汇合顺序**。个性化业务条件优先走 `conditions`；无法用动态条件表达的（权限、软删、主从表外键）走 Controller `OnBuildSQL` 或 Service `whenQueryWhere` / `WhenQueryWhere`。

#### 适用对象

| 层 | 典型文件 | 职责 |
|---|---|---|
| 前端列表 | `VC_*List.vue` + `vclist` | 组装分页、列筛选、工具栏/卡片筛选、排序 |
| 前端 API | `Biz_*api.js` | `POST …/qlist`，body 为 `PageQueryDto`（或兼容袋） |
| Biz Controller | `Biz_*Controller.cs` | `ToQueryPara` + `OnBuildSQL` + `LoadPageList` |
| Biz Service | `Biz_*Service.LoadPageList` | `prepareQuery<T>(para).GetPageList(para)` |
| mooSQL | `QueryPara` / `SooRepository` | 把 `conditions` / `orderBy` 转成 WHERE / ORDER BY |

#### 非目标

- 不替代复杂报表 SQL；跨表聚合、OR 组、子查询仍用 `OnBuildSQL`。
- 前端 **不得** 把自由 SQL 片段放进 `conditions`（仓储会按实体字段白名单匹配，未知字段直接忽略）。

### 1.2 端到端数据流

```
┌──────────── VC_*List.vue ────────────┐
│ queryParams.pageSize / pageNum       │
│ queryParams.orderBys / orderByColumn │
│ bus.conditions  （树节点、父 VC）     │
│ props.conditions（Page / 父组件）     │
│ 工具栏自定义条件（类别、年份…）        │
│ queryRowCondtions（列头/卡片筛选）    │
│ queryTxt / beforeLoad                │
└─────────────────┬────────────────────┘
                  │ POST JSON
┌─────────────────▼────────────────────┐
│ PageQueryDto                         │
│  pageSize, pageNum                   │
│  conditions: QueryCondition[]        │
│  orderBys / orderByColumn + isAsc    │
│  masterFK, dict                      │
└─────────────────┬────────────────────┘
                  │ ToQueryPara()
┌─────────────────▼────────────────────┐
│ QueryPara                            │
│  conditions → WHERE（字段须在实体中） │
│  orderBy    → ORDER BY               │
│  OnBuildSQL → 权限 / 外键 / 默认排序  │
│  suckWheres → 仅服务端附加（不接 Web）│
└──────────────────────────────────────┘
```

返回：

```json
{
  "Items": [ { "...": "行对象" } ],
  "Total": 123,
  "PageSize": 30,
  "PageNum": 1,
  "Summary": null
}
```

分页缺省（仓储 `CheckQueryPara`）：`pageNum<=0` → 1；`pageSize<=0` → 50。前端列表默认常用 `pageSize: 30`。

### 1.3 mooSQL 仓储动态条件

#### QueryPara

定义：`mooSQL.data.QueryPara`（`pure/src/adoext/dto/QueryPara.cs`）。

| 字段 | 来源 | 说明 |
|---|---|---|
| `pageSize` / `pageNum` | 前端 | 分页；仓储会校正缺省 |
| `conditions` | 前端 + 可服务端补 | 动态 WHERE |
| `orderBy` | `PageQueryDto.ToQueryPara` | 动态 ORDER BY |
| `sumFields` | 较少用 | 汇总列 `sum/avg/min/max/count` |
| `suckWheres` | **仅服务端** `SuckWhere(...)` | `[NonSerialized]`，禁止接口赋值 |
| `OnBuildSQL` | **仅服务端** | 多轮钩子，写 `kit.where` / `kit.orderBy` |

仓储入口：

```csharp
repo.GetPageList(para);   // 分页，无排序时补主键 DESC
repo.GetList(para);       // 不分页
repo.GetPageData(para);   // DataTable
```

Biz 标准写法：

```csharp
public PageOutput<BizSaEbNoteRow> LoadPageList(QueryPara para)
{
    var db = this.prepareQuery<BizSaEbNoteRow>(para);
    return db.GetPageList(para);
}
```

`prepareQuery` 会把 Biz 的 `whenQueryWhere` 挂到转译器（常见：软删 `SYS_Deleted`）。

#### QueryCondition

```json
{ "field": "En_Title", "op": "like", "value": "笔记" }
```

| 属性 | 必填 | 说明 |
|---|---|---|
| `field` | 是 | 实体列名或 Join/Free 的 **Alias**（如 `cataName`、`Ec_Code`） |
| `op` | 是 | 见操作符表；空则忽略该条 |
| `value` | 是 | `null` / 空则忽略该条（本条 `where` 不写；但同条上的 `sink`/`rise` 仍会执行，见下） |
| `vtype` | 否 | 预留，当前转译器**未使用** |
| `sink` | 否 | 条件组**开启**，对应 `SQLBuilder`；见下表 |
| `rise` | 否 | 条件组**关闭**；`true` 时调用 `kit.rise()`，否则不执行 |

转译顺序（`EntityTranslator.PatchSQLByQueryPara`，每条 `conditions` 项）：

1. 若 `sink` 有效 → 先开分组  
2. 再按 `field`/`op`/`value` 写本条 WHERE（字段不匹配或 `op`/`value` 无效则跳过本条 where）  
3. 若 `rise === true` → `kit.rise()` 关闭当前分组  

#### 条件组（sink / rise ↔ SQLBuilder）

`sink` / `rise` 直接映射到 `SQLBuilder` 的分组 API（`SinkType` 枚举）：

| `sink` 值 | 枚举 | 等价调用 | 语义 |
|---|---|---|---|
| `0` / `null` / 未传 | `None` | （不调用） | **不开启**分组 |
| `1` | `And` | `kit.sink()` | 开启 **AND** 分组（默认组内连接为 AND） |
| `2` | `Or` | `kit.sinkOR()` | 开启 **OR** 分组（组内连接为 OR） |
| `3` | `AndNot` | `kit.sink("AND NOT")` | 较少用 |
| `4` | `OrNot` | `kit.sink("OR NOT")` | 较少用 |
| `5` | `NotAnd` | `kit.sinkNot()` | 较少用 |
| `6` | `NotOr` | `kit.sinkNotOR()` | 较少用 |

| `rise` 值 | 等价调用 | 语义 |
|---|---|---|
| `true` | `kit.rise()` | **关闭**当前分组，回到上一层 |
| `false` / `null` / 未传 | （不调用） | 保持在当前分组内 |

列表查询里常用 **`0 / 1 / 2`**：不分组、`sink()`、`sinkOR()`。`sink` 挂在**分组第一条**条件上，`rise: true` 挂在**分组最后一条**上。

关键字跨字段 **OR** 模糊（典型写法，在 **`getlist` 里直接 push**，不要走 `fastFilter`）：

```javascript
// kit.sinkOR() … kit.rise()  →  (En_Title LIKE t OR En_Summary LIKE t)
if (queryTxt.value) {
  const t = String(queryTxt.value).trim();
  if (t) {
    para.conditions.push({ sink: 2, field: "En_Title", op: "like", value: t });
    para.conditions.push({ rise: true, field: "En_Summary", op: "like", value: t });
  }
}
```

卡片搜索框：`queryShowSearch` + `@search` 写入 `queryTxt` 再调 `getlist`。

**`fastFilter` 适用边界**（与上不同）：

| 场景 | 做法 |
|---|---|
| 单字段 like | `:fast-filter` 返回 `[{ field, op:'like', value }]`，写入 `queryConditions` |
| 多字段 **AND** | `:fast-filter` 返回多条普通条件（无 sink/rise OR 组） |
| 多字段 **OR**（sink/rise） | **不用** `fastFilter`，在 `getlist` 中追加条件 |

注意：

- `sink` 开组、本条 where、`rise` 闭组按条顺序执行；`sink` 放分组第一条，`rise: true` 放最后一条。
- `value` 无效导致本条 where 被跳过时，同条上的 `sink`/`rise` **仍会执行**；清空关键字时不要只置空 value，应整组不 push。
- 也可写在 Controller `OnBuildSQL`（`kit.sink()` / `kit.sinkOR()` / `kit.rise()`）或 `para.SuckWhere(...)`。

**字段白名单**：`EntityTranslator.MatchQueryField` 只认当前查询实体：

| 列 Kind | 匹配键 | 写入 SQL |
|---|---|---|
| Base | `DbColumnName` 或 `Alias` | `{表别名}.{DbColumnName}`，主表别名常为 `mtb` |
| Join | `Alias` | `SrcTable.SrcField` 或 `SrcField`（已带别名时） |
| Free | `Alias` | `FreeSQL` 表达式 |

匹配失败 → **静默跳过**，不会报错。列头 `field` 必须与 `Biz_*Row` 属性/Alias 一致。

#### 操作符

比较符大小写敏感地走简单分支（须原样 `=`、`<>`、`>`、`<`、`>=`、`<=`）。其余先 `Trim().ToLower()` 再分发。

| `op`（前端可写） | 后端识别 | 值形态 | SQL 语义 |
|---|---|---|---|
| `=` `<>` `>` `<` `>=` `<=` | 原样 | 标量 | 比较 |
| `like` | `like` | 字符串 | 包含（仓储包 `%`） |
| `likeLeft` / `likeleft` | `likeleft` | 字符串 | 前缀 `LIKE 'v%'` |
| `notlike` | `notlike` | 字符串 | 不包含 |
| `likes` | `likes` | 字符串（`[,;|]` 分隔）或字符串数组 | 多值 OR LIKE |
| `likelefts` | `likelefts` | 同上 | 多值前缀 |
| `in` | `in` | 字符串分隔或数组 | `IN (...)` |
| `notin` | `notin` | 同上 | `NOT IN` |
| `between` | `between` | `"start,end"` 或长度≥2 的数组 | 区间 |
| `isnull` | `isnull` | 任意非空占位即可 | `IS NULL` |
| `notnull` | `notnull` | 同上 | `IS NOT NULL` |

分隔符：`in` / `notin` / `likes` / `likelefts` / `between` 对字符串值按 **`,` `;` `|`** 切开。

`vclist.fmtAPIConditions` 会把数组 `value` **join 成逗号串** 再提交，因此：

- 日期范围：`op: "between"`，`value: ["2026-01-01", "2026-01-31"]` → `"2026-01-01,2026-01-31"`
- 代码表多选：应使用 `op: "in"`，不要用 `=`（`=` 会把 `"1,2"` 当成单个等值）

#### 排序

`QueryOrderBy`：`{ field, order, idx }`。`order` 仅认 `desc`（大小写不敏感），其余当升序。`idx` 决定多列顺序。

`PatchSQLByQueryPara` 中 ORDER BY 叠加顺序：

1. **`OnBuildSQL` 里 `kit.orderBy(...)`**（最先写入 builder）
2. **`para.orderBy`**（前端列头排序，按 `idx`）
3. **实体元数据** `SooColumn` 的 `Desc`/`Asc`（已出现的列跳过）
4. **仓储兜底** `EnsureDefaultOrderByPk`：若仍无排序，主键 DESC（分页方言需要 ORDER BY）

因此默认排序钩子必须先判断前端是否已带 `orderBy`：

```csharp
if (para.orderBy != null && para.orderBy.Count > 0) return;
kit.orderBy("mtb.SYS_Created desc");
```

`PageQueryDto.ToQueryPara` 还会：

- `orderBys` 为空且 `orderByColumn` 为简单标识符时，用 `orderByColumn` + `isAsc`（`desc` 或默认 `asc`）补一条；
- 按**列名最后一段**去重（`mtb.En_Title` 与 `En_Title` 视为同一列），避免 SQL Server 重复排序列报错。

### 1.4 标准 Biz 列表接口

#### HTTP 约定

| 项 | 约定 |
|---|---|
| 方法 | `POST` |
| 路由 | `[Route("biz/{slug}/")]` + `[HttpPost("{mod}/{entity}/qlist")]` |
| 入参 | `[FromBody] PageQueryDto input`（标准动态条件管道） |
| 出参 | `PageOutput<Biz*Row>` |
| 前端函数 | `queryBiz{实体}list(para)`，例如 `queryBizSaEbNotelist` |

`PageQueryDto`（平台列表查询 DTO；`ToQueryPara()` → mooSQL `QueryPara`）：

| 字段 | 对应前端 | 说明 |
|---|---|---|
| `pageSize` / `pageNum` | `queryParams` | 分页 |
| `conditions` | 汇合后的条件数组 | 动态筛选（§1.3） |
| `orderBys` | `queryParams.orderBys` | 推荐 |
| `orderByColumn` / `isAsc` | 列头排序兼容字段 | `isAsc` 取 `asc`/`desc` |
| `masterFK` | `props.fk` | 子表外键，**不会**自动进 WHERE |
| `dict` | `props.params` | 扩展袋，Controller 用 `TryGetString` 读取 |

> **与 `BizQueryInput` 的关系**：部分早期 Biz 列表走简化入参 `BizQueryInput`（`Keyword` / `Filters` + `BizServiceBase.LoadPageList`），**不经过** `PatchSQLByQueryPara`。vclist 列头 / 卡片筛选发出的仍是 `conditions[]`；要让列头 `like`/`in`/`between`/`sink`/`rise` 全能力生效，Service 应按本节接 `PageQueryDto` → `QueryPara` → `GetPageList`（或在基类中把 `Conditions` 转译进 builder）。简化路径仅适合 Keyword + 少量 Filters 的列表。

`masterFK` 必须在 `OnBuildSQL` 显式落地：

```csharp
kit.whereGuid("mtb.Tm_TopicOID", input.masterFK);
```

#### 生成骨架（覆盖区内）

```csharp
[HttpPost("ebook/ebnote/qlist")]
public PageOutput<BizSaEbNoteRow> QueryList([FromBody] PageQueryDto input)
{
    var para = input.ToQueryPara();
    para.OnBuildSQL((kit) =>
    {
        // 权限 / 软删补充 / masterFK / 默认可跳过的排序
        // 业务固定条件写在覆盖区外的钩子，或此处保持生成器形态
        if (para.orderBy == null || para.orderBy.Count == 0)
            kit.orderBy("mtb.SYS_Created desc");
    });
    var res = _noteService.LoadPageList(para);
    return this.OnAfterLoadList(_noteService, res);
}
```

个性化过滤优先：

1. 能用实体字段表达 → 让前端 `conditions` 带过来，Controller 不要再手写一遍；
2. 权限 / 软删 / 主从表 → `OnBuildSQL` 或 Service `whenQueryWhere`；
3. 默认排序 → `OnBuildSQL` 且 **用户已排序则跳过**。

前端 API 对应：

```javascript
export function queryBizSaEbNotelist(para) {
    return request({
        url: '/biz/saebnotelist/ebook/ebnote/qlist',
        method: 'post',
        data: para
    });
}
```

### 1.5 VC 列表如何支持查询

#### vclist 查询相关属性

| 属性 | 作用 |
|---|---|
| `query` | 为 true 时，列头出现筛选框（`vccol` 未单独设 `query` 则跟随此值） |
| `v-model:queryConditions` | 列头/卡片筛选结果，即 `queryRowCondtions` |
| `:para="queryParams"` | 分页 + 排序；列头排序写回 `orderBys` |
| `@query="getlist"` | 分页、排序、列筛选触发加载 |
| `queryShowSearch` | 卡片模式右侧快捷搜索框 |
| `fastFilter` | `(text) => QueryCondition[]`：仅用于**单字段**模糊，或**多字段 AND**；返回条件写入 `queryConditions`。多字段 **OR** 不要用，改在 `getlist` 里 push `sink`/`rise` |
| `queryFastFields` | 卡片筛选条字段（与 `fastFilter` / 搜索框关键字不同） |
| `queryBarLabel` | 卡片筛选条标题 |

列头控件类型跟 `vccol` 的 `type`：`text`/`number` 默认 `like`；`dict`/`bool` 默认 `=`；`date`/`datetime` 列头与卡片 `fastbar` 默认都是 `between`，结束日补到当天 `23:59:59`。

列头漏斗可选 op：`like`、`likeLeft`、`=`、`<>`、`>`、`<`、`>=`、`<=`。没有 `in` / `isnull` 选项；日期列头会自动发 `between`，其它 `in`/`isnull` 仍须手写 `conditions` 或改 `queryFastFields.op`。

#### 标准 listpara / getlist

生成列表的汇合顺序（后者 concat 到前者之后，AND）：

```javascript
const listpara = computed(() => {
    const res = {
        masterFK: props.fk,
        conditions: [],
        pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum,
        orderByColumn: queryParams.value.orderByColumn,
        isAsc: queryParams.value.isAsc,
        orderBys: queryParams.value.orderBys,
        dict: props.params,
    };
    if (Array.isArray(busData.value?.conditions)) {
        res.conditions = res.conditions.concat(busData.value.conditions);
    }
    if (props.conditions) {
        res.conditions = res.conditions.concat(props.conditions);
    }
    return res;
});

const getlist = () => {
    let para = Object.assign({}, listpara.value);
    if (Array.isArray(queryRowCondtions.value)) {
        para.conditions = para.conditions.concat(queryRowCondtions.value);
    }
    if (typeof props.beforeLoad === "function") {
        para = props.beforeLoad(para);
    }
    props.api.queryBizXxxlist(para).then((res) => {
        mlist.value = res.Items;
        total.value = res.Total;
    });
};
```

**条件层（从稳到变）：**

| 层 | 来源 | 典型用途 |
|---|---|---|
| 1 | `bus.dataMap[VCName].conditions` | 左树点选组织、父列表驱动 |
| 2 | `props.conditions` | Page / 父组件固定过滤 |
| 3 | 工具栏本地状态 | 状态、分类、关键字 |
| 4 | `queryRowCondtions` | 列头筛选、卡片 `queryFastFields` |
| 5 | `queryTxt` / `beforeLoad` | 卡片搜索框、加载前改写 |

改 `bus.conditions` 后须调 `state.refresh()`（即 `getlist`）。`pageDataBus.initVCList` 会注册 `refresh`。

#### 列头排序

`vclist` 在 `@sort-change` 时写入：

```javascript
queryParams.orderByColumn = field;
queryParams.isAsc = "asc" | "desc";
queryParams.orderBys = [{ field, order: "asc"|"desc" }];
```

取消排序则恢复初次捕获的默认 `orderBys`。`vccol` 的 `field` / `sortField` 同样必须是实体可匹配字段。

### 1.6 常见过滤写法

#### 列头筛选（推荐默认）

```vue
<vclist query v-model:queryConditions="queryRowCondtions" :para="queryParams" @query="getlist">
    <vccol field="En_Title" caption="标题" type="text" />
    <vccol field="En_Status" caption="状态" type="dict" :dictlist="codeTables.En_Status" />
    <vccol field="SYS_Created" caption="创建时间" type="date" />
</vclist>
```

#### 工具栏等值 / 文本

```javascript
res.conditions.push({ field: "En_Status", op: "=", value: String(status) });
res.conditions.push({ field: "En_Title", op: "like", value: keyword });
```

#### 分类树前缀（层次码）

```javascript
state.conditions = [{
    field: "Ec_Code",
    op: "likeLeft",
    value: record.Ec_Code,
}];
state.refresh();
```

#### 卡片筛选条

```vue
<vclist
    :face="mface"
    :queryShowSearch="mface === 'card'"
    queryBarLabel="笔记索引"
    querySearchPlaceholder="输入标题搜索"
    :queryFastFields="['En_Title', 'En_Status', 'cataName']"
    @search="onQueryBarSearch"
/>
```

`queryFastFields` 也可用对象指定 op：

```javascript
[
    { field: "En_Status", label: "状态", ctype: "dict", dictlist: opts, op: "=" },
    { field: "En_Title", label: "标题", op: "like" },
]
```

卡片搜索框 `@search` **不会**自动变 `conditions`，需在 `getlist` 里追加，例如 `{ field: "En_Title", op: "like", value: queryTxt }`。

#### 日期区间 / 多选 IN

```javascript
para.conditions.push({
    field: "SYS_Created",
    op: "between",
    value: [start, end],   // 提交时会被 join 成 "start,end"
});
para.conditions.push({
    field: "En_Status",
    op: "in",
    value: ["draft", "done"],     // 提交成 "draft,done"
});
```

#### 子表 masterFK

VC 把 `masterFK: props.fk` 放进 body；Controller 必须 `kit.whereGuid("mtb.{父表}_FK", input.masterFK)`。不要指望仓储自动按 `masterFK` 过滤。

#### 仅服务端条件

权限、`SYS_Deleted`、跨表存在性：写 `OnBuildSQL` / `whenQueryWhere`，不要做进 `conditions`（前端可伪造）。

### 1.7 能力对照与陷阱

| 能力 | mooSQL | 标准 Biz | 标准 VC / vclist |
|---|---|---|---|
| 等值 / 比较 | 有 | 透传 | 列头 + 手写 |
| like / likeLeft | 有 | 透传 | 列头有；树过滤常用 likeLeft |
| in / notin / between | 有 | 透传 | `in`/`isnull` 须手写；日期列头/fastbar 自动 `between` |
| isnull / notnull | 有 | 透传 | 须手写 |
| OR 条件组 | sink/rise → sink()/sinkOR()/rise() | 透传 | 可手写；列头默认不发 |
| 字段白名单 | 按实体 | 依赖 Biz Row | 未知 field 静默无效 |
| 多列排序 | orderBy[] | ToQueryPara 去重 | 列头一次一列，写入 orderBys |
| 默认排序 | 实体列 + PK | OnBuildSQL 需避让用户排序 | 取消排序回退初始 orderBys |
| 汇总 | sumFields | 少用 | 无标准 UI |
| 自由 SQL 条件 | SuckWhere / OnBuildSQL | 允许 | **禁止**塞进 conditions |

常见踩坑：

1. **筛了没效果**：`field` 不是 `Biz_*Row` 的列名/Alias（例如写了表前缀 `mtb.En_Title`，Base 列匹配的是无前缀名）；或 Service 仍走简化 `BizQueryInput` 未转译 `conditions`。
2. **字典多选无效**：列头默认 `=`，数组被 join 成 `"1,2"` 后按等值比。多选改 `in`。
3. **日期列头不准**：列头与卡片 fastbar 须发 `between`；检查结束日是否被截成 `00:00:00`。
4. **默认排序盖住用户排序**：`OnBuildSQL` 无条件 `kit.orderBy` 会排在用户排序之前。先判断 `para.orderBy`。
5. **重复 ORDER BY 列**：同一列带不同别名；已由 `ToQueryPara` 按列名去重，自定义 `kit.orderBy` 仍可能冲突。
6. **把筛选写进覆盖区 SQL**：能 `conditions` 表达的不要改 `LoadPageList` 骨架。

### 1.8 新增列表检查清单（查询）

- [ ] Controller：`[FromBody] PageQueryDto` → `ToQueryPara()` → `LoadPageList` → `OnAfterLoadList`
- [ ] `OnBuildSQL` 只放权限 / 软删补充 / `masterFK` / 可跳过的默认排序
- [ ] 前端 `queryBizXxxlist` POST 同一 `qlist`
- [ ] `vclist` 绑 `para`、`queryConditions`、`@query="getlist"`
- [ ] `listpara` 按 bus → props → 工具栏 汇合；`getlist` 再 concat 列筛选
- [ ] 列 `field` 与 Row Alias 一致；Join 列用 Alias（`cataName` 而非 `cata.Ec_Name`）
- [ ] 文本 `like`，编码前缀 `likeLeft`，字典单值 `=`，字典多选 `in`，日期区间 `between`
- [ ] 子表传递并消费 `masterFK`
- [ ] 不把用户输入拼进 `kit.where("...")` 字符串

---

## 2. 最大化与工具栏 Teleport

> 样本：`VC_Sa_EbTopicList.vue`（专题材料）。  
> 目标：普通态工具栏挂到 Page 节标题旁；最大化后同一套工具栏挂到弹窗标题栏（`#maxtop`）。

### 2.1 两层传送分工

```
┌─ Page（如 Biz_Sa_EbTopicList）────────────────────────────┐
│  <div ref="VC_xxxHead" class="sect-toolbar"></div>      │  ← 普通态目标
│  <VC_xxxList :position="VC_xxxHead" ... />              │
└─────────────────────────────────────────────────────────┘

┌─ VC_*List.vue ──────────────────────────────────────────┐
│  flexpan #header                                        │
│    <teleport :to="toolbarPlaced" :disabled="!toolbarPlaced">
│      .vc-top-toolbar  （按钮组）                          │
│    </teleport>                                          │
│  flexpan #default                                       │
│    <vclist maxable v-model:maxed="maxed" ...>           │
│      <template #maxtop>                                 │
│        <div ref="maxlistTopRef"></div>  ← 最大化态目标   │
│      </template>                                        │
│    </vclist>                                            │
└─────────────────────────────────────────────────────────┘

┌─ vclist.vue 内部 ───────────────────────────────────────┐
│  openTomodal:                                           │
│    1. seemodal = true   → 弹出 modal，渲染 #header/#maxtop │
│    2. nextTick → openTele                               │
│       modalRoot = 弹窗 body 内容器                        │
│       teleported = true  → v-model:maxed 同步为 true      │
│  列表主体：<teleport :to="modalRoot" :disabled="!teleported"> │
│  弹窗标题：modal #header ← slot #maxtop                   │
│  弹窗底栏：modal #footer ← slot #maxfoot（分页/还原）      │
└─────────────────────────────────────────────────────────┘
```

| 层 | 谁传送 | 目标 | 开关 |
|---|---|---|---|
| **列表主体** | `vclist` 内置 Teleport | 弹窗 `modal-body` 内 `ref="groot"` | `v-model:maxed` / `teleported` |
| **工具栏按钮** | ListVC 自己的 Teleport | 普通：`props.position`；最大化：`#maxtop` 内锚点 | `toolbarPlaced` 有值则启用 |

二者独立：`vclist` 只保证列表进弹窗、`#maxtop` 进标题栏；**工具栏不会自动过去**，须 ListVC 再传一次。

### 2.2 时序

```
用户点最大化
  → vclist.openTomodal()
  → seemodal = true          // modal 挂载，#maxtop 中 maxlistTopRef 得到 DOM
  → nextTick
  → openTele()
      modalRoot = groot
      teleported = true      // 同步到父级 maxed
  → 父级 toolbarPlaced 切到 maxlistTopRef.value
  → 工具栏 Teleport 启用，插入标题栏锚点
```

关闭时：`teleported = false` → `maxed = false` → 工具栏目标回到 `props.position` → 再关 modal。

### 2.3 ListVC 标准写法

```vue
<template>
  <flexpan :flex="flex" :height="height">
    <template #header>
      <teleport :to="toolbarPlaced" :disabled="!toolbarPlaced" v-if="!hideToolbar">
        <div class="vc-top-toolbar">
          <!-- 工具栏按钮 -->
          <slot name="toolbar"></slot>
        </div>
      </teleport>
    </template>
    <template #default>
      <vclist
        maxable
        maxTitle="专题材料"
        v-model:maxed="maxed"
        ...
      >
        <template #maxtop>
          <div ref="maxlistTopRef"></div>
        </template>
        <!-- vccol ... -->
      </vclist>
    </template>
  </flexpan>
</template>

<script setup>
const maxed = ref(false);
const maxlistTopRef = ref(null);

/** 普通态 → Page 传入的 position；最大化 → 弹窗标题栏锚点 DOM */
const toolbarPlaced = computed(() => {
  if (maxed.value) {
    return maxlistTopRef.value; // 必须是 HTMLElement，不能是 Ref 对象
  }
  return props.position;
});
</script>
```

Page 侧普通态挂点：

```vue
<div ref="VC_Sa_EbTopicListHead" class="sect-toolbar"></div>
<VC_Sa_EbTopicList :position="VC_Sa_EbTopicListHead" ... />
```

多数列表只需 `:position` + `:disabled="!position"`（无最大化工具栏需求时不必接 `maxed` / `#maxtop`）。

### 2.4 关键约定（易踩坑）

| 约定 | 正确 | 错误 |
|---|---|---|
| `disabled` | `:disabled="!toolbarPlaced"` | `maxed ? true : …`（最大化时反而禁止传送） |
| `:to` 目标 | `maxlistTopRef.value`（HTMLElement）或 `'#某id'` | `return maxlistTopRef`（Ref 对象） |
| 锚点位置 | `#maxtop` 内空 `div`，供 append | 把整段工具栏写进 `#maxtop`（与 Teleport 重复） |
| Teleport 组件 | Vue 内置 `<teleport>` | 勿与 `ui/common/teleport.vue` 旧实现混用 |

错误表现：

1. **最大化后标题栏只有占位文字、没有按钮**：`disabled` 在 `maxed===true` 时为 `true`，工具栏留在原 `flexpan #header`（被遮罩挡住）。
2. **`parent.insertBefore is not a function`**：`:to` 收到了 Ref / 非 DOM。Vue `moveTeleport` 把目标当 parent 调 `insertBefore` 即崩。

### 2.5 相关文件

| 文件 | 职责 |
|---|---|
| `web/src/ui/common/vclist/vclist.vue` | `maxable`、`v-model:maxed`、`#maxtop` / `#maxfoot`、列表主体 Teleport |
| `web/src/ui/common/vclist/modal.vue` | 最大化弹层（header / body / footer） |
| ListVC（如 `VC_Sa_EbTopicList.vue`） | 工具栏 Teleport + `toolbarPlaced` + `#maxtop` 锚点 |
| Page | `ref` 容器 + `:position` |

### 2.6 验收要点（最大化工具栏）

- [ ] 普通态：工具栏出现在 Page `position` 容器，不在列表上方留空栏（或符合版式）
- [ ] 最大化：同一套按钮出现在弹窗标题栏；还原后回到 `position`
- [ ] 最大化/还原无 `insertBefore` 报错
- [ ] `#maxtop` 仅放锚点（及必要标题文案），不重复整段工具栏 DOM

---

## 3. 表格 / 卡片双视图

> 基于落地实践整理。样本：`VC_Sa_EbNoteList`（笔记拆解）、`VC_Sa_EbMatList`（素材资料）。

### 3.1 目标与范围

- 同一业务列表支持 **table / card** 切换，偏好写入本地缓存。
- 卡片视图信息密度适中：标题可点开表单，关键标签与次要信息分区清晰，右侧有主/副操作。
- 行卡片为**独立组件**，列表 VC 只负责接线，避免把卡片 UI 塞进生成器 cover 区逻辑里难以维护。

| 层 | 职责 |
|---|---|
| `vclist` | 通用能力：`face` 双向绑定、`faceKey` 控制翻页栏切换按钮、`#li` 渲染卡片 |
| `mfaceToggle` | 切换 UI + `faceCache` 读写 |
| 业务 ListVC | 传 `v-model:face` / `face-key`、`query-fast-fields`、`#li`、组装 `rowApi` |
| `uicard/*Card.vue` | 单行卡片展示与行内操作入口 |

不在本规范内：后端接口改造、生成器自动生成卡片（当前为手写）。

### 3.2 基础设施

路径：`web/src/ui/common/vclist/vclist.vue`

- `face`：`defineModel('face')`，取值 `table` | `card`（与 `mfaceToggle.shared` 一致）。
- `faceKey`：非空时，在翻页栏**导出按钮之后**显示内置 `mfaceToggle`，并把 `faceKey` 作为缓存键。
- 卡片模式走 `#li`（或 `#card`）插槽；表头筛选列在 card 下不挂载时，用 `query-fast-fields` 提供查询。

```vue
<vclist
  v-model:face="mface"
  :face-key="faceCacheKey"
  :query-show-search="true"
  :query-fast-fields="cardQueryFields"
  ...
>
  <template #li="{ row }">
    <XxxCard ... />
  </template>
</vclist>
```

```js
import { VIEW_TABLE } from "@/ui/common/listCardView/mfaceToggle.shared.js";

const mface = ref(VIEW_TABLE);           // 默认表格，避免改变现网习惯
const faceCacheKey = `${VCName}.card`;   // 缓存键：VC 名 + .card
```

| 文件 | 说明 |
|---|---|
| `web/src/ui/common/listCardView/mfaceToggle.vue` | 切换按钮（`v-model` + `cache-key`） |
| `web/src/ui/common/listCardView/mfaceToggle.shared.js` | `VIEW_TABLE` / `VIEW_CARD` |
| `web/src/ui/common/listCardView/utils/faceCache.js` | localStorage 读写 |

工具栏**不要再单独放** `mfaceToggle`（除非特殊嵌入场景）；统一走 vclist 翻页栏，避免双入口。

### 3.3 标准目录与命名

```
.../VC/ListVC/<业务域>/
  VC_XXXList.vue          # 列表壳
  uicard/
    XXXCard.vue           # 行卡片（推荐前缀与业务表/VC 对齐）
```

示例：

- `ebook/note/VC/ListVC/uicard/Sa_EbNoteCard.vue`
- `ebook/material/VC/ListVC/uicard/Sa_EbMatCard.vue`

历史样本另有 `cardui/`（如 FlowMark），**新改造统一用 `uicard/`**。

### 3.4 卡片信息架构（推荐三区）

```
┌──────────────┬─────────────────────────────┬────────────┐
│ (可选)勾选/  │ ① 标题 + 跟随 tag            │ ④ 右侧工具栏 │
│ 等级小卡等   │ ② 副标题/关键字段行           │   主按钮    │
│              │ ③ 末行元信息                 │   副链接    │
└──────────────┴─────────────────────────────┴────────────┘
```

#### 标题区

- **一级标题**：业务主文案（可拼接编码），样式加粗；**可点击**，行为与「编辑」一致（不可编辑时改为查看）。
- **Tag**：紧跟标题右侧（`flex: 0 1 auto`，**不要**把标题撑满把 tag 顶到行尾）。
- Tag 内容选「分类/状态类」短字段（类型、专业、风险类型等）。

#### 副标题 / 次行

- 地点、单位、责任人等；关键 label 可用图标。
- 图标统一用全局组件 **`SvgIcon`**（`name="fa fa-xxx"`），**不要**写裸 `<i class="fa ...">`。

#### 末行

- 次要统计与审计信息：附件数、状态、年度/管控期、创建人与时间等。
- 创建信息建议靠右，格式可参考：`创建：张三|2026-1-1 9:55`。

#### 右侧工具栏

- 容器：`flex-direction: column; align-items: flex-end; align-self: stretch; justify-content: space-between`
- 主按钮：默认高度约 32px / 字号 14px（不要默认 `size="small"`）
- 副操作：蓝色文字链，用 `|` 分隔；需要贴底时对链接行使用 `margin-top: auto`
- `el-tooltip` 包按钮时加一层 `inline-flex` 包裹，避免破坏右对齐

#### 可选：左侧强调块

- 适合「等级 / 优先级」等需强视觉的字段。
- **只跨标题 + 次行**（与末行脱钩），避免竖排通栏三行。

#### 勾选与批量

- 卡片模式无表格 selection 列时，若列表依赖批量操作，在卡片左侧加 `el-checkbox`，与父级 `checkedRows` 同步。

### 3.5 列表 VC 接线清单

1. **确认字段**：标题、tags、副行、末行、主/副操作分别映射哪些列。
2. **新建** `uicard/XxxCard.vue`（props：`row`、`focusRow`、`rowApi`、业务字典/回调；按需 `checked` / `attachCount`）。
3. **ListVC 引入**：`VIEW_TABLE`、`XxxCard`、`mface` / `faceCacheKey` / `cardQueryFields`。
4. **`vclist` 增加**：`v-model:face`、`face-key`、`query-show-search`、`query-fast-fields`。
5. **`#li` 挂载卡片**，传入 `rowApi` 与展示辅助函数。
6. **组装 `rowApi`**：只暴露卡片需要的方法，避免把整个页面塞进 props。
7. **保留全部 `vccol`**：table 模式仍用；不要为了卡片删列。
8. **生成器区域**：`#li` 与 face 属性可落在 columns 的 cover 区内；复杂 `rowApi`、缓存键放在 cover 外 / lv3。

`cardQueryFields` 示例：

```js
const cardQueryFields = [
  { field: "En_Title", label: "标题", op: "like" },
  { field: "En_Status", label: "状态", ctype: "dict", dict: "En_Status", op: "=" },
];
```

### 3.6 卡片组件约定

| 项 | 约定 |
|---|---|
| `row` | 必填，当前行 |
| `focusRow` | 用于 `is-focused` 描边 |
| `rowApi` | 必填，方法对象 |
| `disabled` | 禁用编辑类操作 |
| `@select` | 点击卡片主体时回传行，父级 `setFocusRow` |

- `scoped`；BEM 前缀与业务相关。
- 空值统一 `—`。
- 卡片 `@click` → `select`；操作区、勾选、tag 区 `@click.stop`。
- 标题点击与「编辑」同路径；只读场景走查看。

### 3.7 两轮样本对照

| 项 | 笔记拆解 `Sa_EbNoteCard` | 素材资料 `Sa_EbMatCard` |
|---|---|---|
| 标题 | `En_Title` | `Em_Title` / 编码+名称 |
| Tag | 状态、来源平台 | 类型、格式 |
| 次行 | 点赞/收藏/评论等指标摘要 | 分类/摘要 |
| 左侧强调 | 可选封面或状态色块 | 类型小卡 |
| 末行 | 创建人\|时间 | 附件数、状态、创建时间 |
| 勾选 | 按需（批量） | 按需 |
| 主按钮 | 打开报告 / 编辑 | 编辑 |
| 副链接 | 下载\|删除 | 预览\|删除 |
| face 默认 | `VIEW_TABLE` | `VIEW_TABLE` |
| 缓存键 | `VC_Sa_EbNoteList.card` | `VC_Sa_EbMatList.card` |

### 3.8 实施路径与验收

```text
1. 盘点列表字段与行操作
2. 画卡片三区字段草图
3. 新建 uicard/XxxCard.vue
4. ListVC：mface + faceCacheKey + cardQueryFields
5. vclist：v-model:face + face-key + #li
6. rowApi +（可选）checked / attachCount 接线
7. 自测：table↔card、缓存、标题打开表单、副操作、批量、卡片查询
```

- [ ] 未传 `faceKey` 的其它列表无多余切换按钮
- [ ] 有 `faceKey` 时翻页栏出现切换，刷新后视图偏好恢复
- [ ] 卡片标题可打开表单；操作与表格列权限/状态一致
- [ ] SvgIcon 图标可见
- [ ] table 模式列与导出/筛选行为未回归

### 3.9 注意与反例

1. **不要**在工具栏与翻页栏各放一个 face 切换。
2. **不要**用裸 Font Awesome `<i>`，统一 `SvgIcon`。
3. **不要**把等级块做成通栏三行竖排。
4. **不要**为卡片删除 `vccol`；card 下查询靠 `query-fast-fields`。
5. **不要**把大段业务逻辑写进卡片；卡片只调 `rowApi`。
6. 生成器 cover 区：优先只加接线；逻辑放 cover 外，降低被覆盖风险。

### 3.10 参考路径速查

```text
web/src/ui/common/vclist/vclist.vue
web/src/ui/common/vclist/modal.vue
web/src/ui/common/listCardView/mfaceToggle.vue
web/src/ui/common/listCardView/mfaceToggle.shared.js
web/src/ui/common/listCardView/utils/faceCache.js
web/src/components/custom/svg-icon.vue

web/src/widgets/officeman/views/ebook/note/VC/ListVC/VC_Sa_EbNoteList.vue
web/src/widgets/officeman/views/ebook/note/VC/ListVC/uicard/Sa_EbNoteCard.vue
web/src/widgets/officeman/views/ebook/material/VC/ListVC/VC_Sa_EbMatList.vue
web/src/widgets/officeman/views/ebook/material/VC/ListVC/uicard/Sa_EbMatCard.vue

web/src/widgets/officeman/views/ebook/topic/VC/ListVC/VC_Sa_EbTopicList.vue  # 列表/工具栏参考
```
