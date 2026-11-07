using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using mooSQL.config;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.Options;
using SleveenAdmin.Saiflow.Jobs;
using SleveenAdmin.System.Jobs;

namespace SleveenAdmin.Web.Api.Scheduling;

public static class HangfireSetupExtensions
{
    public static IServiceCollection AddSleveenAdminHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>() ?? new HangfireOptions();
        if (!options.Enabled)
            return services;

        var master = ResolveMasterConnection(configuration);
        if (IsSqlite(master.DbType))
        {
            throw new InvalidOperationException(
                "Hangfire 当前使用 SQL Server 存储，不能与 SQLITE 主库同时启用。" +
                "请将 Hangfire:Enabled 设为 false，或将 DBCoonfig 主库改为 MSSQL。");
        }

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(master.ConnectString, new SqlServerStorageOptions
            {
                SchemaName = options.SchemaName,
                PrepareSchemaIfNecessary = true
            }));

        services.AddHangfireServer();
        services.AddScoped<RefreshTokenCleanupJob>();
        services.AddScoped<SailProcessArchiveJob>();
        return services;
    }

    public static WebApplication UseSleveenAdminHangfire(this WebApplication app)
    {
        var options = app.Configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>() ?? new HangfireOptions();
        if (!options.Enabled)
            return app;

        app.UseHangfireDashboard(options.DashboardPath, new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()]
        });

        RecurringJob.AddOrUpdate<RefreshTokenCleanupJob>(
            "refresh-token-cleanup",
            job => job.ExecuteAsync(),
            options.RefreshTokenCleanupCron);

        if (options.SailProcessArchiveEnabled)
        {
            RecurringJob.AddOrUpdate<SailProcessArchiveJob>(
                "sail-process-archive",
                job => job.ExecuteAsync(),
                options.SailProcessArchiveCron);
        }

        return app;
    }

    private static DBPosition ResolveMasterConnection(IConfiguration configuration)
    {
        var dbConfig = configuration.GetSection(DBCoonfigOptions.SectionName).Get<DBCoonfigOptions>()
                       ?? throw new InvalidOperationException("DBCoonfig configuration is required.");
        return dbConfig.Connections.FirstOrDefault(c => c.Position == MooSqlSetup.DefaultPosition)
               ?? dbConfig.Connections.FirstOrDefault()
               ?? throw new InvalidOperationException("DBCoonfig master connection is required.");
    }

    private static bool IsSqlite(string? dbType) =>
        string.Equals(dbType, "SQLITE", StringComparison.OrdinalIgnoreCase);
}
