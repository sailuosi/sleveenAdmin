namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>§H：按 FormCode 路由到唯一业务 Handler。</summary>
public sealed class SailFormBizRegistry
{
    readonly Dictionary<string, ISailFormBizHandler> _map;

    public SailFormBizRegistry(IEnumerable<ISailFormBizHandler> handlers)
    {
        _map = new Dictionary<string, ISailFormBizHandler>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in handlers)
        {
            if (h == null || string.IsNullOrWhiteSpace(h.FormCode)) continue;
            _map[h.FormCode.Trim()] = h;
        }
    }

    public ISailFormBizHandler? TryGet(string? formCode)
    {
        if (string.IsNullOrWhiteSpace(formCode)) return null;
        return _map.TryGetValue(formCode.Trim(), out var h) ? h : null;
    }

    public ISailFormBizHandler GetRequired(string formCode)
    {
        var h = TryGet(formCode);
        if (h == null)
            throw new InvalidOperationException(
                $"No form biz handler registered for FormCode '{formCode}'.");
        return h;
    }

    public IReadOnlyCollection<string> RegisteredCodes => _map.Keys.ToList();
}
