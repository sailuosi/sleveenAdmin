using Microsoft.Extensions.Options;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Options;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.Monitor.Dtos;
using StackExchange.Redis;

namespace SleveenAdmin.System.Monitor.Service;

public sealed class RedisService : IScoped
{
    private const int MaxScanKeys = 5000;

    private readonly RedisOptions _options;

    public RedisService(IOptions<RedisOptions> options) => _options = options.Value;

    public RedisInfoDto GetInfo()
    {
        if (!TryConnect(out var mux, out var error))
        {
            return new RedisInfoDto
            {
                Configured = !string.IsNullOrWhiteSpace(_options.ConnectionString),
                Connected = false,
                Error = error
            };
        }

        using (mux)
        {
            var server = mux.GetServer(mux.GetEndPoints().First());
            return new RedisInfoDto
            {
                Configured = true,
                Connected = mux.IsConnected,
                ServerVersion = server.Version.ToString(),
                EndPoint = server.EndPoint.ToString(),
                DbSize = server.DatabaseSize()
            };
        }
    }

    public PagedResult<RedisKeyDto> GetKeysPage(RedisKeyQuery query)
    {
        if (!TryConnect(out var mux, out var error))
            throw new InvalidOperationException(error ?? "Redis 不可用");

        using (mux)
        {
            var db = mux.GetDatabase();
            var pattern = string.IsNullOrWhiteSpace(query.Pattern) ? "*" : query.Pattern.Trim();
            var server = mux.GetServer(mux.GetEndPoints().First());
            var keys = new List<RedisKeyDto>();

            foreach (var key in server.Keys(database: db.Database, pattern: pattern, pageSize: 500))
            {
                var keyText = key.ToString();
                keys.Add(new RedisKeyDto
                {
                    Key = keyText,
                    Type = db.KeyType(key).ToString(),
                    TtlSeconds = GetTtlSeconds(db, key)
                });

                if (keys.Count >= MaxScanKeys)
                    break;
            }

            keys = keys.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase).ToList();
            return PageHelper.Apply(keys, query);
        }
    }

    public RedisKeyValueDto? GetKeyValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key 不能为空");

        if (!TryConnect(out var mux, out var error))
            throw new InvalidOperationException(error ?? "Redis 不可用");

        using (mux)
        {
            var db = mux.GetDatabase();
            var redisKey = new RedisKey(key.Trim());
            if (!db.KeyExists(redisKey))
                return null;

            var type = db.KeyType(redisKey);
            return new RedisKeyValueDto
            {
                Key = key.Trim(),
                Type = type.ToString(),
                Value = ReadValue(db, redisKey, type),
                TtlSeconds = GetTtlSeconds(db, redisKey)
            };
        }
    }

    public void DeleteKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key 不能为空");

        if (!TryConnect(out var mux, out var error))
            throw new InvalidOperationException(error ?? "Redis 不可用");

        using (mux)
        {
            var db = mux.GetDatabase();
            if (!db.KeyDelete(key.Trim()))
                throw new InvalidOperationException("Key 不存在或删除失败");
        }
    }

    private bool TryConnect(out ConnectionMultiplexer mux, out string? error)
    {
        mux = null!;
        error = null;

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            error = "未配置 Redis 连接串";
            return false;
        }

        try
        {
            mux = ConnectionMultiplexer.Connect(_options.ConnectionString);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static long? GetTtlSeconds(IDatabase db, RedisKey key)
    {
        var ttl = db.KeyTimeToLive(key);
        return ttl.HasValue ? (long)ttl.Value.TotalSeconds : null;
    }

    private static string? ReadValue(IDatabase db, RedisKey key, RedisType type) => type switch
    {
        RedisType.String => db.StringGet(key).ToString(),
        RedisType.List => string.Join("\n", db.ListRange(key).Select(x => x.ToString())),
        RedisType.Set => string.Join("\n", db.SetMembers(key).Select(x => x.ToString())),
        RedisType.Hash => string.Join("\n", db.HashGetAll(key).Select(x => $"{x.Name}: {x.Value}")),
        RedisType.SortedSet => string.Join("\n", db.SortedSetRangeByRankWithScores(key).Select(x => $"{x.Element} ({x.Score})")),
        _ => "(unsupported type preview)"
    };
}
