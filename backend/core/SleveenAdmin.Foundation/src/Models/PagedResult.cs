namespace SleveenAdmin.Foundation.Models;

public class PagedQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public List<T> Records { get; set; } = [];

    public int Current { get; set; }

    public int Size { get; set; }

    public int Total { get; set; }

    public static PagedResult<T> Create(IReadOnlyList<T> records, int current, int size, int total)
        => new()
        {
            Records = records.ToList(),
            Current = current,
            Size = size,
            Total = total
        };
}
