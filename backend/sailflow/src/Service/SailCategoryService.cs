using System.Security.Claims;
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

    public List<SailProcessCategoryEntity> List(ClaimsPrincipal? user = null)
    {
        EnsureTable();
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailProcessCategoryEntity>(out var c);
        clip.orderBy(() => c.SortOrder);
        var all = clip.select(c).queryList().ToList();
        if (user == null) return all;
        return all.Where(x => _access.CanAccessResource(user, x.AuthType, x.AuthId, x.AuthCode, write: false)).ToList();
    }

    public SailProcessCategoryEntity Save(SailProcessCategoryEntity row, ClaimsPrincipal? user = null)
    {
        EnsureTable();
        if (string.IsNullOrWhiteSpace(row.Code))
            throw new ArgumentException("Code required");
        if (string.IsNullOrWhiteSpace(row.Name))
            throw new ArgumentException("Name required");

        if (user != null)
            _access.RequireAccess(user, row.AuthType, row.AuthId, row.AuthCode, write: true);

        if (SailAuthScope.EqualsType(row.AuthType, SailAuthScope.Unit)
            && string.IsNullOrWhiteSpace(row.AuthId))
            throw new ArgumentException("UNIT 分类须填写 AuthId");

        var repo = _sail.UseRunSQL().useRepo<SailProcessCategoryEntity>();
        var existing = List().FirstOrDefault(x =>
            string.Equals(x.Code, row.Code, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(row.Id) && x.Id == row.Id));

        if (existing == null)
        {
            row.Id = string.IsNullOrWhiteSpace(row.Id) ? Guid.NewGuid().ToString("N") : row.Id;
            row.CreatedAt ??= DateTime.Now;
            repo.Insert(row);
            return row;
        }

        if (user != null)
            _access.RequireAccess(user, existing.AuthType, existing.AuthId, existing.AuthCode, write: true);

        existing.Name = row.Name;
        existing.SortOrder = row.SortOrder;
        existing.Remark = row.Remark ?? "";
        existing.Code = row.Code;
        existing.AuthType = row.AuthType;
        existing.AuthCode = row.AuthCode;
        existing.AuthId = row.AuthId;
        existing.AuthLevel = row.AuthLevel;
        repo.Update(existing);
        return existing;
    }

    public int Delete(string idOrCode)
    {
        EnsureTable();
        var list = List();
        var hit = list.FirstOrDefault(x =>
            x.Id == idOrCode || string.Equals(x.Code, idOrCode, StringComparison.OrdinalIgnoreCase));
        if (hit == null) return 0;
        _sail.UseRunSQL().useRepo<SailProcessCategoryEntity>().Delete(hit);
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
        entity.AppType = categoryCode ?? "";
        _wf.UpdateProcess(entity);
    }

    public Dictionary<string, int> CountProcessesByCategory()
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<ProcessEntity>(out var p);
        var all = clip.select(p).queryList().ToList();
        return all
            .GroupBy(x => string.IsNullOrWhiteSpace(x.AppType) ? "" : x.AppType!)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
