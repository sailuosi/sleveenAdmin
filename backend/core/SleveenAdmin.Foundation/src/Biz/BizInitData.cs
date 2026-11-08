namespace SleveenAdmin.Foundation.Biz;

public sealed class BizDictItem
{
    public string? Value { get; set; }

    public string? Label { get; set; }
}

public sealed class BizInitData
{
    public string BizName { get; set; } = string.Empty;

    public Dictionary<string, List<BizDictItem>> Dicts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>与 gene ListVC / EditVC 约定：page.codeTable。</summary>
    public Dictionary<string, List<BizDictItem>> CodeTable { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class BizAffectedResult
{
    public int Affected { get; set; }

    public static BizAffectedResult Of(int affected) => new() { Affected = affected };
}
