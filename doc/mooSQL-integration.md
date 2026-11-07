# sleveen-admin 集成 mooSQL 指南

> 本文说明当前仓储如何�?[mooSQL](../../../moo-sqlm/mooSQL2024/) 作为数据访问层接�?sleveen-admin，涵盖项目引用、配置、实体映射、仓储封装与典型用法�? 
> 相关方案�?[PLAN.md](../PLAN.md)，实施进度见 [IMPLEMENTATION.md](../IMPLEMENTATION.md)�?

---

## 1. 集成概览

sleveen-admin **不使�?Furion / SqlSugar 作为 ORM 运行�?*，而是�?

| 层次 | 说明 |
|------|------|
| **ORM 运行�?* | [mooSQL.Ext](../../../moo-sqlm/mooSQL2024/ext/mooSQL.Ext.csproj)（源�?ProjectReference�?|
| **实体映射** | mooSQL 原生 `[SooTable]` / `[SooColumn]`，由 `DBClientBuilder` 内置 `InternalEntityParser` 解析 |
| **应用封装** | `SleveenAdmin.Foundation/Data/` 中的 `MooSqlSetup`、`MooRepository<T>` |
| **业务访问** | 各域 Service 注入 `MooRepository<T>`，通过 `.Repo` 调用 `SooRepository<T>` API |

```mermaid
flowchart LR
    subgraph apps [应用层]
        Service["UserService / AuthService �?]
    end

    subgraph moosql [mooSQL]
        Builder["DBClientBuilder"]
        Parser["InternalEntityParser"]
        Cash["DBInsCash"]
        Instance["DBInstance"]
        SooRepo["SooRepository&lt;T&gt;"]
    end

    Service --> MooRepo
    Setup --> Builder
    Builder --> Parser
    Builder --> Cash
    MooRepo --> Cash
    Cash --> Instance
    Instance --> SooRepo
```

---

## 2. 仓库目录与项目引�?

### 2.1 相对路径

sleveen-admin �?mooSQL 在同一 monorepo 下，目录关系如下�?

```
my/                                 # 仓库�?
├── moo-sqlm/mooSQL2024/
�?  ├── pure/mooSQL.Pure.csproj     # 核心 ADO / LINQ / DDL
�?  └── ext/mooSQL.Ext.csproj       # 扩展（多数据库驱动、Excel 等）
└── admin/sleveenAdmin/
    ├── doc/                        # 本文档所在目�?
    └── backend/
        ├── sleveenAdmin.sln
        └── core/
            ├── SleveenAdmin.Foundation/    # 引用 mooSQL.Ext
            ├── SleveenAdmin.System/        # 引用 mooSQL.Ext（DbInitializer 直接使用 mooSQL API�?
            └── SleveenAdmin.Web.Api/       # 通过 Foundation 间接使用；Program 调用 AddMooSql
```

### 2.2 csproj 引用方式

**Foundation**（所有数据访问封装在此）�?

```xml
<ProjectReference Include="..\..\..\..\moo-sqlm\mooSQL2024\ext\mooSQL.Ext.csproj" />
```

**System**（Bootstrap �?DDL / Seed 需直接引用 `mooSQL.data` 命名空间）：

```xml
<ProjectReference Include="..\SleveenAdmin.Foundation\SleveenAdmin.Foundation.csproj" />
<ProjectReference Include="..\..\..\..\moo-sqlm\mooSQL2024\ext\mooSQL.Ext.csproj" />
```

> **说明**：采�?**ProjectReference 源码引用** 而非 NuGet 包，便于联调 DDL、方言�?bugfix。独立发布时可改为打包后�?NuGet 引用，配置与 API 用法不变。项�?*不引�?SqlSugar**，实体映射完全使�?mooSQL 特性�?

---

## 3. 启动注册

### 3.1 配置�?

`Configuration/DBCoonfig.json` �?`DBCoonfig` 节绑定到 `DBCoonfigOptions`（与 api10 一致，使用 mooSQL `DBPosition`）：

```json
{
  "DBCoonfig": {
    "Connections": [
      {
        "Position": 0,
        "Name": "Master",
        "DbType": "SQLite",
        "ConnectString": "Data Source=App_Data/sleveenadmin.db",
        "Version": "3.0",
        "WatchSQL": true,
        "MinTimeSpan": 900
      }
    ]
  }
}
```

开发默�?**SQLite**（相对路径会解析�?`ContentRoot/App_Data` 并自动建目录）。切�?SQL Server 示例�?

```json
{
  "DbType": "MSSQL",
  "ConnectString": "Enlist=false;Data Source=localhost;Database=sleveenadmin;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True;",
  "Version": "13.0"
}
```

