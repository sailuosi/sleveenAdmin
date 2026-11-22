using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace SleveenAdmin.Foundation.Diagnostics;

/// <summary>注册 <see cref="DevConsoleFormatter"/> 作为控制台格式化器。</summary>
public static class DevConsoleLoggingExtensions
{
    /// <summary>将控制台日志改为 DevConsole 分节块（建议仅 Development）。</summary>
    public static ILoggingBuilder AddDevConsoleFormatter(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddConsole(options =>
        {
            options.FormatterName = DevConsoleFormatter.FormatterName;
        });
        builder.AddConsoleFormatter<DevConsoleFormatter, ConsoleFormatterOptions>();
        return builder;
    }
}
