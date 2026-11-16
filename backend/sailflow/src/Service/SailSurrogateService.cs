using mooSQL.data;
using sailflow.engine.Client;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>委托代批配置 CRUD；列表强制 GrantorId=当前用户。</summary>
[Scoped]
public class SailSurrogateService
{
    readonly SailClient _sail;

    public SailSurrogateService(SailClient sail) =>
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));

    public void EnsureTable() => SailHostSchemaInitializer.EnsureHostTables(_sail);

    public SailSurrogateEntity? Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _sail.UseRunSQL().useRepo<SailSurrogateEntity>().GetById(id);
    }

    public (List<SailSurrogateEntity> Items, int Total) QueryMine(
        string grantorId,
        int pageIndex,
        int pageSize,
        string? processId = null,
        int? enabled = null)
    {
        if (string.IsNullOrWhiteSpace(grantorId))
            return ([], 0);

        EnsureTable();
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailSurrogateEntity>(out var s)
            .where(() => s.GrantorId, grantorId.Trim());
        if (!string.IsNullOrWhiteSpace(processId))
            clip.where(() => s.ProcessId, processId.Trim());
        if (enabled.HasValue)
            clip.where(() => s.Enabled, enabled.Value);

        clip.orderByDesc(() => s.UpdatedAt);
        var all = clip.select(s).queryList()?.ToList() ?? [];
        var total = all.Count;
        var items = all
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return (items, total);
    }

    public SailSurrogateEntity Save(SailSurrogateEntity row, string grantorId, string? grantorName)
    {
        if (row == null) throw new ArgumentNullException(nameof(row));
        if (string.IsNullOrWhiteSpace(grantorId))
            throw new UnauthorizedAccessException("grantor required");
        if (string.IsNullOrWhiteSpace(row.AgentId))
            throw new ArgumentException("AgentId required");

        grantorId = grantorId.Trim();
        row.AgentId = row.AgentId.Trim();
        if (string.Equals(row.AgentId, grantorId, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("不能委托给自己");

        EnsureTable();
        var now = DateTime.UtcNow;
        row.GrantorId = grantorId;
        row.GrantorName = grantorName ?? row.GrantorName;
        row.ProcessId = string.IsNullOrWhiteSpace(row.ProcessId) ? null : row.ProcessId.Trim();
        if (row.Weight <= 0) row.Weight = 1m;
        row.Enabled = row.Enabled == 1 ? 1 : 0;
        row.UpdatedAt = now;

        var repo = _sail.UseRunSQL().useRepo<SailSurrogateEntity>();
        if (string.IsNullOrWhiteSpace(row.Id))
        {
            row.Id = Guid.NewGuid().ToString("N");
            row.CreatedAt = now;
            repo.Insert(row);
        }
        else
        {
            var existing = repo.GetById(row.Id)
                ?? throw new InvalidOperationException("surrogate not found");
            if (!string.Equals(existing.GrantorId, grantorId, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("not your surrogate");
            row.CreatedAt = existing.CreatedAt ?? now;
            repo.Update(row);
        }

        return row;
    }

    public bool SetEnabled(string id, string grantorId, bool enabled)
    {
        var row = Get(id);
        if (row == null) return false;
        if (!string.Equals(row.GrantorId, grantorId, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("not your surrogate");
        row.Enabled = enabled ? 1 : 0;
        row.UpdatedAt = DateTime.UtcNow;
        _sail.UseRunSQL().useRepo<SailSurrogateEntity>().Update(row);
        return true;
    }

    public bool Delete(string id, string grantorId)
    {
        var row = Get(id);
        if (row == null) return false;
        if (!string.Equals(row.GrantorId, grantorId, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("not your surrogate");
        _sail.UseRunSQL().useRepo<SailSurrogateEntity>().Delete(row);
        return true;
    }
}
