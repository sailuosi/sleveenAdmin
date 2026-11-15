using System.Data;
using System.Net;
using System.Security.Claims;
using mooSQL.data;
using sailflow.engine.Client;
using sailflow.engine.Entity;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>
/// 单位流程归属门禁：读 Auth*（宿主 SQL）+ Sys_PostScope ManagedUnits。
/// </summary>
[Scoped]
public class SailUnitAccess
{
    private readonly SailClient _sail;

    public SailUnitAccess(SailClient sail) => _sail = sail;

    public int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    public bool IsPlatformAdmin(ClaimsPrincipal? user) =>
        AuthPrincipalHelper.BypassesPermissionCheck(user);

    public IReadOnlyCollection<string> GetManagedUnitIds(ClaimsPrincipal? user)
    {
        if (IsPlatformAdmin(user))
            return Array.Empty<string>();

        var userId = CurrentUser.GetUserId(user);
        if (userId == null || userId == Guid.Empty)
            return Array.Empty<string>();

        try
        {
            var kit = DBCash.useSQL(MasterDbPosition);
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var postRows = kit.clear()
                .setTable("Sys_Post")
                .select("P_OrgId")
                .where("P_UserId", userId.Value)
                .query();
            CollectOrgIds(postRows, "P_OrgId", set);

            var scopeRows = kit.clear()
                .setTable("Sys_PostScope")
                .select("Ps_OrgId")
                .where("Ps_UserId", userId.Value)
                .query();
            CollectOrgIds(scopeRows, "Ps_OrgId", set);

            return set;
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public bool CanManageUnit(ClaimsPrincipal? user, string? unitId)
    {
        if (IsPlatformAdmin(user)) return true;
        if (string.IsNullOrWhiteSpace(unitId)) return false;
        return GetManagedUnitIds(user).Contains(unitId.Trim());
    }

    public bool CanAccessResource(ClaimsPrincipal? user, string? authType, string? authId, string? authCode, bool write)
    {
        if (IsPlatformAdmin(user)) return true;
        if (SailAuthScope.IsUnassigned(authType))
            return false;
        if (SailAuthScope.EqualsType(authType, SailAuthScope.Unit))
            return CanManageUnit(user, authId);
        if (write) return false;
        var allowed = SailAuthScope.ParseAllowedUnits(authCode);
        if (allowed.Count == 0) return true;
        var managed = GetManagedUnitIds(user);
        return managed.Any(u => allowed.Contains(u));
    }

    public void RequireAccess(ClaimsPrincipal? user, string? authType, string? authId, string? authCode, bool write)
    {
        if (!CanAccessResource(user, authType, authId, authCode, write))
            throw new SailUnitAccessException(write ? "无权修改该流程资源" : "无权查看该流程资源");
    }

    public SailProcessAuthBag GetProcessAuth(string processId, string version) =>
        SailProcessAuthStore.Read(_sail, processId, version);

    public void RequireProcessAccess(ClaimsPrincipal? user, string processId, string version, bool write)
    {
        var auth = GetProcessAuth(processId, version);
        RequireAccess(user, auth.AuthType, auth.AuthId, auth.AuthCode, write);
    }

    public void RequireProcessAccess(ClaimsPrincipal? user, ProcessEntity process, bool write) =>
        RequireProcessAccess(user, process.ProcessId, process.Version ?? "1", write);

    public IEnumerable<ProcessEntity> FilterProcesses(
        ClaimsPrincipal? user,
        IEnumerable<ProcessEntity> source,
        string? workspace)
    {
        var list = source.Where(p => SailSoftDelete.IsAlive(p.Deleted)).ToList();
        var authMap = SailProcessAuthStore.ReadMany(
            _sail,
            list.Select(p => (p.ProcessId, p.Version ?? "1")));

        SailProcessAuthBag AuthOf(ProcessEntity p) =>
            authMap.TryGetValue(SailProcessAuthStore.Key(p.ProcessId, p.Version ?? "1"), out var a)
                ? a
                : new SailProcessAuthBag();

        if (IsPlatformAdmin(user))
        {
            return workspace?.ToLowerInvariant() switch
            {
                "templates" => list.Where(p =>
                {
                    var a = AuthOf(p);
                    return SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Template)
                           || SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Public)
                           || SailAuthScope.IsUnassigned(a.AuthType);
                }),
                "unit" => list.Where(p =>
                {
                    var a = AuthOf(p);
                    return SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Unit)
                           || SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Public)
                           || SailAuthScope.IsUnassigned(a.AuthType);
                }),
                _ => list
            };
        }

        var managed = GetManagedUnitIds(user);
        return list.Where(p =>
        {
            var a = AuthOf(p);
            if (SailAuthScope.IsUnassigned(a.AuthType))
                return false;
            if (SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Unit))
                return !string.IsNullOrWhiteSpace(a.AuthId) && managed.Contains(a.AuthId);
            if (SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Template)
                || SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Public))
            {
                if (workspace == "unit" && SailAuthScope.EqualsType(a.AuthType, SailAuthScope.Template))
                    return false;
                var allowed = SailAuthScope.ParseAllowedUnits(a.AuthCode);
                return allowed.Count == 0 || managed.Any(u => allowed.Contains(u));
            }
            return false;
        });
    }

    public IReadOnlyList<string> GetOrgAncestorChain(string unitId)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(unitId) || !Guid.TryParse(unitId, out var id))
            return result;

        try
        {
            var kit = DBCash.useSQL(MasterDbPosition);
            var guard = 0;
            while (guard++ < 32)
            {
                result.Add(id.ToString());
                var rows = kit.clear()
                    .setTable("SysOrg")
                    .select("So_Pid")
                    .where("So_Id", id)
                    .query();
                if (rows.Rows.Count == 0) break;
                var pid = rows.Rows[0]["So_Pid"];
                if (pid == null || pid == DBNull.Value) break;
                if (!Guid.TryParse(Convert.ToString(pid), out id) || id == Guid.Empty) break;
            }
        }
        catch
        {
            // ignore
        }

        return result;
    }

    static void CollectOrgIds(DataTable rows, string col, HashSet<string> set)
    {
        if (rows == null) return;
        for (var i = 0; i < rows.Rows.Count; i++)
        {
            var v = rows.Rows[i][col];
            if (v == null || v == DBNull.Value) continue;
            var s = Convert.ToString(v)?.Trim();
            if (!string.IsNullOrWhiteSpace(s) && s != Guid.Empty.ToString())
                set.Add(s);
        }
    }
}

public sealed class SailUnitAccessException : Exception
{
    public HttpStatusCode StatusCode { get; } = HttpStatusCode.Forbidden;

    public SailUnitAccessException(string message) : base(message) { }
}
