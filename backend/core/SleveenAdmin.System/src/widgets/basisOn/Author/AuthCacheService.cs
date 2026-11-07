using Microsoft.Extensions.Caching.Memory;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.System.Author;

public sealed class AuthCacheService : ISingleton
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public AuthCacheService(IMemoryCache cache) => _cache = cache;

    public T? Get<T>(string key) => _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null)
    {
        lock (_lock) _keys.Add(key);
        _cache.Set(key, value, absoluteExpiration ?? TimeSpan.FromMinutes(20));
    }

    public void Remove(string key)
    {
        lock (_lock) _keys.Remove(key);
        _cache.Remove(key);
    }

    public int RemoveByPrefix(string prefix)
    {
        List<string> toRemove;
        lock (_lock)
            toRemove = _keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();

        foreach (var key in toRemove)
            Remove(key);

        return toRemove.Count;
    }

    public int RemoveKeyLike(string fragment)
    {
        List<string> toRemove;
        lock (_lock)
            toRemove = _keys.Where(k => k.Contains(fragment, StringComparison.Ordinal)).ToList();

        foreach (var key in toRemove)
            Remove(key);

        return toRemove.Count;
    }
}
