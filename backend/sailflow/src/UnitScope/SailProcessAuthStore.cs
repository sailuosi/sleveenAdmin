using mooSQL.data;
using sailflow.data;
using sailflow.engine.Client;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.UnitScope;

/// <summary>
/// 读写流程 Auth*：主源为 <see cref="ProcessEntity"/>（p_auth_*）；
/// 兼容迁移：若主表为空则回读 <c>sf_process_auth</c> 并回填主表。
/// </summary>
public static class SailProcessAuthStore
{
    public static void EnsureColumns(SailClient sail)
    {
        SailHostSchemaInitializer.EnsureHostTables(sail);
    }

    public static SailProcessAuthBag Read(SailClient sail, string processId, string version)
    {
        EnsureColumns(sail);
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var entity = FindProcess(sail, processId, ver);
        if (entity != null && HasAuth(entity))
            return FromProcess(entity);

        var legacy = ReadLegacy(sail, processId, ver);
        if (entity != null && HasAuth(legacy))
        {
            entity.AuthType = legacy.AuthType;
            entity.AuthCode = legacy.AuthCode;
            entity.AuthId = legacy.AuthId;
            entity.AuthLevel = legacy.AuthLevel;
            sail.UseRunSQL().useRepo<ProcessEntity>().Update(entity);
            return FromProcess(entity);
        }

        if (entity != null)
            return FromProcess(entity);

        return legacy;
    }

    public static Dictionary<string, SailProcessAuthBag> ReadMany(
        SailClient sail,
        IEnumerable<(string ProcessId, string Version)> keys)
    {
        EnsureColumns(sail);
        var map = new Dictionary<string, SailProcessAuthBag>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in keys)
            map[Key(item.ProcessId, item.Version)] = Read(sail, item.ProcessId, item.Version);
        return map;
    }

    public static void Write(SailClient sail, SailProcessAuthBag auth)
    {
        if (auth == null || string.IsNullOrWhiteSpace(auth.ProcessId))
            throw new ArgumentException("ProcessId required");
        EnsureColumns(sail);
        var ver = string.IsNullOrWhiteSpace(auth.Version) ? "1" : auth.Version;
        var entity = FindProcess(sail, auth.ProcessId, ver)
            ?? throw new InvalidOperationException($"process not found: {auth.ProcessId}/{ver}");

        entity.AuthType = auth.AuthType ?? "";
        entity.AuthCode = auth.AuthCode ?? "";
        entity.AuthId = auth.AuthId ?? "";
        entity.AuthLevel = auth.AuthLevel;
        entity.UpdatedDateTime = DateTime.Now;
        sail.UseRunSQL().useRepo<ProcessEntity>().Update(entity);

        // 双写遗留表，便于回滚期对照
        WriteLegacy(sail, auth);
    }

    public static string Key(string processId, string version) =>
        $"{processId}|{(string.IsNullOrWhiteSpace(version) ? "1" : version)}";

    static ProcessEntity? FindProcess(SailClient sail, string processId, string version)
    {
        var clip = sail.UseRunSQL().useClip();
        clip.from<ProcessEntity>(out var p);
        clip.where(() => p.ProcessId, processId);
        clip.where(() => p.Version, version);
        return clip.select(p).queryList().FirstOrDefault();
    }

    static bool HasAuth(ProcessEntity entity) =>
        !string.IsNullOrWhiteSpace(entity.AuthType)
        || !string.IsNullOrWhiteSpace(entity.AuthId)
        || !string.IsNullOrWhiteSpace(entity.AuthCode);

    static bool HasAuth(SailProcessAuthBag bag) =>
        !string.IsNullOrWhiteSpace(bag.AuthType)
        || !string.IsNullOrWhiteSpace(bag.AuthId)
        || !string.IsNullOrWhiteSpace(bag.AuthCode);

    static SailProcessAuthBag FromProcess(ProcessEntity entity) => new()
    {
        ProcessId = entity.ProcessId,
        Version = entity.Version ?? "1",
        AuthType = entity.AuthType ?? "",
        AuthCode = entity.AuthCode ?? "",
        AuthId = entity.AuthId ?? "",
        AuthLevel = entity.AuthLevel
    };

    static SailProcessAuthBag ReadLegacy(SailClient sail, string processId, string ver)
    {
        try
        {
            var clip = sail.UseRunSQL().useClip();
            clip.from<SailProcessAuthEntity>(out var a);
            clip.where(() => a.ProcessId, processId);
            clip.where(() => a.Version, ver);
            var row = clip.select(a).queryList().FirstOrDefault();
            if (row == null)
                return new SailProcessAuthBag { ProcessId = processId, Version = ver };
            return new SailProcessAuthBag
            {
                ProcessId = row.ProcessId,
                Version = row.Version,
                AuthType = row.AuthType ?? "",
                AuthCode = row.AuthCode ?? "",
                AuthId = row.AuthId ?? "",
                AuthLevel = row.AuthLevel
            };
        }
        catch
        {
            return new SailProcessAuthBag { ProcessId = processId, Version = ver };
        }
    }

    static void WriteLegacy(SailClient sail, SailProcessAuthBag auth)
    {
        try
        {
            var ver = string.IsNullOrWhiteSpace(auth.Version) ? "1" : auth.Version;
            var clip = sail.UseRunSQL().useClip();
            clip.from<SailProcessAuthEntity>(out var a);
            clip.where(() => a.ProcessId, auth.ProcessId);
            clip.where(() => a.Version, ver);
            var existing = clip.select(a).queryList().FirstOrDefault();
            var repo = sail.UseRunSQL().useRepo<SailProcessAuthEntity>();
            if (existing == null)
            {
                repo.Insert(new SailProcessAuthEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ProcessId = auth.ProcessId,
                    Version = ver,
                    AuthType = auth.AuthType ?? "",
                    AuthCode = auth.AuthCode ?? "",
                    AuthId = auth.AuthId ?? "",
                    AuthLevel = auth.AuthLevel,
                    UpdatedAt = DateTime.Now
                });
                return;
            }

            existing.AuthType = auth.AuthType ?? "";
            existing.AuthCode = auth.AuthCode ?? "";
            existing.AuthId = auth.AuthId ?? "";
            existing.AuthLevel = auth.AuthLevel;
            existing.UpdatedAt = DateTime.Now;
            repo.Update(existing);
        }
        catch
        {
            /* legacy table optional */
        }
    }
}
