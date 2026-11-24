using mooSQL.data;
using sailflow.hosting;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>我的审批常用语 CRUD（仅本人）。</summary>
[Scoped]
public class SailApprovalPhraseService
{
    readonly SailClient _sail;

    public SailApprovalPhraseService(SailClient sail) =>
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));

    void Ensure() => SailHostSchemaInitializer.EnsureHostTables(_sail);

    public List<SailApprovalPhraseEntity> ListMine(string userId, bool enabledOnly = true)
    {
        Ensure();
        if (string.IsNullOrWhiteSpace(userId))
            return new List<SailApprovalPhraseEntity>();

        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailApprovalPhraseEntity>(out var p)
            .where(() => p.UserId, userId.Trim());
        if (enabledOnly)
            clip.where(() => p.Enabled, 1);
        clip.orderBy(() => p.SortOrder);
        return clip.select(p).queryList()?.ToList() ?? new List<SailApprovalPhraseEntity>();
    }

    public SailApprovalPhraseEntity Save(SailApprovalPhraseEntity row, string userId)
    {
        Ensure();
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("user required");
        if (string.IsNullOrWhiteSpace(row.Content))
            throw new ArgumentException("Content required");

        var uid = userId.Trim();
        var content = row.Content.Trim();
        if (content.Length > 500)
            content = content[..500];

        var repo = _sail.UseRunSQL().useRepo<SailApprovalPhraseEntity>();
        SailApprovalPhraseEntity? existing = null;
        if (!string.IsNullOrWhiteSpace(row.Id))
        {
            existing = repo.GetById(row.Id);
            if (existing != null
                && !string.Equals(existing.UserId, uid, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("not owner");
        }

        if (existing == null)
        {
            var entity = new SailApprovalPhraseEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = uid,
                Content = content,
                SortOrder = row.SortOrder > 0 ? row.SortOrder : NextSort(uid),
                Enabled = row.Enabled == 0 ? 0 : 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            repo.Insert(entity);
            return entity;
        }

        existing.Content = content;
        existing.SortOrder = row.SortOrder > 0 ? row.SortOrder : existing.SortOrder;
        existing.Enabled = row.Enabled == 0 ? 0 : 1;
        existing.UpdatedAt = DateTime.Now;
        repo.Update(existing);
        return existing;
    }

    public bool Delete(string id, string userId)
    {
        Ensure();
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userId))
            return false;
        var repo = _sail.UseRunSQL().useRepo<SailApprovalPhraseEntity>();
        var row = repo.GetById(id);
        if (row == null)
            return false;
        if (!string.Equals(row.UserId, userId.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("not owner");
        repo.Delete(row);
        return true;
    }

    /// <summary>按 ids 顺序重排（仅本人；未列出的保持原序靠后）。</summary>
    public int UpdateSortBatch(IReadOnlyList<string> ids, string userId)
    {
        Ensure();
        if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(userId))
            return 0;
        var uid = userId.Trim();
        var mine = ListMine(uid, enabledOnly: false)
            .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var repo = _sail.UseRunSQL().useRepo<SailApprovalPhraseEntity>();
        var order = 10;
        var n = 0;
        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id) || !mine.TryGetValue(id.Trim(), out var row))
                continue;
            row.SortOrder = order;
            row.UpdatedAt = DateTime.Now;
            repo.Update(row);
            order += 10;
            n++;
        }
        return n;
    }

    int NextSort(string userId)
    {
        var list = ListMine(userId, enabledOnly: false);
        return list.Select(x => x.SortOrder).DefaultIfEmpty(0).Max() + 10;
    }
}
