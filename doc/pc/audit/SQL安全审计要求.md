# SQL 安全审计要求（专篇）

> **状态**：**强制**审计专篇（评审 / 门禁 / 新增代码红线）  
> **目录**：`doc/pc/audit/`  
> **规范依据**：[代码风格规范.md](../design/代码风格规范.md) §2.4–§2.5；[数据库查询.md](../design/数据库查询.md)（`whereGuid` 等）  
> **总册索引**：[审计要求.md](./审计要求.md)  
> **Agent 镜像**：`.cursor/rules/sql-safety.mdc`

**一句话**：值进参数、标识进白名单、限行用 `.top`、软删用 `whereIsOrNull`、禁止躲扫描。Hits=0 ≠ 安全。

---

## 1. 分级准则

| 等级 | 含义 | 典型形态 | 处置 |
|------|------|----------|------|
| **P0** | 外部可控值进入 SQL 且无参数化/白名单 | 搜索词拼 LIKE、`$"{input}"` 进 `exe`*/`where` | **立刻改**；可利用注入 |
| **P1** | 含运行时参数的整段 SQL 字符串执行 | `exeQuery`/`exeNonQuery` + `$`/`Format`/`+` | **禁止新增**；触达改 builder |
| **P2** | 先 `SqlFilter`/手工 `Replace("'","''")` 再拼接 | 「消毒后」仍拼进 SQL | 规范违规；新代码禁止复制 |
| **P3** | 表/列/`ORDER BY` 等标识符动态拼接 | `.from(a+b)`、`orderBy("c."+col)` | 须白名单，否则可升为注入 |
| **方言/规范** | 可移植性与 API 债 | `.select("TOP n")`、`ISNULL(col,v)=v`、`GETDATE()`、`set` 无谓 `ToString`、手写 `COUNT(*)` | 按 §5 改写 |
| **执行红线** | 躲扫描 | 拼好 → 中间变量 → `.select(var)` 专为躲门禁 | **违纪**；按风险本质改 |

**完全固定无参 SQL**（字面量/`const`，无插值、无变量拼接）可直接 `exe`*，**不计 P1**。`BatchSQL`、经白名单的必要 DDL、校验后的必要 `MERGE` 按风格 §2.4 允许口径。

---

## 2. 正面案例（必须照此写）

```csharp
// ✅ 值条件参数化
kit.select("*").from("T").where("Id", id);
kit.whereLike("mtb.En_Title", keyword);
kit.whereLikes(["mtb.En_Title", "mtb.En_Summary"], keyword);

// ✅ Guid 相等
kit.whereGuid("Sa_EbNoteOID", noteOid);
kit.whereGuid("En_Cata_FK", cataFk);

// ✅ 限行 / 可空相等 / 行数
kit.top(1).select("Sa_EbNoteOID").from("Sa_EbNote").whereGuid("Sa_EbNoteOID", oid);
kit.whereIsOrNull("SYS_Deleted", 0);
int cc = kit.from("T").where("Id", id).count();
bool ok = kit.from("T").whereGuid("T_OID", oid).exist();

// ✅ set：时间用 C# 时钟；Guid/数值原样传；DataRow 单元格可直接 set
kit.setTable("T")
   .set("Name", row["En_Title"])
   .set("SYS_LAST_UPD", DateTime.Now)
   .set("SYS_LAST_UPD_BY", userId)                 // Guid，勿 ToString()
   .whereGuid("T_OID", oid)
   .doUpdate();

// ✅ 更新/删除走 builder，勿 exeNonQuery 拼 DML
kit.setTable("Sa_EbNote")
   .set("En_Title", title)
   .whereGuid("Sa_EbNoteOID", noteOid)
   .doUpdate();

// ✅ 完全无参数的固定 SQL（字面量/常量）可直接 exe*
const string EnsureColSql = @"
IF COL_LENGTH('T','C') IS NULL
  ALTER TABLE T ADD C nvarchar(50) NULL;";
kit.exeNonQuery(EnsureColSql);

// ✅ 标识符：白名单枚举后得到常量列名再 orderBy
string orderCol = sort switch {
    "title" => "En_Title",
    "created" => "SYS_Created",
    _ => "SYS_Created"
};
kit.orderBy(orderCol);   // orderCol 仅为常量分支，无用户原文

// ✅ Filters key：BizServiceBase.IsSafeSqlIdent
if (IsSafeSqlIdent(key)) kit.where("mtb." + key, value);
```

---

## 3. 反面案例（禁止）