| 字段 | 说明 |
|------|------|
| `Position` | 连接槽位索引，与 `DBInsCash.getInstance(position)` 对应（`addConfig` �?Position 注册�?|
| `DbType` | 支持 `MSSQL` / `SQLSERVER`、`MYSQL`、`POSTGRESQL` / `PGSQL`、`ORACLE`、`SQLite` |
| `Version` | 数据库版本号，供方言�?DDL 推断列类�?|
| `WatchSQL` / `MinTimeSpan` | �?SQL 监控开关与阈值（ms�?|

主库使用 **Position = 0**，常�?`MooSqlSetup.DefaultPosition = 0`�?

### 3.2 DI 注册

`Program.cs` 中一行完成接入：

```csharp
builder.Services.AddMooSql(builder.Configuration);
```

`AddMooSql`（`Foundation/Data/MooSqlSetup.cs`）注册：

| 服务 | 生命周期 | 作用 |
|------|----------|------|
| `IOptions<DBCoonfigOptions>` | Options | 读取连接配置 |
| `DBInsCash` | Singleton | mooSQL 连接�?/ 多库实例容器 |
| `MooRepository<>` | Scoped | 按请求作用域的泛型仓�?|

### 3.3 构建 DBInsCash 流程

```csharp
var cash = new DBClientBuilder().doBuild();
// DBClientBuilder 内置注册 InternalEntityParser，解�?[SooTable] / [SooColumn]

// SQLite 相对路径先解析到 ContentRoot，再�?Position 注册
cash.addConfig(options.Connections);
```

开发环境下附加 `MooSqlQueryWatchor`：SQL 执行异常时写�?`log/moosql/errSql{yyyyMMdd}.txt`�?

### 3.4 应用启动时建表与 Seed

`Program.cs` 启动后通过 `IDbInitializer` 初始化数据库�?

```csharp
var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
await initializer.InitializeAsync();
```

`DbInitializer` 核心步骤�?

1. `DBExecutor` + `getPingSQL()` �?验证连接
2. `db.useDDL().doInitTable(...)` �?按实体类型自动建�?补列
3. `SeedIfEmpty` / `Ensure*Menus` �?写入 Seed 与增量菜�?

数据库不可达时仅记录 Warning，不阻断应用启动（便于先�?Swagger 排查配置）�?

---

## 4. 封装�?API

### 4.1 MooRepository&lt;T&gt;

```csharp
public class MooRepository<T> where T : class, new()
{
    public SooRepository<T> Repo => _cash.getInstance(_position).useRepo<T>();
    public DBInstance Db => _cash.getInstance(_position);
}
```

| 成员 | 用�?|
|------|------|
| `.Repo` | 日常 CRUD，业�?Service **应优先使�?* |
| `.Db` | 需�?`useWork()`、`useDDL()` 等底层能力时使用（见 DbInitializer�?|

构造时可传 `position` 指定非默认库；当前全�?Service 使用默认 `position = 0`�?

### 4.2 业务 Service 注入示例

```csharp
public sealed class UserService : IScoped
{
    private readonly MooRepository<SysUser> _users;
    private readonly MooRepository<SysUserRole> _userRoles;

    public UserService(
        MooRepository<SysUser> users,
        MooRepository<SysUserRole> userRoles)
    {
        _users = users;
        _userRoles = userRoles;
    }

    public Guid Create(SysUserInput input)
    {
        var id = Guid.CreateVersion7();
        _users.Repo.Insert(new SysUser { Id = id, /* ... */ });
        return id;
    }
}
```

无需注册具体 `MooRepository<SysUser>`：`AddScoped(typeof(MooRepository<>))` 已覆盖所有实体类型�?

### 4.3 SooRepository 常用方法（当前项目用法）

| 方法 | 示例场景 |
|------|----------|
| `GetList()` / `GetList(predicate)` | 列表、关联查询（AuthService 登录�?|
| `GetById(id)` | 单条详情 |
| `GetFirst(predicate)` | 唯一性校验（账号重复�?|
| `Insert(entity)` | 新增 |
| `Update(entity)` | 更新 |
| `Delete(entity)` / `DeleteById(id)` | 删除 |
| `Count(predicate)` | 是否为空（Seed 判断�?|
| `Exist(id)` | 菜单增量 Seed 幂等 |

复杂分页目前在内存侧�?`PageHelper.Apply` 处理（如 `UserService`、`LogService`）；后续可按需改为 mooSQL 侧分�?SQL�?

### 4.4 DBCash 静态门面（�?DI 同源�?

