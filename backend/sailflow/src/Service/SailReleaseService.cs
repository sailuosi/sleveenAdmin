using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using mooSQL.data;
using sailflow.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Service;

public sealed class SailResolvedDefinition
{
    public string ProcessId { get; set; } = "";
    public string Version { get; set; } = "1";
    public string BusinessType { get; set; } = "";
    public string UnitId { get; set; } = "";
    public string Source { get; set; } = SailAuthScope.Public;
    public string ReleaseId { get; set; } = "";
}

/// <summary>发布快照 + 生效路由 + Resolve（发布即生效）。</summary>
[Scoped]
public class SailReleaseService
{
    public const string Paused = "PAUSED";

    private readonly SailClient _sail;
    private readonly IWorkflowService _wf;
    private readonly SailProcessMetaService _meta;
    private readonly SailUnitAccess _access;

    public SailReleaseService(
        SailClient sail,
        IWorkflowService wf,
        SailProcessMetaService meta,
        SailUnitAccess access)
    {
        _sail = sail;
        _wf = wf;
        _meta = meta;
        _access = access;
    }

    void EnsureTables()
    {
        SailHostSchemaInitializer.EnsureHostTables(_sail);
        SailProcessAuthStore.EnsureColumns(_sail);
    }

    public SailPublishedReleaseEntity? OnUsingStateChanged(string processId, string version, int usingState)
    {
        EnsureTables();
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var process = _wf.GetProcessByVersion(processId, ver)
                      ?? throw new InvalidOperationException("process not found");
        var auth = SailProcessAuthStore.Read(_sail, processId, ver);

        if (SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Template)
            || SailAuthScope.IsUnassigned(auth.AuthType))
        {
            if (usingState != 1) DisableReleases(processId, ver);
            return null;
        }

        if (usingState != 1)
        {
            DisableReleases(processId, ver);
            return null;
        }

        var meta = _meta.GetMeta(processId, ver);
        var businessType = (meta?.BusinessType ?? "").Trim();
        if (string.IsNullOrWhiteSpace(businessType))
            businessType = (process.ProcessCode ?? "").Trim();

        var unitId = SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Unit)
            ? (auth.AuthId ?? "").Trim()
            : "";

        var release = new SailPublishedReleaseEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            ProcessId = processId,
            Version = ver,
            BusinessType = businessType,
            UnitId = unitId,
            AuthType = (auth.AuthType ?? "").Trim().ToUpperInvariant(),
            BpmnSha = Sha256(process.XmlContent ?? ""),
            MetaJson = meta == null ? "{}" : JsonSerializer.Serialize(meta),
            FormSnapshot = JsonSerializer.Serialize(new
            {
                meta?.FormType,
                meta?.FormId,
                meta?.FormCustomCreatePath,
                meta?.FormCustomViewPath
            }),
            Enabled = 1,
            CreatedAt = DateTime.Now
        };

        var repo = _sail.UseRunSQL().useRepo<SailPublishedReleaseEntity>();
        DisableReleases(processId, ver);
        repo.Insert(release);

        if ((meta?.Visible ?? 1) == 1)
            PutRoute(businessType, unitId, release.AuthType, processId, ver, skipIfPaused: true);

        return release;
    }

    public void PutRoute(string businessType, string unitId, string authType, string processId, string version, bool skipIfPaused)
    {
        EnsureTables();
        var bt = businessType.Trim();
        var uid = unitId?.Trim() ?? "";
        var existing = FindRoute(bt, uid);
        if (existing != null && skipIfPaused && string.Equals(existing.Source, Paused, StringComparison.OrdinalIgnoreCase))
            return;

        var source = SailAuthScope.EqualsType(authType, SailAuthScope.Unit)
            ? SailAuthScope.Unit
            : SailAuthScope.Public;

        var repo = _sail.UseRunSQL().useRepo<SailEffectiveSourceEntity>();
        if (existing == null)
        {
            repo.Insert(new SailEffectiveSourceEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                UnitId = uid,
                BusinessType = bt,
                Source = source,
                ProcessId = processId,
                Version = version,
                Revision = 1,
                UpdatedAt = DateTime.Now
            });
            return;
        }

        existing.Source = source;
        existing.ProcessId = processId;
        existing.Version = version;
        existing.Revision += 1;
        existing.UpdatedAt = DateTime.Now;
        repo.Update(existing);
    }

    public SailResolvedDefinition Resolve(string businessType, string unitId)
    {
        EnsureTables();
        var bt = (businessType ?? "").Trim();
        if (string.IsNullOrWhiteSpace(bt))
            throw new InvalidOperationException("businessType required");

        var uid = (unitId ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(uid))
        {
            foreach (var ancestor in _access.GetOrgAncestorChain(uid))
            {
                var route = FindRoute(bt, ancestor);
                if (route == null) continue;
                if (string.Equals(route.Source, Paused, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"业务 {bt} 在单位 {ancestor} 已暂停发起");
                if (string.Equals(route.Source, SailAuthScope.Unit, StringComparison.OrdinalIgnoreCase)
                    && TryUseRelease(route.ProcessId, route.Version, out var unitRelease))
                {
                    return new SailResolvedDefinition
                    {
                        ProcessId = route.ProcessId,
                        Version = route.Version,
                        BusinessType = bt,
                        UnitId = ancestor,
                        Source = SailAuthScope.Unit,
                        ReleaseId = unitRelease!.Id
                    };
                }
            }
        }

        var pub = FindRoute(bt, "");
        if (pub != null
            && !string.Equals(pub.Source, Paused, StringComparison.OrdinalIgnoreCase)
            && TryUseRelease(pub.ProcessId, pub.Version, out var pubRelease))
        {
            return new SailResolvedDefinition
            {
                ProcessId = pub.ProcessId,
                Version = pub.Version,
                BusinessType = bt,
                UnitId = "",
                Source = SailAuthScope.Public,
                ReleaseId = pubRelease!.Id
            };
        }

        throw new InvalidOperationException($"未找到业务 {bt} 可用的生效流程（单位 {uid}）");
    }

    public bool TryUseRelease(string processId, string version, out SailPublishedReleaseEntity? release)
    {
        release = null;
        EnsureTables();
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailPublishedReleaseEntity>(out var r);
        clip.where(() => r.ProcessId, processId);
        clip.where(() => r.Version, ver);
        clip.where(() => r.Enabled, 1);
        release = clip.select(r).queryList().OrderByDescending(x => x.CreatedAt).FirstOrDefault();
        if (release == null) return false;

        var process = _wf.GetProcessByVersion(processId, ver);
        if (process == null || process.Status != 1) return false;
        return true;
    }

    SailEffectiveSourceEntity? FindRoute(string businessType, string unitId)
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailEffectiveSourceEntity>(out var e);
        clip.where(() => e.BusinessType, businessType);
        clip.where(() => e.UnitId, unitId ?? "");
        return clip.select(e).queryList().FirstOrDefault();
    }

    void DisableReleases(string processId, string version)
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailPublishedReleaseEntity>(out var r);
        clip.where(() => r.ProcessId, processId);
        clip.where(() => r.Version, version);
        var list = clip.select(r).queryList();
        var repo = _sail.UseRunSQL().useRepo<SailPublishedReleaseEntity>();
        foreach (var item in list.Where(x => x.Enabled == 1))
        {
            item.Enabled = 0;
            repo.Update(item);
        }
    }

    static string Sha256(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text ?? ""));
        return Convert.ToHexString(bytes);
    }
}
