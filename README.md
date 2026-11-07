# sleveen-admin

基于 **.NET 10** 与 **Soybean Admin** 的现代化后台管理系统（仓库名 **sleveen-admin**）。以 Admin.NET 标准版为功能蓝本，采用自研 ORM **mooSQL** 与清晰的分层架构，提供认证授权、RBAC、多租户、基础设施运维等开箱即用的平台能力；前端深度改造为 Element Plus + Biz/VC 范式，并接入独立引擎仓 **sailflow** 的流程工作台。

> **当前重点**：权限与基础场景打磨；**无界壳 + 门户工作台**一体感；**Sailflow 流程工作台**联调与 UAT  
> **引擎**：运行语义走独立仓 **sailflow**（`Start/Run/Reject/…`）  
> **壳层**：默认主题「左栏无界」；首页等为无界 Vue + 门户装配（详见 [doc/无界模式/无界模式.md](./doc/无界模式/无界模式.md)）

---

## 特性概览

| 模块 | 能力 |
|------|------|
| **认证授权** | JWT + RefreshToken 轮换、图形验证码、按钮级权限 |
| **RBAC** | 用户 / 角色 / 菜单 / 组织，动态路由与菜单 |
| **平台扩展** | 多租户隔离、公告与站内信、SignalR 在线用户、Hangfire 定时任务 |
| **基础设施** | 文件上传、操作/访问日志、服务器监控、Redis 键值管理 |
| **无界壳** | 左栏无界布局（`vertical-card`）、无界 Vue 页（`menuType=4`）、悬浮侧栏 / 压缩顶栏页签、内容背景可铺满全屏 |
| **门户工作台** | `paotal` 可配置卡片网格；首页 / 业务子门户外壳；与无界组合达到菜单与内容浑然一体 |
| **AI 助手** | Microsoft Agents Framework (MAF) 流式对话，SSE 接口 |
| **Sailflow 流程** | BPMN 设计器（`sf:` 扩展）、动态表单、待办/已办/抄送、发起与办理台、分类/模板/报表、规则集·AI 活动·知识库·Outbox |
| **部署** | YARP 开发代理、生产 SPA 静态托管、Docker 单端口部署 |

### 无界理念（摘要）

打破传统 Admin「菜单栏 · 页签栏 · 内容栏」三层下巴的臃肿感，让菜单与内容区干净整洁、浑然一体。无界页压缩顶栏与页签，侧栏以悬浮卡片融入同一画幅，内容组件背景可铺满屏幕。完整说明见 [无界模式专题](./doc/无界模式/无界模式.md)。

---

## 技术栈

### 后端

- **.NET 10**（`net10.0`）
- **mooSQL.Ext** — ORM，开发默认 SQLite，可切换 SQL Server 等
- **Scrutor** — 约定式依赖注入（`*Service` + 生命周期标记）
- **JWT Bearer** + 自定义 `PermissionAuthorizationHandler`
- **Hangfire** — SQL Server 存储的定时任务（默认关闭；主库为 SQLite 时不可启用）
- **StackExchange.Redis** — 缓存与监控
- **YARP** — 内嵌反向代理，单端口联调/部署
- **Mapster** — 对象映射
- **sailflow** — 独立解决方案引擎；本仓 `backend/saiflow` 为 Host 薄适配（`/api/Wf2Xml`、`/api/sail/`*）

### 前端

- **Vue 3** + **Vite 7** + **TypeScript**
- **Element Plus** + **UnoCSS**（Soybean Admin Element 壳，深度改造）
- **门户引擎** — `@/ui/portal/paotal.vue` + Portlet VC；首页 `homePage` + `BorderlessDiv`
- **vue-element-plus-x** — AI 助手流式 UI
- **SignalR** — 在线用户实时连接
- **widgets/sailflow** — 流程工作台（待办办理壳 + BPMN 设计器，API 为 Sail 形状）

---

## 架构