```csharp
// ❌ P0/P1：字符串拼接参数当 SQL
kit.where("Id='" + id + "'");
kit.exeNonQuery($"UPDATE T SET Name=N'{name}' WHERE Id='{id}'");
string.Format("SELECT * FROM T WHERE Id='{0}'", id);
kit.where($"mtb.En_Title like '%{keyword}%'");

// ❌ P0：搜索词拼 LIKE 再塞进 from/where 串
string likeStr = string.Format(
    " where (mtb.En_Title like '%{0}%' or mtb.En_Summary like '%{0}%')", key);
kit.from("Sa_EbNote mtb" + likeStr);

// ❌ P2：转义后仍拼接（新代码禁止）
var safeKw = "%" + keyword.Replace("'", "''") + "%";
kit.where($"mtb.En_Title like '{safeKw}'");

// ❌ P3：标识符当普通值偷懒拼接（无白名单）
kit.exeNonQuery($"UPDATE T SET {field} = NULL WHERE ...");
kit.orderBy("c." + orderByColumn);                 // orderByColumn 来自请求
kit.exeNonQuery($"DELETE FROM {tableName} WHERE ...");

// ❌ 方言锁死
kit.select("TOP 1 Sa_EbNoteOID").from("Sa_EbNote");
kit.where("ISNULL(SYS_Deleted,0)=0");

// ❌ set / 行数
kit.set("SYS_LAST_UPD", "GETDATE()", false);
kit.set("SYS_LAST_UPD_BY", userId.ToString());
kit.select("COUNT(*) as cnt").from("T").where(...).query();

// ❌ 列上函数包装再比较
kit.where("UPPER(LTRIM(RTRIM(En_SourceUrl)))", url);

// ❌ 执行红线：躲扫描（拼好再交给 builder）
var selectSql = "(SELECT ... WHERE OID='" + safeOid + "') AS X, ...";
kit.select(selectSql).query();
```

---

## 4. 问题 Demo（按等级）

下列为**问题形态演示**（已去业务细节、改为示意）。评审时对号入座；**新代码不得出现同类形态**。

### 4.1 P0 Demo — 关键词拼 LIKE（可注入）

```csharp
// ❌ Demo：请求 key 无转义拼进 LIKE，再拼进 from
public void SearchNote(string key)
{
    string likeStr = string.Format(
        " where (mtb.En_Title like '%{0}%' or mtb.En_Summary like '%{0}%')",
        key);
    var kit = Db.useSQL();
    kit.select("mtb.*").from("Sa_EbNote mtb" + likeStr).query();
}

// ✅ 改法
public void SearchNote(string key)
{
    var kit = Db.useSQL();
    kit.select("mtb.*")
       .from("Sa_EbNote as mtb")
       .whereLikes(new[] { "mtb.En_Title", "mtb.En_Summary" }, key)
       .query();
}
```

### 4.2 P0/P1 Demo — 筛选码拼 WHERE

```csharp
// ❌ Demo：前端传入的筛选码直接 Format 进条件
string status = input.Filters?["status"];
kit.where(string.Format("En_Status = '{0}'", status));

// ✅ 改法（key 过 IsSafeSqlIdent；值走参数）
if (IsSafeSqlIdent("En_Status"))
    kit.where("mtb.En_Status", status);
```

### 4.3 P1 Demo — exe* 插值 DML / 批量 IN

```csharp
// ❌ Demo
kit.exeNonQuery($"UPDATE Sa_EbNote SET En_Title=N'{title}' WHERE Sa_EbNoteOID='{oid}'");
kit.exeNonQuery(string.Format(
    "DELETE FROM Sa_EbTpMat WHERE Tm_TopicOID IN ({0})", deleteOidStr));

// ✅ 改法方向
kit.setTable("Sa_EbNote")
   .set("En_Title", title)
   .whereGuid("Sa_EbNoteOID", oid)
   .doUpdate();

kit.setTable("Sa_EbTpMat").whereInGuid("Tm_TopicOID", oidList).doDelete();
```

### 4.4 P2 Demo — 转义后仍拼接

```csharp
// ❌ Demo：以为「消毒了」就安全 —— 仍属规范违规，新代码禁止
var safeKw = "%" + keyword.Replace("'", "''") + "%";
kit.where($"mtb.En_Title like '{safeKw}'");

// ✅
kit.whereLike("mtb.En_Title", keyword);
```

### 4.5 P3 Demo — 动态标识符

```csharp
// ❌ Demo：请求传入的列名/表名直接进 SQL
kit.orderBy("c." + orderByColumn);
kit.exeNonQuery($"DELETE FROM {tableName} WHERE FK='{fk}'");

// ✅：白名单 → 常量标识符；值仍参数化
string col = orderByColumn switch {
    "title" => "En_Title",
    "date" => "SYS_Created",
    _ => "SYS_Created"
};
kit.orderBy(col);
// 表名必须来自服务端固定映射，禁止请求直传表名
```

