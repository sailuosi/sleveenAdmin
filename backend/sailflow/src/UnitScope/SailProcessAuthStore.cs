using mooSQL.data;
using sailflow.data;
using sailflow.engine.Client;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.UnitScope;

/// <summary>
/// 读写流程 Auth*（表 sf_process_auth；语义同 PUBLIC/UNIT/TEMPLATE 合同）。
/// NuGet ProcessEntity 未暴露 p_auth_* 时由宿主附表承担。
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

    public static string Key(string processId, string version) =>
        $"{processId}|{(string.IsNullOrWhiteSpace(version) ? "1" : version)}";
}