```
sleveen-admin/
├── backend/                         # 后端（sln + 源码 + 测试）
│   ├── sleveenAdmin.sln
│   ├── core/
│   │   ├── SleveenAdmin.Foundation/ # ApiResult、分页、mooSQL、安全
│   │   ├── SleveenAdmin.System/     # Auth / User / Role / Menu / Org / Dict …
│   │   ├── SleveenAdmin.Business/   # AI 等业务扩展
│   │   └── SleveenAdmin.Web.Api/    # 宿主：Program、JWT、中间件
│   ├── saiflow/                     # Sailflow Host：Wf2Xml、办理聚合、Assignee 适配
│   └── tests/SleveenAdmin.Tests/
├── web/                             # 前端（Element Plus / Soybean 改造）
│   └── src/
│       ├── layouts/                 # base-layout：左栏无界 / 无界页编排
│       ├── ui/portal/               # paotal 门户引擎
│       ├── views/home/              # 首页无界 + 门户外壳
│       └── widgets/                 # 业务域（含 sailflow、officeman …）
├── doc/
│   ├── 无界模式/无界模式.md         # 左栏无界 / 无界 Vue / 门户 / 背景穿透
│   ├── pc/design/                   # 前端设计规范与实施计划
│   ├── pc/audit/                    # 审计要求
│   ├── sailflow-*.md                # 流程接入与能力说明
│   └── archive/                     # R1–R11 等归档
├── PLAN.md                          # 现行架构与路线图
└── IMPLEMENTATION.md                # 指向 R1–R11 归档
```

**依赖方向**：`Business → Foundation`，`System → Foundation`，`Web.Api → System + Business + Foundation + Saiflow`；引擎在外部仓 **sailflow**。细则见 [PLAN.md](./PLAN.md)。

**设计原则**

- 业务 Service 用具体类，禁止 Java 式 `IUserService` / `UserServiceImpl` 堆叠
- 域内 `DBLayer → Service → Controller` 纵向切片；配置拆 `Configuration/*.json`
- 前端为 Soybean **深度改造副本**（Element Plus + Biz/VC 范式），非上游 Naive 原样
- 主键 **Guid v7**，列名带表前缀；流程身份键对齐 **`SysUser.Account`**
- 壳层优先「无界」观感：菜单与内容同一画幅，避免多余实色嵌套壳

---

## 功能清单

以下为种子菜单 / 内置页面能力（随 Seed 幂等写入，可在「菜单管理」调整）。隐藏路由（设计器、详情等）不列入侧栏展示项。

### 首页与壳

| 功能 | 说明 |
|------|------|
| **首页** | 无界 Vue + 门户装配（`homePage` → `paotal`），氛围底铺满 |
| **左栏无界布局** | 默认主题 `vertical-card`：圆角卡片侧栏、内容整页 |
| **无界页面形态** | 菜单类型 `menuType=4`：悬浮侧栏、压缩顶栏/页签、内容背景可全屏 |

### 系统管理

| 功能 | 说明 |
|------|------|
| 用户管理 | 账号与资料 |
| 菜单管理 | 路由 / 组件 / 权限标识 / 菜单类型（含无界 Vue） |
| 组织管理 | 组织树 |
| 字典管理 | 数据字典 |
| 配置管理 | 系统配置 |
| 权限配置 | AuthBag 权限包（查看 / 编辑） |

### 平台扩展

| 功能 | 说明 |
|------|------|
| 租户管理 | 多租户 |
| 通知管理 / 我的通知 | 公告与站内信 |
| 在线用户 | SignalR 在线列表 |
| 定时任务 | Hangfire 控制台入口（主库 MSSQL 且启用时） |

### 基础设施

| 功能 | 说明 |
|------|------|
| 文件管理 | 上传与文件元数据 |
| 操作日志 / 访问日志 | 审计日志 |
| 服务器监控 | 主机指标 |
| Redis 管理 | 键值浏览 |

### 门户管理

| 功能 | 说明 |
|------|------|
| 门户配置 | 门户方案 |
| 页面管理 | 门户页 |
| 组件库 | Portlet / 小程序控件 |
| 门户设计 | 可视化设计器 |

### AI

| 功能 | 说明 |
|------|------|
| AI 助手 | MAF 流式对话（SSE） |

### 流程引擎（Sailflow）

| 分组 | 功能 |
|------|------|
| **办理** | 我的待办、我的已办、抄送我的、发起流程、我的流程 |
| **管理** | 任务管理、流程实例管理、流程定义、流程表单 |
| **特色** | 规则集、AI 模型、知识库、回调死信、流程模板库、请假样例 |
| **运维** | 流程分类、流程报表 |
| **设计入口** | 流程设计器 / 表单设计器 / 流程详情 / 流程编辑（侧栏常隐藏，从列表跳入） |

挂载字段与路径见 [doc/sailflow-menu-mount.md](./doc/sailflow-menu-mount.md)。

### 创作工作台（officeman 示例域）

| 功能 | 说明 |
|------|------|
| 素材库 / 选题池 / 笔记 / 分类设置 | 电子书创作域 Biz 示例，可作业务子门户扩展参照 |

