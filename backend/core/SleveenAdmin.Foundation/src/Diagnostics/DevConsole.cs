using System.Text;

namespace SleveenAdmin.Foundation.Diagnostics;

/// <summary>
/// TP 风格美化控制台（无 Furion 依赖）。分节标题 + 描述 + <c>##标签## 值</c> 行。
/// 供开发核验；生产噪声场景勿用。
/// </summary>
public static class DevConsole
{
    /// <summary>组装并打印一块美化文本。</summary>
    public static void WriteBlock(string title, string? description, params string[] lines)
    {
        Console.WriteLine(Format(title, description, lines));
    }

    /// <summary>仅格式化为字符串（便于测试）。</summary>
    public static string Format(string title, string? description, params string[] lines)
    {
        var head = string.IsNullOrWhiteSpace(title) ? "消息" : title.Trim();
        var sb = new StringBuilder(256);
        sb.Append("╭─ ").Append(head).Append(' ');
        var pad = Math.Max(8, 48 - head.Length);
        sb.Append('─', pad).AppendLine();
        if (!string.IsNullOrWhiteSpace(description))
            sb.Append("│ ").Append(description.Trim()).AppendLine();
        if (lines is { Length: > 0 })
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                sb.Append("│ ").Append(line.Trim()).AppendLine();
            }
        }

        sb.Append('╰').Append('─', Math.Max(16, head.Length + pad + 3));
        return sb.ToString();
    }

    /// <summary>构造 <c>##标签## 值</c> 行。</summary>
    public static string Field(string label, string? value) =>
        $"##{label}## {value ?? ""}";
}
