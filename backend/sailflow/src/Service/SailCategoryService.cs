using mooSQL.data;
using sailflow.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>流程分类目录；绑定落在 Process.AppType。</summary>
[Scoped]
public class SailCategoryService
{
    private readonly SailClient _sail;
    private readonly IWorkflowService _wf;

    public SailCategoryService(SailClient sail, IWorkflowService wf)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _wf = wf ?? throw new ArgumentNullException(nameof(wf));
    }

    public void EnsureTable() => SailHostSchemaInitializer.EnsureHostTables(_sail);

    public List<SailProcessCategoryEntity> List()
    {
        EnsureTable();
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailProcessCategoryEntity>(out var c);
        clip.orderBy(() => c.SortOrder);
        return clip.select(c).queryList().ToList();
    }

    public SailProcessCategoryEntity Save(SailProcessCategoryEntity row)
    {
        EnsureTable();
        if (string.IsNullOrWhiteSpace(row.Code))
            throw new ArgumentException("Code required");
        if (string.IsNullOrWhiteSpace(row.Name))
            throw new ArgumentException("Name required");

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

        existing.Name = row.Name;
        existing.SortOrder = row.SortOrder;
        existing.Remark = row.Remark ?? "";
        existing.Code = row.Code;
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
