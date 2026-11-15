namespace SleveenAdmin.Sailflow.UnitScope;

/// <summary>流程/表单/分类 AuthType 归属约定（宿主消费；引擎不强制）。</summary>
public static class SailAuthScope
{
    public const string Public = "PUBLIC";
    public const string Unit = "UNIT";
    public const string Template = "TEMPLATE";

    /// <summary>单位流程管理员职责编号（岗位类型角色绑定）。</summary>
    public const string UnitAdminDutyCode = "sail_unit_admin";

    public static bool IsKnown(string? authType) =>
        string.Equals(authType, Public, StringComparison.OrdinalIgnoreCase)
        || string.Equals(authType, Unit, StringComparison.OrdinalIgnoreCase)
        || string.Equals(authType, Template, StringComparison.OrdinalIgnoreCase);

    public static bool IsUnassigned(string? authType) => string.IsNullOrWhiteSpace(authType);

    public static bool EqualsType(string? a, string expected) =>
        string.Equals(a?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> ParseAllowedUnits(string? authCode)
    {
        if (string.IsNullOrWhiteSpace(authCode))
            return Array.Empty<string>();
        var raw = authCode.Trim();
        if (raw.StartsWith('['))
        {
            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw);
                return list?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                       ?? (IReadOnlyList<string>)Array.Empty<string>();
            }
            catch
            {
                // fall through to CSV
            }
        }

        return raw.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
