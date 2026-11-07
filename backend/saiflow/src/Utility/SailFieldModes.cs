using saiflow.hub.Form;

namespace SleveenAdmin.Saiflow.Utility;

/// <summary>§C 办理台字段编辑性：统一 string mode，兼容旧 IsReadOnly/IsNotVisible。</summary>
public sealed class SailFieldModeItem
{
    public string FieldName { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>editable | readonly | hidden | masked</summary>
    public string Mode { get; set; } = "readonly";
    public bool IsReadOnly { get; set; }
    public bool IsNotVisible { get; set; }
}

public static class SailFieldModes
{
    public static SailFieldModeItem FromMode(string field, string? title, string mode)
    {
        var m = SailNodePolicyCatalog.NormalizeFieldMode(mode);
        return new SailFieldModeItem
        {
            FieldName = field,
            Title = title ?? "",
            Mode = m,
            IsReadOnly = m is "readonly" or "masked",
            IsNotVisible = m == "hidden"
        };
    }

    public static SailFieldModeItem FromLegacy(FieldActivityEdit edit)
    {
        var mode = "editable";
        if (edit.IsNotVisible) mode = "hidden";
        else if (edit.IsReadOnly) mode = "readonly";
        return FromMode(edit.FieldName ?? "", "", mode);
    }

    /// <summary>
    /// NodePolicy.fieldsPermission 非空则为其权威；否则用 Form 活动权限表。
    /// </summary>
    public static List<SailFieldModeItem> Resolve(
        IReadOnlyList<SailFieldPermissionItem>? nodePolicyFields,
        IEnumerable<FieldActivityEdit?>? formEdits)
    {
        if (nodePolicyFields is { Count: > 0 })
        {
            return nodePolicyFields
                .Where(x => !string.IsNullOrWhiteSpace(x.Field))
                .Select(x => FromMode(x.Field, x.Title, x.Permission))
                .ToList();
        }

        if (formEdits == null) return new List<SailFieldModeItem>();
        return formEdits
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.FieldName))
            .Select(FromLegacy!)
            .ToList();
    }

    public static Dictionary<string, string> ToMap(IEnumerable<SailFieldModeItem> items)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.FieldName)) continue;
            map[item.FieldName] = item.Mode;
        }
        return map;
    }
}
