using Hangfire;
using Hangfire.Dashboard;
using mooSQL.data;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Sailflow.Jobs;
using SleveenAdmin.System.Jobs;

namespace SleveenAdmin.Web.Api.Scheduling;

public static class HangfireSetupExtensions
{
    public static IServiceCollection AddSleveenAdminHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>() ?? new HangfireOptions();
        if (!options.Enabled)
            return services;

        services.AddHangfire((sp, config) =>
        {
            var cash = sp.GetRequiredService<DBInsCash>();
            var db = cash.getInstance(MooSqlSetup.DefaultPosition);
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMooSqlStorage(db, opt =>
                {
                    opt.TablesPrefix = options.TablesPrefix;
                    opt.PrepareSchemaIfNecessary = true;
                });
        });

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
}
