using mooSQL.data;
using sailflow.data;
using sailflow.engine.Client;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Saiflow.Entity;

namespace SleveenAdmin.Saiflow.Service;

/// <summary>请假样例 CRUD + 状态回写。</summary>
[Scoped]
public class SailLeaveService
{
    private readonly SailClient _sail;

    public SailLeaveService(SailClient sail) =>
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));

    public void EnsureTable()
    {
        _sail.UseRunSQL().useDBInit().autoMode().createTable<SailLeaveEntity>();
    }

    public SailLeaveEntity Insert(SailLeaveEntity row)
    {
        if (string.IsNullOrWhiteSpace(row.Id))
            row.Id = Guid.NewGuid().ToString("N");
        row.CreatedAt ??= DateTime.Now;
        if (row.Status <= 0) row.Status = SailLeaveStatus.Running;
        _sail.UseRunSQL().useRepo<SailLeaveEntity>().Insert(row);
        return row;
    }

    public void Update(SailLeaveEntity row)
    {
        _sail.UseRunSQL().useRepo<SailLeaveEntity>().Update(row);
    }

    public SailLeaveEntity? Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _sail.UseRunSQL().useRepo<SailLeaveEntity>().GetById(id);
    }

    public List<SailLeaveEntity> ListByUser(string? userId, int limit = 100)
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailLeaveEntity>(out var t);
        clip.whereIf(!string.IsNullOrWhiteSpace(userId), () => t.UserId, userId);
        clip.orderByDesc(() => t.CreatedAt);
        return clip.select(t).queryList().Take(Math.Max(1, limit)).ToList();
    }

    public int ApplyStatusByAppInstance(string appInstanceId, int status)
    {
        if (string.IsNullOrWhiteSpace(appInstanceId)) return 0;
        var row = Get(appInstanceId);
        if (row == null) return 0;
        if (row.Status == status) return 0;
        row.Status = status;
        Update(row);
        return 1;
    }
}
