using System.Text.RegularExpressions;
using sailflow.engine.Logging;
using sailflow.msg.Logging;
using SleveenAdmin.Foundation.Diagnostics;

namespace SleveenAdmin.Sailflow.Hosting;

/// <summary>
/// Sleveen 增强控制台日志：继承 <see cref="ConsoleSailLog"/>，用 <see cref="DevConsole"/> 带标题分节输出。
/// 消息中若含插件 <c>「Title」</c>，块标题为
/// <c>插件 · 流程回调 · {Title} · 同步/异步</c>（见 <see cref="SailLogCategories.PluginBlockTitle"/>）。
/// </summary>
public class SleveenDevConsoleSailLog : ConsoleSailLog
{
    static readonly Regex PluginTitleRegex = new(@"「([^」]+)」", RegexOptions.Compiled);

    public SleveenDevConsoleSailLog(Func<SailLogOptions> options, ConsoleSailLogOptions? self = null)
        : base(options, self)
    {
    }

    public SleveenDevConsoleSailLog(SailLogOptions options, ConsoleSailLogOptions? self = null)
        : base(options, self)
    {
    }

    protected override void Emit(
        SailLogLevel level,
        string category,
        string message,
        string formattedLine,
        Exception? ex)
    {
        var content = message ?? "";
        var pluginTitle = TryExtractPluginTitle(ref content);
        var blockTitle = SailLogCategories.PluginBlockTitle(category, pluginTitle);
        var title = !string.IsNullOrWhiteSpace(blockTitle)
            ? blockTitle!
            : TitleFor(level, category);
        var description = !string.IsNullOrWhiteSpace(blockTitle)
            ? LevelLabel(level)
            : string.IsNullOrWhiteSpace(category)
                ? LevelLabel(level)
                : $"{LevelLabel(level)} ／ {category}";

        var lines = new List<string>(4)
        {
            DevConsole.Field("内容", content),
        };
        if (ex != null)
        {
            lines.Add(DevConsole.Field("异常", ex.GetType().Name));
            lines.Add(DevConsole.Field("详情", ex.Message));
        }

        try
        {
            var block = DevConsole.Format(title, description, lines.ToArray());
            WriteConsoleLine(level, block);
        }
        catch
        {
            // DevConsole 失败时退回基类单行
            base.Emit(level, category, message, formattedLine, ex);
        }
    }

    /// <summary>从消息取出首个 <c>「Title」</c>，并从内容中去掉书名号段（避免标题重复）。</summary>
    static string? TryExtractPluginTitle(ref string content)
    {
        if (string.IsNullOrEmpty(content))
            return null;

        var m = PluginTitleRegex.Match(content);
        if (!m.Success)
            return null;

        var title = m.Groups[1].Value;
        content = PluginTitleRegex.Replace(content, "", 1).Trim();
        content = Regex.Replace(content, @"\s{2,}", " ");
        return string.IsNullOrWhiteSpace(title) ? null : title;
    }

    static string TitleFor(SailLogLevel level, string? category)
    {
        var kind = SailLogCategories.KindLabel(category);
        var cat = string.IsNullOrWhiteSpace(kind)
            ? (string.IsNullOrWhiteSpace(category) ? "sail" : category.Trim())
            : kind;
        return level switch
        {
            SailLogLevel.Error => $"Sail 错误 · {cat}",
            SailLogLevel.Warning => $"Sail 警告 · {cat}",
            SailLogLevel.Message => $"Sail 消息 · {cat}",
            SailLogLevel.Debug => $"Sail 调试 · {cat}",
            SailLogLevel.Trace => $"Sail 追踪 · {cat}",
            _ => $"Sail · {cat}",
        };
    }

    static string LevelLabel(SailLogLevel level) =>
        level switch
        {
            SailLogLevel.Trace => "Trace",
            SailLogLevel.Debug => "Debug",
            SailLogLevel.Message => "Message",
            SailLogLevel.Warning => "Warning",
            SailLogLevel.Error => "Error",
            _ => level.ToString(),
        };
}
