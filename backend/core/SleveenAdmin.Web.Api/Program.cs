using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SleveenAdmin.Business.Ai;
using SleveenAdmin.Business.Ai.Controller;
using SleveenAdmin.Business.Ai.Service;
using SleveenAdmin.Business.Portal;
using SleveenAdmin.Business.Portal.Controller;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Officeman.Ebook;
using SleveenAdmin.Officeman.Ebook.Controller;
using SleveenAdmin.Officeman.Ebook.Service;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Configuration;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Ids;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Options;
using SleveenAdmin.Foundation.Storage;
using SleveenAdmin.System;
using SleveenAdmin.System.Author;
using SleveenAdmin.System.Auth.Service;
using SleveenAdmin.System.Bootstrap;

using SleveenAdmin.System.User.Controller;
using SleveenAdmin.System.User.Service;
using SleveenAdmin.System.Online.Hubs;
using SleveenAdmin.System.Tenant.Controller;
using SleveenAdmin.Saiflow.Controller;
using SleveenAdmin.Saiflow.Hosting;
using SleveenAdmin.Web.Api.Auth;
using SleveenAdmin.Web.Api.Scheduling;
using SleveenAdmin.Web.Api.Swagger;
using SleveenAdmin.Web.Api.Spa;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddMooJsonShards(builder.Environment);
// 兼容旧路径 Configration/llm.json（ModelProvider 覆盖 Ai 密钥）
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "Configration", "llm.json"),
    optional: true,
    reloadOnChange: true);

builder.Services.AddMooProjectOptions(builder.Configuration);
builder.Services.AddMooSql(builder.Configuration);
builder.Services.AddSaiflowHost(builder.Configuration);
builder.Services.AddSleveenAdminSystem(builder.Configuration);
builder.Services.AddSleveenAdminAi(builder.Configuration);
builder.Services.AddSleveenAdminPortal();
builder.Services.AddSleveenAdminOfficeman();
builder.Services.AddSleveenAdminHangfire(builder.Configuration);
builder.Services.AddSignalR();

builder.Services.AddSleveenAdminServiceDiscovery(
    typeof(BizSysUserService),
    typeof(SysPortalService),
    typeof(AiChatService),
    typeof(AuthService),
    typeof(GuidV7Generator),
    typeof(LocalFileStorage),
    typeof(PermissionAuthorizationHandler),
    typeof(SleveenAdmin.System.AuthBag.Service.AuthBagService),
    typeof(BizSaEbMatService));

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt configuration is required.");

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
// Scrutor 可能未将 AuthorizationHandler 注册为 IAuthorizationHandler，导致 PermissionRequirement 无人处理 → 403 且断点不进 Handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                    context.HttpContext.Items["TokenExpired"] = true;
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted)
                    return;

                var expired = context.HttpContext.Items.ContainsKey("TokenExpired");
                var code = expired ? ApiResultCodes.TokenExpired : ApiResultCodes.Unauthorized;
                var msg = expired ? "Token 已过期" : "未授权";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResult.Fail(msg, code));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthRoles.SuperAdmin, policy =>
        policy.RequireAssertion(ctx => AuthPrincipalHelper.BypassesPermissionCheck(ctx.User)));
});

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();

    })
    .AddApplicationPart(typeof(BizSysUserListController).Assembly)
    .AddApplicationPart(typeof(AiChatController).Assembly)
    .AddApplicationPart(typeof(BizSysPortalListController).Assembly)
    .AddApplicationPart(typeof(BizSysTenantListController).Assembly)
    .AddApplicationPart(typeof(ProbeController).Assembly)
    .AddApplicationPart(typeof(BizSaEbMatListController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "sleveen-admin API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins("http://localhost:9527", "http://127.0.0.1:9527")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddSleveenAdminFrontend(builder.Configuration, builder.Environment);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.InitializeAsync();
        var portalDb = scope.ServiceProvider.GetRequiredService<PortalDbInitializer>();
        await portalDb.InitializeAsync();
        var ebookDb = scope.ServiceProvider.GetRequiredService<EbookDbInitializer>();
        await ebookDb.InitializeAsync();
        AuthorServices.Configure(scope.ServiceProvider.GetRequiredService<AuthCacheService>());
        logger.LogInformation("Database connection verified and seed completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database initialization skipped; check DBCoonfig:Connections (default SQLite App_Data/sleveenadmin.db).");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.CustomDocumentSerializer = new SwaggerUiCompatDocumentSerializer();
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "sleveen-admin API v1");
    });
    app.UseCors("DevCors");
}

app.UseSleveenAdminStaticFrontend();

var fileStorage = app.Services.GetRequiredService<LocalFileStorage>();
fileStorage.EnsureRootExists();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fileStorage.RootPath),
    RequestPath = fileStorage.PublicPathPrefix
});

app.UseAuthentication();
app.UseAuthorization();
app.UseSleveenAdminHangfire();
//app.UseMiddleware<VisitLogMiddleware>();
app.MapControllers();
app.MapHub<OnlineUserHub>("/hubs/online");
app.MapSleveenAdminFrontend();

await app.RunAsync();