### 4.6 方言/规范 Demo — TOP / ISNULL / GETDATE / COUNT / 躲扫描

```csharp
// ❌ TOP 写进 select
kit.select("TOP 1 Sa_EbNoteOID").from("Sa_EbNote").whereGuid("Sa_EbNoteOID", oid).query();
// ✅
kit.top(1).select("Sa_EbNoteOID").from("Sa_EbNote").whereGuid("Sa_EbNoteOID", oid).query();

// ❌ ISNULL 硬写
kit.where("ISNULL(SYS_Deleted,0)=0");
// ✅
kit.whereIsOrNull("SYS_Deleted", 0);

// ❌ 库时钟 / ToString / 手写 COUNT
kit.set("SYS_LAST_UPD", "GETDATE()", false);
kit.set("SYS_LAST_UPD_BY", userId.ToString());
kit.select("COUNT(*) as cnt").from("T").where("Id", id).query();
// ✅
kit.set("SYS_LAST_UPD", DateTime.Now).set("SYS_LAST_UPD_BY", userId);
int n = kit.from("T").where("Id", id).count();

// ❌ 躲扫描（执行红线）
var yearExpr = "(SELECT ... WHERE y.OID='" + safeYearFk + "')";
kit.select(yearExpr + " AS X, ...").query();
// ✅：独立 queryScalar / 参数化子查询 / 内存组装；禁止「为躲命中而拆变量」
```

---

## 5. 方言与规范债速查（M12–M17）

| 代号 | 问题 | 正确写法 |
|------|------|----------|
| **M12** | `.select("TOP n …")` | `.top(n).select("列…")`；有无行用 `exist()` |
| **M13** | `where("ISNULL(col,v)=v")` | `whereIsOrNull(col, v)`；d≠v 用 `where(col, v)` |
| **M14** | `set(col, "GETDATE()", false)` | `set(col, DateTime.Now)` |
| **M15** | `set(OID列, guid.ToString())` | 直接传 `Guid?` |
| **M16** | 仅为行数 `select COUNT(*)…query` | `.count()` → `int` |
| **M17** | 躲扫描 | 禁止；按 P0–P3 本质改 |

相关子查询里的 `(SELECT TOP n …)` 不计入 M12 主清零，触达时优先拆独立查询。

---

## 6. 允许口径（勿误杀）

| 允许 | 说明 |
|------|------|
| 无参固定 SQL 字面量/`const` 的 `exe*` | 无插值、无 `+` 变量、无 `$"..."` |
| `BatchSQL`（`useBatchSQL` → `addInsert`/`addUpdate` → `exeNonQuery`） | 链式设参后批量提交 |
| 白名单后的必要 DDL | 表名经 `IsSafeSqlIdent` 或等价 |
| 入参已校验的必要 `MERGE` | 优先能拆则拆 `doUpdate`/`doInsert` |
| `BizServiceBase.ApplyFilters` | key 已过 `IsSafeSqlIdent`；值走 `where` 参数 |

---

## 7. 评审清单（SQL 专检）

- [ ] 无 P0：外部输入未进拼接 SQL  
- [ ] 无新增 P1：`exe*`/`where`/`Format` 拼运行时值  
- [ ] 无新增 P2：`SqlFilter`/`Replace` 后再拼  
- [ ] 动态表/列/排序仅白名单常量  
- [ ] Guid 用 `whereGuid`；无 `'"+oid+"'`  
- [ ] 限行 `.top`；软删 `whereIsOrNull`；时间 `DateTime.Now`  
- [ ] 纯行数 `.count()`；无躲扫描注释或中间变量骗门禁  

---

## 8. 与其它文档关系

| 文档 | 关系 |
|------|------|
| **本文** | SQL 安全**专篇**：准则 + 正反例 + 问题 Demo（**优先阅读**） |
| [审计要求.md](./审计要求.md) | 全主题索引；§1 摘要指向本文 |
| [代码风格规范.md](../design/代码风格规范.md) §2.4–§2.5 | 日常开发总册中的同条约束 |
| [数据库查询.md](../design/数据库查询.md) | `whereGuid` / `setGuid` 等类型化专项 |

---

## 9. 修订记录

| 日期 | 说明 |
|------|------|
| 2026-11-07 | 按 sleveen-admin（`Sa_*`、`BizServiceBase`、`whereLike`）改写；去掉来源项目路径与表前缀 |
