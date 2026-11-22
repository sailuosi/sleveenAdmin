using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace SleveenAdmin.Foundation.Diagnostics;

/// <summary>
/// 将 MEL 控制台日志渲染为 <see cref="DevConsole"/> 分节块（开发核验用）。
/// </summary>
public sealed class DevConsoleFormatter : ConsoleFormatter
{
    public const string FormatterName = "sleveen-dev";

    public DevConsoleFormatter() : base(FormatterName)
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message) && logEntry.Exception is null)
            return;

        var shortCategory = ShortCategory(logEntry.Category);
        var title = TitleFor(logEntry.LogLevel, shortCategory);
        var description = LevelLabel(logEntry.LogLevel);

        var lines = new List<string>(4);
        if (!string.IsNullOrEmpty(message))
            lines.Add(DevConsole.Field("内容", message));

        if (logEntry.Exception is { } ex)
        {
            lines.Add(DevConsole.Field("异常", ex.GetType().Name));
            lines.Add(DevConsole.Field("详情", ex.Message));
        }

        textWriter.WriteLine(DevConsole.Format(title, description, lines.ToArray()));
    }

    internal static string ShortCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return "host";

        var trimmed = category.Trim();
        var lastDot = trimmed.LastIndexOf('.');
        return lastDot < 0 || lastDot == trimmed.Length - 1
            ? trimmed
            : trimmed[(lastDot + 1)..];
    }

    internal static string TitleFor(LogLevel level, string shortCategory) =>
        level switch
        {
            LogLevel.Error or LogLevel.Critical => $"宿主 错误 · {shortCategory}",
            LogLevel.Warning => $"宿主 警告 · {shortCategory}",
            LogLevel.Debug or LogLevel.Trace => $"宿主 调试 · {shortCategory}",
            _ => $"宿主 · {shortCategory}",
        };

    static string LevelLabel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => "Trace",
            LogLevel.Debug => "Debug",
            LogLevel.Information => "Information",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Error",
            LogLevel.Critical => "Critical",
            _ => level.ToString(),
        };
}
