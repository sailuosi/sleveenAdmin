# sleveen-admin

基于 **.NET 10** 与 **Soybean Admin** 的现代化后台管理系统（仓库名 **sleveen-admin**）。以 Admin.NET 标准版为功能蓝本，采用自研 ORM **mooSQL** 与清晰的分层架构，提供认证授权、RBAC、多租户、基础设施运维等开箱即用的平台能力，并已接入独立引擎仓 **sailflow** 的流程工作台。

> **当前重点**：权限与基础场景打磨；**Sailflow 流程工作台**（设计 → 表单 → 发起/待办办理 → 运维）已挂载，按文档联调与 UAT  
> **引擎**：运行语义走独立仓 **sailflow**（`Start/Run/Reject/…`）

---

## 特性概览

| 模块 | 能力 |
|------|------|
| **认证授权** | JWT + RefreshToken 轮换、图形验证码、按钮级权限 |
| **RBAC** | 用户 / 角色 / 菜单 / 组织，动态路由与菜单 |
| **平台扩展** | 多租户隔离、公告与站内信、SignalR 在线用户、Hangfire 定时任务 |
| **基础设施** | 文件上传、操作/访问日志、服务器监控、Redis 键值管理 |
| **AI 助手** | Microsoft Agents Framework (MAF) 流式对话，SSE 接口 |
| **Sailflow 流程** | BPMN 设计器（`sf:` 扩展）、动态表单、待办/已办/抄送、发起与办理台、分类/模板/报表、规则集·AI 活动·知识库·Outbox |
| **部署** | YARP 开发代理、生产 SPA 静态托管、Docker 单端口部署 |

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
- **Element Plus** + **UnoCSS**（Soybean Admin Element 壳）
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
├── web/                             # 前端（Element Plus / Soybean）
│   └── src/widgets/sailflow/        # 流程菜单页、设计器、办理壳
├── doc/                             # 专题文档 + archive
├── PLAN.md                          # 现行架构与路线图
└── IMPLEMENTATION.md                # 指向 R1–R11 归档
```

**依赖方向**：`Business → Foundation`，`System → Foundation`，`Web.Api → System + Business + Foundation + Saiflow`；引擎在外部仓 **sailflow**。细则见 [PLAN.md](./PLAN.md)。

**设计原则**

- 业务 Service 用具体类，禁止 Java 式 `IUserService` / `UserServiceImpl` 堆叠
- 域内 `DBLayer → Service → Controller` 纵向切片；配置拆 `Configuration/*.json`
- 前端为 Soybean **深度改造副本**（Element Plus + Biz/VC 范式），非上游 Naive 原样
- 主键 **Guid v7**，列名带表前缀；流程身份键对齐 **`SysUser.Account`**

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

流程菜单需在「系统管理 → 菜单管理」按 [doc/sailflow-menu-mount.md](./doc/sailflow-menu-mount.md) 挂载。

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

下一阶段以 **AI、审批工作流优化、AI 开发文档齐备** 为主，保障业务分层清晰、堆叠不崩（详见 [PLAN.md](./PLAN.md) §7）：

- [ ] **AI** — 会话持久化、知识库联动、可配置 Agent 边界
- [ ] **审批工作流优化** — 办理闭环 UAT、选人与办理体验、设计器/表单缺口收口
- [ ] **AI 开发文档齐备** — 范式/配置/挂菜单手册与 Agent 规则对齐
- [ ] **平台稳定性** — 数据权限、Seed/菜单一致性、审计（并行）

R1–R11 平台底座实施记录：[doc/archive/IMPLEMENTATION-R1-R11.md](./doc/archive/IMPLEMENTATION-R1-R11.md)。

---

## 相关文档

| 文档 | 说明 |
|------|------|
| [PLAN.md](./PLAN.md) | 架构约束、Soybean 改造、分层/配置、未来路线图 |
| [doc/archive/IMPLEMENTATION-R1-R11.md](./doc/archive/IMPLEMENTATION-R1-R11.md) | R1–R11 实施归档 |
| [doc/biz-module-paradigm.md](./doc/biz-module-paradigm.md) | 业务模块 Page / VC / Biz 范式 |
| [doc/mooSQL-integration.md](./doc/mooSQL-integration.md) | mooSQL 在本仓的用法 |
| [doc/sailflow-features.md](./doc/sailflow-features.md) | **Sailflow 现行能力**（产品/接入/开发） |
| [doc/sailflow-host-capability-pack.md](./doc/sailflow-host-capability-pack.md) | Host 能力包：`UseDebugConsoleHook` / Leave / Portal 等 |
| [doc/sailflow-menu-mount.md](./doc/sailflow-menu-mount.md) | 流程菜单挂载字段表 |
| [doc/sailflow-nflowable-feature-replication.md](./doc/sailflow-nflowable-feature-replication.md) | 流程产品面复刻规划与进度 |
| [doc/sailflow-assignee-candidate-design.md](./doc/sailflow-assignee-candidate-design.md) | 待办人 / 候选人配置 |
| [doc/sailflow-buttons-capability.md](./doc/sailflow-buttons-capability.md) | 办理按钮 `sf:ButtonsSetting` |
| [doc/sailflow-designer-sidebar-gap.md](./doc/sailflow-designer-sidebar-gap.md) | 设计器侧栏 / 齿轮弹窗缺口 |
| [doc/sailflow-portal-todo-push.md](./doc/sailflow-portal-todo-push.md) | 待办推送集成平台（统一日程） |

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
