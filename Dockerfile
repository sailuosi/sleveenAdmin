# syntax=docker/dockerfile:1

# ---- 前端构建 ----
FROM node:22-alpine AS web-build
WORKDIR /src/web
RUN corepack enable
COPY web/package.json web/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY web/ ./
RUN pnpm build

# ---- 后端发布 ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY moo-sqlm/mooSQL2024/ext/mooSQL.Ext.csproj moo-sqlm/mooSQL2024/ext/
COPY admin/sleveenAdmin/backend/core/SleveenAdmin.Foundation/SleveenAdmin.Foundation.csproj admin/sleveenAdmin/backend/core/SleveenAdmin.Foundation/
COPY admin/sleveenAdmin/backend/core/SleveenAdmin.System/SleveenAdmin.System.csproj admin/sleveenAdmin/backend/core/SleveenAdmin.System/
COPY admin/sleveenAdmin/backend/core/SleveenAdmin.Business/SleveenAdmin.Business.csproj admin/sleveenAdmin/backend/core/SleveenAdmin.Business/
COPY admin/sleveenAdmin/backend/core/SleveenAdmin.Web.Api/SleveenAdmin.Web.Api.csproj admin/sleveenAdmin/backend/core/SleveenAdmin.Web.Api/
COPY admin/sleveenAdmin/backend/sleveenAdmin.sln admin/sleveenAdmin/backend/
RUN dotnet restore admin/sleveenAdmin/backend/sleveenAdmin.sln
COPY moo-sqlm/ moo-sqlm/
COPY admin/sleveenAdmin/ admin/sleveenAdmin/
COPY --from=web-build /src/web/dist admin/sleveenAdmin/backend/core/SleveenAdmin.Web.Api/wwwroot/
RUN dotnet publish admin/sleveenAdmin/backend/core/SleveenAdmin.Web.Api/SleveenAdmin.Web.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- 运行镜像 ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=api-build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SleveenAdmin.Web.Api.dll"]
