namespace SleveenAdmin.Foundation.Options;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    /// <summary>开发模式下将非 API 请求反向代理至 Vite 开发服务器。</summary>
    public bool DevProxyEnabled { get; set; }

    /// <summary>Vite 开发服务器地址。</summary>
    public string DevServerUrl { get; set; } = "http://localhost:9527";

    /// <summary>生产静态文件目录（相对 ContentRootPath 或绝对路径）。</summary>
    public string StaticRootPath { get; set; } = "wwwroot";

    /// <summary>SPA history 模式 fallback 页面。</summary>
    public string FallbackFile { get; set; } = "index.html";
}
