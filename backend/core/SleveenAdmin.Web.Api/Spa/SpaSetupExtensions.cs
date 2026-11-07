using Microsoft.Extensions.FileProviders;
using SleveenAdmin.Foundation.Options;
using Yarp.ReverseProxy.Configuration;

namespace SleveenAdmin.Web.Api.Spa;

public static class SpaSetupExtensions
{
    public static IServiceCollection AddSleveenAdminFrontend(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // FrontendOptions 由宿主 AddMooProjectOptions 注册
        var options = configuration.GetSection(FrontendOptions.SectionName).Get<FrontendOptions>()
                      ?? new FrontendOptions();

        if (environment.IsDevelopment() && options.DevProxyEnabled)
        {
            var devServer = options.DevServerUrl.TrimEnd('/') + "/";
            services.AddReverseProxy()
                .LoadFromMemory(
                    [
                        new RouteConfig
                        {
                            RouteId = "vite-dev",
                            ClusterId = "vite-dev",
                            Match = new RouteMatch { Path = "{**catch-all}" },
                            Order = int.MaxValue
                        }
                    ],
                    [
                        new ClusterConfig
                        {
                            ClusterId = "vite-dev",
                            Destinations = new Dictionary<string, DestinationConfig>
                            {
                                ["vite"] = new() { Address = devServer }
                            }
                        }
                    ]);
        }

        return services;
    }

    /// <summary>生产模式静态资源中间件（须在 UseAuthentication 之前调用）。</summary>
    public static WebApplication UseSleveenAdminStaticFrontend(this WebApplication app)
    {
        if (!ShouldServeStaticFrontend(app))
            return app;

        var fileProvider = CreateStaticFileProvider(app);
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
        return app;
    }

    /// <summary>SPA fallback 或开发 YARP 代理（须在 MapControllers 之后调用）。</summary>
    public static WebApplication MapSleveenAdminFrontend(this WebApplication app)
    {
        var options = app.Configuration.GetSection(FrontendOptions.SectionName).Get<FrontendOptions>()
                      ?? new FrontendOptions();

        if (app.Environment.IsDevelopment() && options.DevProxyEnabled)
        {
            app.MapReverseProxy();
            return app;
        }

        if (!ShouldServeStaticFrontend(app))
            return app;

        var fileProvider = CreateStaticFileProvider(app);
        app.MapFallbackToFile(options.FallbackFile, new StaticFileOptions { FileProvider = fileProvider });
        return app;
    }

    private static bool ShouldServeStaticFrontend(WebApplication app)
    {
        var options = app.Configuration.GetSection(FrontendOptions.SectionName).Get<FrontendOptions>()
                      ?? new FrontendOptions();
        if (app.Environment.IsDevelopment() && options.DevProxyEnabled)
            return false;

        return Directory.Exists(ResolveStaticRoot(app, options));
    }

    private static PhysicalFileProvider CreateStaticFileProvider(WebApplication app)
    {
        var options = app.Configuration.GetSection(FrontendOptions.SectionName).Get<FrontendOptions>()
                      ?? new FrontendOptions();
        return new PhysicalFileProvider(ResolveStaticRoot(app, options));
    }

    private static string ResolveStaticRoot(WebApplication app, FrontendOptions options)
    {
        return Path.IsPathRooted(options.StaticRootPath)
            ? options.StaticRootPath
            : Path.Combine(app.Environment.ContentRootPath, options.StaticRootPath);
    }
}