Author / AuthBag 移植层沿用参考项目的 `DBCash.useSQL(0)` 写法。为避免「DI 一�?`DBInsCash`、静态调用再懒建一套」的双实例问题，`AddMooSql` 构建完连接池后会调用 `DBCash.Bind(cash)`，静态方法与 `MooRepository<T>` **共享同一 `DBInsCash` 单例**�?

```csharp
// MooSqlSetup.CreateCash() 末尾
DBCash.Bind(cash);
return cash;
```

| 静态方�?| 委托目标 |
|----------|----------|
| `GetDBInstance(position\|name)` | `cash.getInstance(...)` |
| `useSQL(position\|name)` | `new SQLBuilder().setDBInstance(...)` |
| `useRepo<T>(position)` | `getInstance(position).useRepo<T>()` |
| `useWork` / `useDDL` / `useClip` / `useBus` / `useQueryable` | 同参�?api8 `DBCash` |

未执�?`AddMooSql`（即�?Bind）前调用 `DBCash.useSQL` 会抛出明确异常，避免静默创建第二套连接�?

实现位置：[DBCash.cs](../backend/core/SleveenAdmin.Foundation/src/Data/DBCash.cs)、[MooSqlSetup.cs](../backend/core/SleveenAdmin.Foundation/src/Data/MooSqlSetup.cs)�?

---

## 5. 实体定义规范

### 5.1 文件位置

每个域在 `DBLayer/` 下定义实体，例如 `SleveenAdmin.System/User/DBLayer/SysUser.cs`�?

### 5.2 特性与列名

```csharp
using mooSQL.data;

[SooTable("SysUser", Caption = "系统用户")]
public class SysUser
{
    [SooColumn("Su_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Su_Account", Length = 64, CanBeNull = false)]
    public string Account { get; set; } = string.Empty;

    [SooColumn("Su_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }
}
```

| 规范 | 说明 |
|------|------|
| 表名 | `[SooTable("SysUser")]` �?`[SooTable("SysUser", Caption = "�?)]` |
| 列名 | `{缩写}_字段` �?`SysUser` �?`Su_`；`SysRoleMenu` �?`Srm_` |
| 主键 | `[SooColumn(�? IsPrimaryKey = true)]`；运行时 `Guid.CreateVersion7()` |
| 可空 | `CanBeNull = true/false`（对�?DDL `NULL` / `NOT NULL`�?|
| 长度 | `Length = N` 用于 `varchar` 等类�?|
| SQL 类型 | �?mooSQL DDL 根据 C# 属性类型推断；可显式指�?`DbType` |

### 5.3 特性对照（�?SqlSugar 迁移�?

| SqlSugar（已废弃�?| mooSQL |
|-------------------|--------|
| `[SugarTable("T", TableDescription = "�?)]` | `[SooTable("T", Caption = "�?)]` |
| `[SugarColumn(ColumnName = "C", IsPrimaryKey = true)]` | `[SooColumn("C", IsPrimaryKey = true)]` |
| `[SugarColumn(ColumnName = "C", Length = N)]` | `[SooColumn("C", Length = N, CanBeNull = false)]` |
| `[SugarColumn(�? IsNullable = true)]` | `[SooColumn(�? CanBeNull = true)]` |

实体解析�?mooSQL.Ext �?`InternalEntityParser` 完成（`DBClientBuilder.buildingCash()` 自动注册），无需�?sleveen-admin 侧编写桥接解析器�?

---

## 6. DDL �?Seed

### 6.1 注册实体类型

新增表时，在 `DbInitializer.EntityTypes` 数组中追加实�?`typeof`�?

```csharp
private static readonly Type[] EntityTypes =
[
    typeof(SysUser),
    typeof(SysRole),
    // �?新增实体在此追加
];
```

启动�?`doInitTable` 会对比实体与库表结构，自动创建或变更（具体行为由 mooSQL DDL �?`DMLOption` 决定）�?

### 6.2 事务批量写入

Seed 多表插入使用 `useWork()`�?

```csharp
var work = db.useWork();
work.Insert(admin);
work.Insert(role);
foreach (var menu in menus)
    work.Insert(menu);
work.Commit();
```

`Ensure*Menus` 等增�?Seed 通过 `menuRepo.Exist(fixedId)` 保证幂等，避免重复插入�?

### 6.3 直接使用 DBInstance

Bootstrap 层注�?`DBInsCash` 而非 `MooRepository`，以便在同一次初始化中调�?DDL、Repo、Work�?

```csharp
public DbInitializer(DBInsCash cash) => _cash = cash;

public Task InitializeAsync(...)
{
    var db = _cash.getInstance(MooSqlSetup.DefaultPosition);
    db.useDDL().doInitTable(new DMLOption(), EntityTypes);
    // ...
}
```

