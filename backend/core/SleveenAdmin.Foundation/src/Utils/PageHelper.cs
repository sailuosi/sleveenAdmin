using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Foundation.Utils;

public static class PageHelper
{
    public static PagedResult<T> Apply<T>(IEnumerable<T> source, PageQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var size = query.PageSize <= 0 ? 10 : query.PageSize;
        var list = source.ToList();
        var total = list.Count;
        var records = list.Skip((page - 1) * size).Take(size).ToList();
        return PagedResult<T>.Create(records, page, size, total);
    }
}
