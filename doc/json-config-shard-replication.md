# JSON 配置分片加载 �?功能复刻文档

> 本文分析**参照�?* `hhny.net.base.pure` �?*参照宿主** `slickflow/api10` 将配置拆成独�?JSON 并自动合并加载的原理，对�?**sleveen-admin** 现状，规划基于标�?ASP.NET Core 的轻量自研复刻方案�? 
> 相关方案�?[PLAN.md](../PLAN.md)；mooSQL 集成�?[mooSQL-integration.md](./mooSQL-integration.md)�?

---

## 1. 结论先行

| 维度 | 结论 |
|------|------|
| **核心能力** | 一特�?一模块一�?JSON �?目录扫描 �?`AddJsonFile` 合并�?`IConfiguration` �?按节名绑�?`IOptions<T>` |
| **参照宿主实践** | `appsettings` 只声明扫描目录；业务配置落在 `Configuration/*.json`；`AddProjectOptions` 批量注册 Options |
| **双轨配置** | 基础设施用文�?Options；产品开关可再镜像到 DB（`SysConfig`）供管理�?CRUD |
| **admin 复刻约束** | 不引用参照库运行时；复刻扫描/合并机制本身，用标准 ASP.NET Core API 自研轻量扩展 |
| **admin 现状缺口** | 配置集中在单文件 `appsettings.json`；`SysConfig` 仅扁�?KV CRUD，业务侧几乎未消费；仅有手工 `Configration/llm.json` |

---

## 2. 原理：参照库如何「拆 JSON + 加载�?

