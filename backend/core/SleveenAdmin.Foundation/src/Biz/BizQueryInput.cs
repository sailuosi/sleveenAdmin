using System.Text.Json;

namespace SleveenAdmin.Foundation.Biz;

/// <summary>Biz 列表查询入参（兼容 page/pageNum/pageSize 与 conditions）。</summary>
public sealed class BizQueryInput
{
    public int Page { get; set; } = 1;

    public int PageNum { get; set; }

    public int PageSize { get; set; } = 10;

    public string? Keyword { get; set; }

    public string? OrderBy { get; set; }

    public JsonElement? Conditions { get; set; }

    public Dictionary<string, string?>? Filters { get; set; }

    public int ResolvePage()
    {
        if (PageNum > 0) return PageNum;
        return Page <= 0 ? 1 : Page;
    }

    public int ResolvePageSize() => PageSize <= 0 ? 10 : PageSize;
}