---

## 7. 与其他组件的共享

### 7.1 Hangfire

Hangfire 使用 **SQL Server** 存储，复�?Master 连接串（`HangfireSetupExtensions`），Schema 默认 `Hangfire`。开发默认主库为 SQLite 时，`Hangfire:Enabled` 应为 `false`；若�?SQLITE 下误开，启动会抛出明确异常。切�?`MSSQL` 后可�?`Hangfire:Enabled` 设为 `true`�?

### 7.2 错误日志

SQL 异常 �?`MooSqlQueryWatchor.onAfterExecuteSetError` �?控制�?+ `log/moosql/errSql*.txt`（相�?`ContentRootPath`，即 Web.Api 项目目录）�?

---

## 8. 新增实体检查清�?

按顺序完成以下步骤：

1. **实体** �?在对应域 `DBLayer/` 新建类，补全 `[SooTable]` / `[SooColumn]`
2. **DDL** �?�?`typeof(YourEntity)` 加入 `DbInitializer.EntityTypes`
3. **Seed**（可选） �?固定 Guid 写入 `SeedIds`；在 `DbInitializer` 或独�?`Ensure*` 方法中插入初始数�?
4. **Service** �?注入 `MooRepository<YourEntity>`，通过 `.Repo` 完成 CRUD
5. **验证** �?本地启动 API，确�?Swagger 可用、表已创建、Seed 符合预期

**主键约定**

- 运行时业务数据：`Guid.CreateVersion7()`
- Seed / 菜单 / 内置角色：`SeedIds` 中预定义固定 Guid

---

## 9. �?Admin.NET / SqlSugar 的差�?

| 项目 | Admin.NET | sleveen-admin |
|------|-----------|----------|
| ORM | SqlSugarRepository | mooSQL `SooRepository<T>` |
| DI 仓储 | `SqlSugarRepository<T>` 注入 | `MooRepository<T>` 注入 |
| 特�?| SqlSugar 原生 | mooSQL `[SooTable]` / `[SooColumn]` |
| 主键 | 雪花 `long` | Guid v7 |
| 列名 | 框架默认 | 强制 `{Abbr}_` 前缀 |
| 多库 | 内置 | `DBInsCash.addDataBase(index, �?`，按 Position 扩展 |

业务迁移时：�?Admin.NET 实体特性改�?`SooTable`/`SooColumn`，并�?`SqlSugarRepository` 调用替换�?`_repo.Repo` 上等价方法�?

---

## 10. 常见问题

### Q1：编译找不到 mooSQL.Ext

确认 monorepo 目录结构完整，且 csproj 中相对路径为�?

`../../../../moo-sqlm/mooSQL2024/ext/mooSQL.Ext.csproj`

（自 `backend/core/SleveenAdmin.Foundation` �?`backend/core/SleveenAdmin.System` 向上�?`my/`。）

### Q2：启动报数据库连接失�?

- 检�?`DBCoonfig:Connections[0].DbType` / `ConnectString`
- 默认 SQLite：确�?`App_Data` 可写；库文件会自动创�?
- 若使�?MSSQL：确�?SQL Server 已启动、库可建�?
- 查看 `Program.cs` �?DbInitializer �?Warning 日志

### Q3：表结构未更�?

- 确认新实体已加入 `DbInitializer.EntityTypes`
- 重启 API 触发 `doInitTable`
- �?DDL 与已有列冲突，检�?`SooColumn` �?`Length` / `CanBeNull` 是否与库表一致；必要时查�?mooSQL DDL 日志�?`log/moosql/` 错误文件

### Q4：能否换 MySQL / PostgreSQL / SQL Server�?

配置中将 `DbType` 改为 `MYSQL`、`POSTGRESQL` �?`MSSQL` 并更换连接串即可（由 mooSQL `DBPosition.asDataBase` / `asDBType` 解析）。开发默认在 SQLite 上验证；换库需自行回归 DDL 与方言差异。启�?Hangfire 时主库须�?SQL Server�?

### Q5：为�?System 重复引用 mooSQL.Ext�?

`DbInitializer` 等类直接使用 `mooSQL.data`、`mooSQL.data.model` 类型（`DBInstance`、`DMLOption`、`SQLCmd`）。显式引用可避免仅依赖传递引用时�?IDE/编译边界问题�?

---

## 11. 关键源文件索�?

