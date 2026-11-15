using System.Security.Claims;
using mooSQL.data;
using sailflow.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Service;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>流程分类目录；绑定落在 Process.AppType；Auth* 与流程同套归属。</summary>
[Scoped]
public class SailCategoryService
{
    private readonly SailClient _sail;
    private readonly IWorkflowService _wf;
    private readonly SailUnitAccess _access;

    public SailCategoryService(SailClient sail, IWorkflowService wf, SailUnitAccess access)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _wf = wf ?? throw new ArgumentNullException(nameof(wf));
        _access = access ?? throw new ArgumentNullException(nameof(access));
    }

    public void EnsureTable() => SailHostSchemaInitializer.EnsureHostTables(_sail);

    public List<SailProcessCategoryEntity> List(ClaimsPrincipal? user = null, string? unitId = null)
    {
        EnsureTable();
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailProcessCategoryEntity>(out var c);
        clip.orderBy(() => c.SortOrder);
        var all = clip.select(c).queryList()
            .Where(x => SailSoftDelete.IsAlive(x.Deleted))
            .ToList();

        if (!string.IsNullOrWhiteSpace(unitId))
        {
            var uid = unitId.Trim();
            all = all.Where(x =>
                SailAuthScope.EqualsType(x.AuthType, SailAuthScope.Public)
                || SailAuthScope.EqualsType(x.AuthType, SailAuthScope.Template)
                || (SailAuthScope.EqualsType(x.AuthType, SailAuthScope.Unit)
                    && string.Equals(x.AuthId, uid, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        if (user == null) return all;
        return all.Where(x => _access.CanAccessResource(user, x.AuthType, x.AuthId, x.AuthCode, write: false)).ToList();
    }

    public int NextSort(string? authType, string? unitId)
    {
        var list = List(user: null);
        IEnumerable<SailProcessCategoryEntity> layer = list;
        if (SailAuthScope.EqualsType(authType, SailAuthScope.Unit))
        {
            layer = list.Where(x =>
                SailAuthScope.EqualsType(x.AuthType, SailAuthScope.Unit)
                && string.Equals(x.AuthId, unitId ?? "", StringComparison.OrdinalIgnoreCase));
        }
        else if (SailAuthScope.EqualsType(authType, SailAuthScope.Public)
                 || SailAuthScope.EqualsType(authType, SailAuthScope.Template))
        {
            var kind = authType!.Trim();
            layer = list.Where(x => SailAuthScope.EqualsType(x.AuthType, kind));
        }

        return layer.Select(x => x.SortOrder).DefaultIfEmpty(0).Max() + 10;
    }

    public SailProcessCategoryEntity Save(SailProcessCategoryEntity row, ClaimsPrincipal? user = null)
    {
        EnsureTable();
        if (string.IsNullOrWhiteSpace(row.Name))
            throw new ArgumentException("Name required");

        NormalizeAuth(row, user);

        if (user != null)
            _access.RequireAccess(user, row.AuthType, row.AuthId, row.AuthCode, write: true);

        if (SailAuthScope.EqualsType(row.AuthType, SailAuthScope.Unit)
            && string.IsNullOrWhiteSpace(row.AuthId))
            throw new ArgumentException("UNIT 分类须填写 AuthId");

        if (string.IsNullOrWhiteSpace(row.Code))
        {
            var orgNo = SailAuthScope.EqualsType(row.AuthType, SailAuthScope.Unit)
                ? SailResourceCodes.ResolveOrgNo(row.AuthId)
                : null;
            row.Code = SailResourceCodes.Build("cat", row.AuthType, orgNo, row.Name);
        }

        var repo = _sail.UseRunSQL().useRepo<SailProcessCategoryEntity>();
        var existing = List(user: null).FirstOrDefault(x =>
            string.Equals(x.Code, row.Code, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(row.Id) && x.Id == row.Id));

        if (existing == null)
        {
            row.Id = string.IsNullOrWhiteSpace(row.Id) ? Guid.NewGuid().ToString("N") : row.Id;
            row.CreatedAt ??= DateTime.Now;
            row.Deleted = 0;
            row.DeleteTime = null;
            if (row.SortOrder <= 0)
                row.SortOrder = NextSort(row.AuthType, row.AuthId);
            FillCreator(row, user);
            repo.Insert(row);
            return row;
        }

        if (user != null)
            _access.RequireAccess(user, existing.AuthType, existing.AuthId, existing.AuthCode, write: true);

        existing.Name = row.Name;
        existing.SortOrder = row.SortOrder > 0 ? row.SortOrder : existing.SortOrder;
        existing.Remark = row.Remark ?? "";
        existing.Code = row.Code;
        existing.AuthType = row.AuthType;
        existing.AuthCode = row.AuthCode;
        existing.AuthId = row.AuthId;
        existing.AuthLevel = row.AuthLevel;
        repo.Update(existing);
        return existing;
    }

    public int Delete(string idOrCode, ClaimsPrincipal? user = null)
    {
        EnsureTable();
        var list = List(user: null);
        var hit = list.FirstOrDefault(x =>
            x.Id == idOrCode || string.Equals(x.Code, idOrCode, StringComparison.OrdinalIgnoreCase));
        if (hit == null) return 0;
        if (user != null)
            _access.RequireAccess(user, hit.AuthType, hit.AuthId, hit.AuthCode, write: true);
        SailSoftDelete.MarkDeleted(v => hit.Deleted = v, t => hit.DeleteTime = t);
        _sail.UseRunSQL().useRepo<SailProcessCategoryEntity>().Update(hit);
        return 1;
    }

    /// <summary>将流程定义 AppType 设为分类 Code。</summary>
    public void BindProcess(string processId, string version, string? categoryCode)
    {
        if (string.IsNullOrWhiteSpace(processId))
            throw new ArgumentException("ProcessId required");
        var entity = _wf.GetProcessByVersion(processId, string.IsNullOrWhiteSpace(version) ? "1" : version);
        if (entity == null)
            throw new InvalidOperationException("process not found");
        if (SailSoftDelete.IsDeleted(entity.Deleted))
            throw new InvalidOperationException("process deleted");
        entity.AppType = categoryCode ?? "";
        _wf.UpdateProcess(entity);
    }

    public Dictionary<string, int> CountProcessesByCategory()
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<ProcessEntity>(out var p);
        var all = clip.select(p).queryList()
            .Where(x => SailSoftDelete.IsAlive(x.Deleted))
            .ToList();
        return all
            .GroupBy(x => string.IsNullOrWhiteSpace(x.AppType) ? "" : x.AppType!)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    void NormalizeAuth(SailProcessCategoryEntity row, ClaimsPrincipal? user)
    {
        if (string.IsNullOrWhiteSpace(row.AuthType))
        {
            if (user != null && !_access.IsPlatformAdmin(user))
                throw new ArgumentException("单位管理员须指定 UNIT 归属");
            return;
        }

        if (!SailAuthScope.IsKnown(row.AuthType) && !SailAuthScope.IsUnassigned(row.AuthType))
            throw new ArgumentException("AuthType 须为 PUBLIC / UNIT / TEMPLATE 或空");

        if (SailAuthScope.EqualsType(row.AuthType, SailAuthScope.Public)
            || SailAuthScope.EqualsType(row.AuthType, SailAuthScope.Template))
        {
            if (user != null && !_access.IsPlatformAdmin(user))
                throw new SailUnitAccessException("仅平台可维护公共/模板分类");
        }
    }

    static void FillCreator(SailProcessCategoryEntity row, ClaimsPrincipal? user)
    {
        if (!string.IsNullOrWhiteSpace(row.Creator)) return;
        row.Creator = CurrentUser.GetAccount(user) ?? CurrentUser.GetUserId(user)?.ToString() ?? "";
        row.CreatorName = user?.Identity?.Name ?? row.Creator;
    }
}