### 平台横切能力（非菜单）

| 能力 | 说明 |
|------|------|
| 认证授权 | JWT + RefreshToken、图形验证码、按钮级权限 |
| 动态路由 | 登录菜单树驱动前端路由 |
| 门户引擎 | `paotal` 可配置卡片网格与个性化布局 |
| 单端口部署 | YARP 开发代理 / 生产 SPA 静态托管 / Docker |

---

## 快速开始

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+ 与 [pnpm](https://pnpm.io/)
- （可选）Redis，用于监控与键值管理
- （可选）SQL Server — 仅在切换主库为 `MSSQL` 并启用 Hangfire 时需要
- 同级或可引用的 **sailflow** 引擎仓（`backend/saiflow` 已 ProjectReference）

### 1. 数据库（默认 SQLite，开箱即用）

默认配置已指向文件库 `App_Data/sleveenadmin.db`（相对 Web.Api 内容根）。`dotnet run` 时会自动创建目录、建表并写入 Seed，无需安装外部数据库。

默认超级管理员：

| 账号 | 密码 |
|------|------|
| `admin` | `123456` |

默认连接配置：`Configuration/DBCoonfig.json`（节点 `DBCoonfig`）。如需改用 SQL Server 并开启 Hangfire，可覆盖该分片或通过环境变量：

```json
{
  "DBCoonfig": {
    "Connections": [
      {
        "Position": 0,
        "Name": "Master",
        "DbType": "MSSQL",
        "ConnectString": "Enlist=false;Data Source=localhost;Database=sleveenadmin;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True;",
        "Version": "13.0"
      }
    ]
  },
  "Hangfire": {
    "Enabled": true
  }
}
```

主库为 `SQLite` 时请保持 `Hangfire:Enabled=false`（Hangfire 当前仅支持 SQL Server 存储）。

### 2. 启动后端

```bash
cd backend/core/SleveenAdmin.Web.Api
dotnet run
```

- **SleveenAdmin.Web.Api** profile：仅 API + Swagger → `http://localhost:5000/swagger`
- **SleveenAdmin.Unified** profile：单端口联调，非 API 请求代理到 Vite 前端

### 3. 启动前端（独立开发）

```bash
cd web
pnpm install
pnpm dev
```

前端默认运行于 `http://localhost:9527`，API 代理到 `http://localhost:5000`。

流程菜单需在「系统管理 → 菜单管理」按 [doc/sailflow-menu-mount.md](./doc/sailflow-menu-mount.md) 挂载。无界页在菜单中选择类型「无界 Vue 组件」（`menuType=4`），说明见 [无界模式](./doc/无界模式/无界模式.md)。

### 4. 生产 / Docker

```bash
# 构建前端 + 单端口运行
cd web && pnpm build
cd ../backend/core/SleveenAdmin.Web.Api && dotnet run --environment Production

# 或使用 Docker（映射 8080）
cd ../..
docker compose up --build
```

### 5. 运行测试

```bash
dotnet test
```

---

## API 约定

- 统一响应：`{ "code": "0000", "msg": "...", "data": ... }`（平台 API）
- Sailflow Host：`ResponseResult` / `Entity`（`/api/Wf2Xml/*`、`/api/sail/*`）
- 认证头：`Authorization: Bearer <token>`，可辅 `X-Token` / `X-Refresh-Token`
- 动态路由：`GET /api/auth/user-info`、`GET /api/sysMenu/login-menu-tree`

---

## 路线图

总原则：在 AI 辅助开发场景下，**业务分层清晰、堆叠不崩**；壳层继续向「无界一体」演进。细则见 [PLAN.md](./PLAN.md) §7。

### 已落地（底座与壳）

- [x] R1–R11 平台底座（RBAC、基础设施、YARP、AuthForm、MAF AI 骨架等）— [归档](./doc/archive/IMPLEMENTATION-R1-R11.md)
- [x] Sailflow 流程工作台挂载（设计 / 表单 / 待办办理等，见 `doc/sailflow-*.md`）
- [x] **左栏无界**默认布局（`vertical-card`）+ App 根布局 class
- [x] **无界 Vue**（`menuType=4`）：悬浮侧栏、压缩顶栏/页签、mainpan 穿透
- [x] **首页门户**：`homePage` 外层氛围底 + `BorderlessDiv` + `paotal` 穿透
- [x] 无界专题文档：[doc/无界模式/无界模式.md](./doc/无界模式/无界模式.md)

### 进行中 / 下一阶段

| 轨道 | 要点 |
|------|------|
| **无界与门户** | 业务子门户 `Biz_*Portal` 按规范落地；更多菜单切无界页；氛围一体感与卡片玻璃打磨；门户切换缓存 |
| **AI** | 会话持久化与历史；知识库/流程活动联动；Agent 工具边界；模型/指令可配置 |
| **审批工作流** | 办理闭环 UAT；选人/下一跳/按钮；待办人与宿主主数据对齐；设计器与表单缺口收口 |
| **开发文档** | widgets 新增 / 挂菜单 / Configuration 分片短手册；Cursor 规则与范式对齐；文档交叉索引 |
| **平台稳定性** | 数据权限与租户边界；Seed/菜单/权限标识一致；日志审计与运维体验（并行） |

---

## 相关文档

### 壳层与前端设计

| 文档 | 说明 |
|------|------|
| [doc/无界模式/无界模式.md](./doc/无界模式/无界模式.md) | **无界理念**、左栏无界 / 无界 Vue / 门户模式、背景穿透与选择器 |
| [doc/pc/design/业务子门户工作台实施计划.md](./doc/pc/design/业务子门户工作台实施计划.md) | 业务子门户外壳 + `paotal` 标准路径 |
| [doc/pc/design/代码风格规范.md](./doc/pc/design/代码风格规范.md) | 前后端代码风格与落位 |
| [doc/pc/design/biz-module-paradigm.md](./doc/pc/design/biz-module-paradigm.md) | 业务模块 Page / VC / Biz 范式（design 目录） |
| [doc/biz-module-paradigm.md](./doc/biz-module-paradigm.md) | 业务模块范式（仓根摘要/副本） |
| [doc/pc/design/选择器设计规范.md](./doc/pc/design/选择器设计规范.md) | 选择器设计 |
| [doc/pc/design/树查询.md](./doc/pc/design/树查询.md) / [数据库查询.md](./doc/pc/design/数据库查询.md) | 查询约定 |
| [doc/pc/audit/审计要求.md](./doc/pc/audit/审计要求.md) | 审计总册 |

### 平台与架构

| 文档 | 说明 |
|------|------|
| [PLAN.md](./PLAN.md) | 架构约束、Soybean 改造、分层/配置、未来路线图 |
| [doc/archive/IMPLEMENTATION-R1-R11.md](./doc/archive/IMPLEMENTATION-R1-R11.md) | R1–R11 实施归档 |
| [doc/mooSQL-integration.md](./doc/mooSQL-integration.md) | mooSQL 在本仓的用法 |

### Sailflow 流程

| 文档 | 说明 |
|------|------|
| [doc/sailflow-features.md](./doc/sailflow-features.md) | **Sailflow 现行能力**（产品/接入/开发） |
| [doc/sailflow-host-capability-pack.md](./doc/sailflow-host-capability-pack.md) | Host 能力包：`UseDebugConsoleHook` / Leave / Portal 等 |
| [doc/sailflow-menu-mount.md](./doc/sailflow-menu-mount.md) | 流程菜单挂载字段表 |
| [doc/sailflow-nflowable-feature-replication.md](./doc/sailflow-nflowable-feature-replication.md) | 流程产品面复刻规划与进度 |
| [doc/sailflow-assignee-candidate-design.md](./doc/sailflow-assignee-candidate-design.md) | 待办人 / 候选人配置 |
| [doc/sailflow-buttons-capability.md](./doc/sailflow-buttons-capability.md) | 办理按钮 `sf:ButtonsSetting` |
| [doc/sailflow-designer-sidebar-gap.md](./doc/sailflow-designer-sidebar-gap.md) | 设计器侧栏 / 齿轮弹窗缺口 |
| [doc/sailflow-portal-todo-push.md](./doc/sailflow-portal-todo-push.md) | 待办推送集成平台（统一日程） |
| [doc/sailflow-form-biz-handler.md](./doc/sailflow-form-biz-handler.md) | 表单业务处理器 |
| [doc/sailflow-field-modes.md](./doc/sailflow-field-modes.md) | 字段模式 |

引擎仓另有《特性说明-引擎能力基线》《特性说明-待办人管道》等，以 **sailflow** 仓库 `doc/sail/` 为准。

---

## 致谢

- 功能蓝本参考 [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET)
- 前端基于 [Soybean Admin](https://github.com/soybeanjs/soybean-admin)
- ORM 来自同仓系 [mooSQL](../../moo-sqlm/mooSQL2024/)
- 流程引擎为独立仓 **sailflow**

---

## License

待定 / 与主仓库保持一致。