| 文件 | 职责 |
|------|------|
| [MooSqlSetup.cs](../backend/core/SleveenAdmin.Foundation/src/Data/MooSqlSetup.cs) | DI 注册、DBInsCash 构建、`DBCash.Bind`、方言解析、SQL 错误监视 |
| [MooSqlOptions.cs](../backend/core/SleveenAdmin.Foundation/src/Data/MooSqlOptions.cs) | `DBCoonfigOptions` 配置模型 |
| [DBCash.cs](../backend/core/SleveenAdmin.Foundation/src/Data/DBCash.cs) | 静�?SQL/Repo 门面，与 DI 同源 |
| [MooRepository.cs](../backend/core/SleveenAdmin.Foundation/src/Data/MooRepository.cs) | 泛型仓储封装 |
| [DbInitializer.cs](../backend/core/SleveenAdmin.System/src/widgets/Bootstrap/DbInitializer.cs) | 建表、Seed、增量菜�?|
| [Author/](../backend/core/SleveenAdmin.System/src/widgets/basisOn/Author/) | mooSQL.auth 系统层：`useAuthor` / `useDuty` / `useDutyGoods` |
| [AuthBag/](../backend/core/SleveenAdmin.System/src/widgets/basisOn/AuthBag/) | AuthBag 配置 CRUD API �?8 张配置表 |
| [MenuService.cs](../backend/core/SleveenAdmin.System/src/widgets/basisOn/Menu/Service/MenuService.cs) | 运行时菜单过滤（`useDutyGoods` Gi_Type=1�?|
| [Program.cs](../backend/core/SleveenAdmin.Web.Api/Program.cs) | `AddMooSql`、启动初始化 |
| [appsettings.json](../backend/core/SleveenAdmin.Web.Api/appsettings.json) | 连接串配�?|
| [SysUser.cs](../backend/core/SleveenAdmin.System/src/widgets/basisOn/User/DBLayer/SysUser.cs) | 实体定义示例 |
| [UserService.cs](../backend/core/SleveenAdmin.System/src/widgets/basisOn/User/Service/UserService.cs) | Service �?CRUD 示例 |

---

## 12. mooSQL.auth �?AuthBag 权限体系

sleveen-admin �?RBAC 按钮权限（JWT Claims + `[Permission]`）之外，接入 **mooSQL.auth 数据权限引擎** �?**AuthBag 责任权限配置**�?

### 12.1 分层

| 层次 | 目录 | 对外 API |
|------|------|----------|
| mooSQL.auth 引擎 | `mooSQL2024/pure/src/zoom/auth/` | `AuthorBuilder`、`WordBag`、Pipeline |
| 系统角色（DataScope�?| `SleveenAdmin.System/Author/SysRole/` | `SQLBuilder.useAuthor(ctx, b => �?` |
| 责任权限（Duty�?| `SleveenAdmin.System/Author/AuthBag/` | `kit.useDuty(ctx, �?`、`kit.useDutyGoods(ctx, "1", �?` |
| 配置 CRUD | `SleveenAdmin.System/AuthBag/` | `/api/authBag/*` REST |
| 管理界面 | `web/src/views/manage/auth-bag/` | NaiveUI �?Tab（不含门户绑定） |

### 12.2 配置表（8 张）

`AuthDutyItem`、`AuthGoodsBag/Item`、`AuthVisitorBag/Item`、`AuthWordBag/Item/Define`。资源项 `Gi_Type` 首期仅支�?`"1"`（菜单），绑�?`SysMenu` �?Path/Title�?

`SysOrg.So_ClassCode` 供访客项 `Vi_Type=3` 组织编码前缀匹配；未维护时可 fallback �?`So_Code`�?

### 12.3 运行时行�?

- **超管**（角色码 `R_SUPER`）：菜单全量可见�?
- **普通用�?*：若存在匹配的访客集合，菜单�?`SysRoleMenu` 基础上再�?`useDutyGoods(ctx, "1", ids => �?` 过滤；未配置 Duty 时保持原 RBAC 菜单行为（向后兼容）�?
- **缓存**：`AuthCacheService`（`IMemoryCache`）缓存访客包、组织编码等；管理端「缓存清理」调�?`cleanCache` 按前缀失效�?

### 12.4 用户上下�?

`IAuthUserContext` / `AuthUserContext`（Foundation）从 JWT Claims（`account`、`orgId`、`userId`）构�?`mooSQL.auth.AuthUser`，替代参考项目的 `UserManager`�?

---

## 13. 后续演进（可选）

- 将高频分页查询下沉为 mooSQL �?SQL，减�?`GetList()` 全表加载
- 只读从库：在 `Connections` 增加 `Position = 1`，Service 注入时指�?`position`
- 集成测试：基�?Testcontainers 或本�?SQL Server 验证 DDL + Seed 幂等