源码根：`E:\coding\gitlab\my\hhny.net.base.pure\pure\src\`（下文称**参照�?*）�? 
该仓库是**框架�?*，本身不带业�?`Configuration/*.json` 样例；机制在宿主启动时生效�?

### 2.1 设计原则

**一个关注点 = 一份独�?JSON（可选环境覆盖文件），通过 ASP.NET Core 配置源分层合并�?*

| 规则 | 行为 |
|------|------|
| 扫描�?| 始终：`AppContext.BaseDirectory` + `ContentRootPath`（若不同�?|
| 额外目录 | `ConfigurationScanDirectories`（`appsettings` 或注入选项代码侧） |
| 深度 | **仅顶�?*（`TopDirectoryOnly`），不递归 |
| 命名 | `{Name}.json` + 可�?`{Name}.{Environment}.json` |
| 合并 | `AddJsonFile(optional:true, reloadOnChange:true)`；后加载覆盖先加载；同名优先 ContentRoot |
| 排除 | `appsettings*`、`bundleconfig*`、`compilerconfig*`、`*deps.json`、`*runtimeconfig*` �?+ `IgnoreConfigurationFiles` |
| 绑定 | Options 类名去掉 `Options` 后缀 �?配置节路径（�?Options 路径特性） |

> 注意：根目录**总会被扫�?*。仅把文件放�?`Configuration/` 而不声明 `ConfigurationScanDirectories`�?*不会**被加载�?

### 2.2 关键实现

| 角色 | 路径（相对参照库 `pure/src/`�?|
|------|------|
| 扫描/合并引擎 | `App/Internal/InternalApp.cs` �?`AddJsonFiles` |
| 对外扩展 | `App/Extensions/` �?ConfigurationBuilder 扩展 �?公开 JSON 扫描 API |
| 开�?| `App/Options/` 宿主选项（`ScanJsonFiles`；外部宿主模式可关闭自动扫描�?|
| Options 绑定 | `ConfigurableOptions/.../AddConfigurableOptions<T>` |
| 节路径解�?| `ConfigurableOptions/Internal/Penetrates.cs` |
| 静态读�?| `App/App.cs` �?`GetConfig` / `GetOptions*` |

核心流程（简化）�?

```
宿主启动（参照库 Inject / Use* / Serve.Run 一类入口）
  �?先加�?ASP.NET 默认 appsettings*.json
  �?InternalApp.AddJsonFiles
       1. �?ConfigurationScanDirectories / IgnoreConfigurationFiles
       2. ResolveScanDirectories(BaseDirectory, ContentRoot, 额外目录)
       3. 枚举 *.json（顶层）
       4. 按逻辑名分组（Cache.json / Cache.Development.json �?�?"Cache"�?
       5. 每组只保�?{Name}.json �?{Name}.{Env}.json
       6. ContentRoot 优先；按文件名长度排序（基文件先于环境文件）
       7. configurationBuilder.AddJsonFile(...)
  �?services.AddConfigurableOptions<TOptions>()
       �?GetSection(path).Bind �?IOptions<T> + 校验 / PostConfigure / 变更监听
```

`AddJsonFiles` 关键片段�?

```200:254:E:\coding\gitlab\my\hhny.net.base.pure\pure\src\App\Internal\InternalApp.cs
    internal static void AddJsonFiles(IConfigurationBuilder configurationBuilder, IHostEnvironment hostEnvironment)
    {
        // ...
        var scanEntries = (configuration.GetSection("ConfigurationScanDirectories").Get<string[]>()
                ?? Array.Empty<string>())
            .Concat(InjectOptions.InternalConfigurationScanDirectories ?? Array.Empty<string>());

        var scanDirectories = ResolveScanDirectories(executeDirectory, contentRoot, scanEntries);
        // ... 分组、过滤、PreferContentRoot、AddJsonFile(optional: true, reloadOnChange: true)
    }
```

节路径约定（`Penetrates`）：

- 无路径特性：类名�?`Options` 结尾则去掉后缀，否则用全名（`JwtOptions` �?`Jwt`�?
- 有特性且指定 `Path`：用指定路径
- 有特性但 `Path` 为空：用类名原样

### 2.3 宿主侧配置样例（概念�?

```json
// appsettings.json �?只放扫描策略与少量覆�?
{
  "ConfigurationScanDirectories": [ "Configuration" ],
  "IgnoreConfigurationFiles": [ "*.local.json" ]
}
```

```json
// Configuration/CorsAccessorSettings.json
{
  "CorsAccessorSettings": {
    "WithOrigins": [ "http://localhost:5173" ]
  }
}
```

```csharp
services.AddConfigurableOptions<CorsAccessorSettingsOptions>();
// �?App.GetConfig<T>("CorsAccessorSettings", loadPostConfigure: true);
```

---

## 3. 实践：参照宿�?api10 如何落地

源码根：`E:\coding\gitee\my\workflow\slick\slickflow\api10\`（下文称**参照宿主** / **api10**）�? 
api10 **不重写扫描器**，复用参照库�?`AddJsonFiles`；自身负责目录布局、Options 类型�?DB 参数管理�?

### 3.1 目录布局

```
HHNY.NET.Web.Entry/
  appsettings.json                 �?ConfigurationScanDirectories + 少量覆盖
  Configuration/                   �?宿主本地分片（可选）
HHNY.NET.Application/
  Configuration/
    SysConfig.json
    DBCoonfig.json
    Cache.json
    OSS.json                       �?可含多个根节（Upload + OSSProvider�?
    JWT.json / Logging.json / �?
    App.Development.json           �?环境覆盖（组�?App�?
    App.Production.json
  �?csproj CopyToOutputDirectory=PreserveNewest
     输出�?bin/.../Configuration/
```

当前 Application 下主要分片（节名 �?文件逻辑名对应的根键）：

| 文件 | 典型根节 |
|------|----------|
| `SysConfig.json` | `SysConfig` |
| `DBCoonfig.json` | `DBCoonfig` |
| `Cache.json` | `Cache` |
| `OSS.json` | `Upload`, `OSSProvider` |
| `FtpStores.json` | `FtpStore` |
| `JWT.json` | `JWTSettings` |
| `Logging.json` / `Limit.json` / `NFlow.json` / �?| 对应业务�?|

`appsettings.json` 声明�?

```json
"ConfigurationScanDirectories": [ "Configuration", "" ]
```

- `"Configuration"`：扫�?`{Root}/Configuration`
- `""`：显式表示根目录本身（与默认扫描根叠加，语义上强调「根也参与」）

### 3.2 Options 批量注册

`HHNY.NET.Web.Core/ProjectOptions.cs`�?

```csharp
services.AddConfigurableOptions<DBCoonfigOptions>();
services.AddConfigurableOptions<CacheOptions>();
services.AddConfigurableOptions<OSSProviderOptions>();
services.AddConfigurableOptions<UploadOptions>();
services.AddConfigurableOptions<SysConfigOptions>();
// �?Email / SMS / OAuth / OCR �?
```

消费方式：参照库静�?`App.GetOptions<T>()`、`IOptions<T>`、`App.Configuration["Key"]`�?

`SysConfig.json` 示例�?

```json
{
  "SysConfig": {
    "Password": "123456",
    "TokenExpire": 10080,
    "RefreshTokenExpire": 20160,
    "OpLog": true,
    "SingleLogin": false,
    "SecondVer": false,
    "Captcha": false,
    "Watermark": false
  }
}
```

绑定�?`SysConfigOptions`（实现可配置 Options 契约，`PostConfigure` 补默认过期时间）�?

### 3.3 双轨：文�?Options vs DB 参数

| 轨道 | 来源 | 访问 | 变更方式 |
|------|------|------|----------|
| **A. 基础设施 / 强类�?* | `Configuration/*.json` | `IOptions` / `App.GetOptions` | 改文�?/ 部署；可热重�?|
| **B. 运行时产品参�?* | �?`HH_SysConfig` | `SysConfigService` + 缓存 | 管理�?CRUD |

要点�?

- Seed 常把文件里的开�?*镜像**�?DB，供参数管理页展示�?
- 部分热路径（登录/JWT/日志）实际仍�?**Options（文件）**；DB 路径与文件路径需在复刻时明确「谁权威」�?
- 个别引擎可旁路参照库扫描，自�?`new ConfigurationBuilder().AddJsonFile(...)`（如 NFlow `activiti.cfg.json`）�?

### 3.4 启动调用链（api10�?

```
Program: Serve.Run(...)
  �?参照库建 Host
  �?默认 appsettings
  �?AddJsonFiles（ScanJsonFiles=true�?
  �?Startup.ConfigureServices
       �?AddProjectOptions()
       �?App.GetOptions / App.Configuration 消费
```

Job 宿主同样声明扫描目录；部�?JSON �?Link 拷进 Job �?`Configuration/`�?

---

## 4. sleveen-admin 现状对照

| 能力 | 参照�?/ 参照宿主 | sleveen-admin 现状 |
|------|-------------------|-------------------|
| �?JSON 分片扫描合并 | `AddJsonFiles` | **�?*；几乎全�?`appsettings.json` |
| 环境覆盖 `{Name}.{Env}.json` | �?| **�?*（仅标准 `appsettings.{Env}.json`�?|
| 扫描目录声明 | `ConfigurationScanDirectories` | **�?* |
| 强类�?Options 批量注册 | `AddProjectOptions` | 有，但散落在 `Program` / 扩展方法，节都在 appsettings |
| 手工额外 JSON | �?| �?`Configration/llm.json`（拼写为 Configration�?|
| DB `SysConfig` | 双轨 + 缓存读取 | 扁平 `Code/Value` CRUD�?*业务几乎不读** |
| 对参照库的运行时依赖 | 强依�?| **不引�?*（PLAN：标�?WebApplication + 自研扩展�?|

现有 Options：`Jwt`、`DBCoonfig`（`Configuration/DBCoonfig.json`）、`Ai`、`FileStorage`、`Redis`、`Captcha`、`Frontend`、`Hangfire`（多�?Configuration 分片）�?

已有可复用模式：

- Portal：关系表组装 `JsonObject` + 内存缓存  
- AuthBag：多表配�?CRUD + `cleanCache`  
- 宿主：`AddJsonFile` 可选文件（`llm.json`�?

---

## 5. 新能力规划（admin 侧）

�?*不引用参照库**的前提下，规划下列能力包�?

### 5.1 P0 �?配置分片加载底座（复刻核心）

| 能力 | 说明 |
|------|------|
| **目录扫描合并** | `AddMooJsonShards`：扫�?ContentRoot / BaseDirectory + 可配置子目录 |
| **环境覆盖** | 支持 `{Name}.json` + `{Name}.{Environment}.json` |
| **排除规则** | 排除 `appsettings*`、运行时 junk、可配置 ignore 列表/glob |
| **构建拷贝** | System/Business �?`Configuration/*.json` �?`CopyToOutputDirectory` |
| **薄入�?appsettings** | 仅保留扫描策略、连接串密钥、本地覆盖；其余迁出分片 |
| **Options 约定** | `XxxOptions` �?节名 `Xxx`；扩展方�?`AddMooOptions<T>(section?)` |
| **热重�?* | `reloadOnChange: true` + 需要处�?`IOptionsMonitor` |

建议目录�?

```
SleveenAdmin.Web.Api/
  appsettings.json
  appsettings.Development.json
  Configuration/                 # 宿主本地分片（密钥覆盖、本机专用）
SleveenAdmin.System/ �?Foundation/
  Configuration/
    Jwt.json
    Captcha.json
    FileStorage.json
    �?
SleveenAdmin.Business/
  Configuration/
    Ai.json
```

建议 `appsettings.json` 元配置：

```json
{
  "ConfigurationScanDirectories": [ "Configuration", "" ],
  "IgnoreConfigurationFiles": []
}
```

### 5.2 P1 �?系统参数双轨打�?

| 能力 | 说明 |
|------|------|
| **文件权威 vs DB 权威策略** | 明确：基础设施只读文件；产品开关（验证码、水印、默认密码、Token 时长等）�?DB 为准或启动时文件→DB 种子 |
| **ConfigService 增强** | `GetValue<T>(code)` + 缓存；更新后失效 |
| **启动消费** | Auth / Captcha / 日志开关等改为读统一 `ISysRuntimeConfig`（内部可组合 Options + DB�?|
| **管理�?* | 现有 `/manage/config` 升级：分组、类型提示、敏感值脱敏、只读系统标�?|

### 5.3 P2 �?配置中心体验

| 能力 | 说明 |
|------|------|
| 分片清单 API | 列出已加�?JSON 路径与根节（调试用，生产可关�?|
| 租户覆盖（可选） | 租户�?KV 覆盖全局 DB 参数（文件层仍全局�?|
| 校验 | Options �?DataAnnotations / 启动 `ValidateOnStart` |
| 文档�?| 每个分片旁维护简�?README �?JSON Schema（可选） |

### 5.4 明确不做

- 不引用参照库运行时，也不照搬�?`Serve.Run` / 静�?`App.GetConfig` 门面（可用自研极�?`MooConfig.Get<T>` 若确有必要）
- 不递归扫描深层目录
- 不把密钥明文长期放在可被管理端改写的 DB（密钥留文件 / 环境变量 / 用户机密�?

---

## 6. 复刻设计（自研轻量实现）

### 6.1 目标 API（示意）

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddMooJsonShards(builder.Environment, options =>
{
    // �?appsettings 已声明，可省略；代码侧可追加
    // options.ScanDirectories.Add("Configuration");
});

builder.Services.AddMooProjectOptions(builder.Configuration);
```

```csharp
public static class MooProjectOptionsExtensions
{
    public static IServiceCollection AddMooProjectOptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptionsWithValidateOnStart<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.SectionName));
        // �?其余 Options
        return services;
    }
}
```

### 6.2 `AddMooJsonShards` 算法（对齐参照库行为，零参照库依赖）

1. 用当前已构建的配置读�?`ConfigurationScanDirectories`、`IgnoreConfigurationFiles`�? 
2. 解析目录：`BaseDirectory` �?`ContentRoot` �?�?root 下相对路�?�?绝对路径�? 
3. `GetFiles("*.json", TopDirectoryOnly)`，去重�? 
4. 过滤排除前缀/后缀�?ignore�? 
5. 按逻辑名分组：去掉 `.json` 后，若末段等于当�?`EnvironmentName` 则归入去掉末段后的组�? 
6. 每组仅加�?`{Key}.json`、`{Key}.{Env}.json`；同�?ContentRoot 优先；基文件先于环境文件�? 
7. `configurationBuilder.AddJsonFile(path, optional: true, reloadOnChange: true)`�?

放置位置建议：`SleveenAdmin.Foundation/Configuration/MooJsonShardExtensions.cs`（只依赖 `Microsoft.Extensions.Configuration.Json`）�?

### 6.3 �?appsettings 迁出分片的映射（首批�?

| �?appsettings �?| 目标分片文件 | Options |
|-------------------|--------------|---------|
| `Jwt` | `Configuration/Jwt.json` | `JwtOptions` |
| `Captcha` | `Configuration/Captcha.json` | `CaptchaOptions` |
| `FileStorage` | `Configuration/FileStorage.json` | `FileStorageOptions` |
| `Redis` | `Configuration/Redis.json` | `RedisOptions` |
| `Frontend` | `Configuration/Frontend.json` | `FrontendOptions` |
| `Hangfire` | `Configuration/Hangfire.json` | （现有或新建�?|
| `Ai` | `Business/.../Configuration/Ai.json` | Ai Options |
| `DBCoonfig` | `Configuration/DBCoonfig.json`（与 api10 一致） | `AddMooSql` / `DBCoonfigOptions` |
| `Logging` / `AllowedHosts` | 留在 appsettings | �?|

多节单文件示例（对齐参照宿主 `OSS.json`）：

```json
{
  "FileStorage": { "RootPath": "uploads", "MaxSizeMb": 20 },
  "Oss": { "Enabled": false, "Endpoint": "" }
}
```

### 6.4 DB 参数与文件对齐（SysConfig 复刻点）

参考参照宿主：

| 文件字段（SysConfigOptions�?| DB Code 建议 | 消费�?|
|------------------------------|--------------|--------|
| `Password` | `sys_password` | 重置用户密码默认�?|
| `TokenExpire` | `sys_token_expire` | �?Jwt 策略对齐（二选一权威源） |
| `RefreshTokenExpire` | `sys_refresh_token_expire` | 同上 |
| `Captcha` | `sys_captcha` | 登录是否校验验证�?|
| `OpLog` / `SingleLogin` / `Watermark` | 对应 code | 过滤�?/ 登录 / 前端水印 |

实施顺序建议�?*先做文件分片 + Options 消费稳定**，再�?Auth 等改为「DB 优先、文件默认」，最后增强管�?UI�?

### 6.5 �?PLAN 技术栈方向的关�?

PLAN 要求宿主走标�?`WebApplication.CreateBuilder` + `IOptions<T>`，而非参照库的一体式宿主入口与分片运行时�?

本文复刻的是 **「JSON 分片扫描/合并」能�?*：宿主仍�?`WebApplication.CreateBuilder`，以自研 `AddMooJsonShards` 补齐分片加载�?*只对齐行为，不引入参照库依赖**�?

---

## 7. 实施分期

| 阶段 | 内容 | 验收 |
|------|------|------|
| **C0** | 实现 `AddMooJsonShards` + 单元测试（分组、排除、环境覆盖、ContentRoot 优先�?| 测试覆盖核心规则 |
| **C1** | 迁出 Jwt/Captcha/FileStorage/Redis/Frontend/Hangfire/Ai 分片；`CopyToOutputDirectory`；Program 接入 | 启动行为与迁出前一�?|
| **C2** | `AddMooProjectOptions` 集中注册；关键路径改 `IOptionsMonitor`（可选） | �?JSON 热重载可观测 |
| **C3** | `ConfigService.GetValue<T>` + 缓存；Seed 默认参数；Auth/Captcha 消费 DB 或统一门面 | 管理端改参数即时生效 |
| **C4** | 配置页分�?脱敏；可选调试「已加载分片」API | 运维可用 |

---

## 8. 风险与注意点

1. **扫描非递归**：子目录必须写进 `ConfigurationScanDirectories`，不能指望深层自动发现�? 
2. **多点文件�?*：`A.B.Development.json` �?Env=Development 时组名为 `A.B`；避免随意多段命名�? 
3. **密钥**：OSS/DB/JWT SigningKey 不宜进可匿名读取的管�?API；分片文件权限与 `.gitignore` 本地覆盖（`*.local.json` + Ignore）要规划好�? 
4. **双权威冲�?*：Token 时长若同时存�?`Jwt:AccessTokenMinutes` �?DB `sys_token_expire`，必须文档规定优先级�? 
5. **拼写历史**：现�?`Configration/llm.json` 可在 C1 并入 `Configuration/Ai.json` 或保留兼容加载一次�? 
6. **BeeFlow / 工作�?*：引擎专有配置可继续单独 `AddJsonFile`，不必强行进分片扫描（对齐参照宿�?NFlow 特例）�?

---

## 9. 参考路径速查

| 说明 | 绝对路径 |
|------|----------|
| 参照库扫描引�?| `E:\coding\gitlab\my\hhny.net.base.pure\pure\src\App\Internal\InternalApp.cs` |
| 参照�?Options 绑定 | `...\ConfigurableOptions\Extensions\ConfigurableOptionsServiceCollectionExtensions.cs` |
| 参照库节路径 | `...\ConfigurableOptions\Internal\Penetrates.cs` |
| 参照宿主扫描声明 | `...\slickflow\api10\HHNY.NET.Web.Entry\appsettings.json` |
| 参照宿主 Options 注册 | `...\api10\HHNY.NET.Web.Core\ProjectOptions.cs` |
| 参照宿主分片目录 | `...\api10\HHNY.NET.Application\Configuration\` |
| admin 宿主配置 | `...\sleveen-admin\backend\core\SleveenAdmin.Web.Api\appsettings.json` |
| admin Program | `...\SleveenAdmin.Web.Api\Program.cs` |
| admin SysConfig | `...\SleveenAdmin.System\src\widgets\configs\Config\` |
| admin 建设方案 | `E:\coding\gitee\my\sleveen-admin\PLAN.md` |

---

## 10. 一句话摘要

**参照库用目录扫描把独�?JSON 合并�?`IConfiguration`；参照宿主用 `Configuration/` 分片 + Options 批量绑定 + 可�?DB 参数落地。sleveen-admin 应自研同等扫�?合并�?Options 约定，把臃肿 `appsettings` 拆开，并打�?SysConfig 运行时消费——复刻能力，不引入参照库依赖�?*
