using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Service;

public sealed class SailOrgNode
{
    public string Id { get; set; } = "";
    public string? ParentId { get; set; }
    public string Name { get; set; } = "";
    public string? OrgNo { get; set; }
    public int? Lv { get; set; }
    public int? Idx { get; set; }
}

/// <summary>
/// 可选组织树：平台全量；单位管理员以管理单位为种子含子级。
/// 种子节点清 ParentId，避免 ManagedUnits 展开后拍成平行根。
/// </summary>
[Scoped]
public class SailOrgTreeService
{
    private readonly SailUnitAccess _access;

    public SailOrgTreeService(SailUnitAccess access) => _access = access;

    public int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    public List<SailOrgNode> SelectableOrgs(System.Security.Claims.ClaimsPrincipal? user)
    {
        var all = LoadActiveOrgs();
        var byId = all.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var children = all
            .Where(x => !string.IsNullOrWhiteSpace(x.ParentId))
            .GroupBy(x => x.ParentId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Idx ?? 0).ThenBy(x => x.Name).ToList(), StringComparer.OrdinalIgnoreCase);

        if (_access.IsPlatformAdmin(user))
            return Order(all);

        var managed = _access.GetManagedUnitIds(user)
            .Where(id => byId.ContainsKey(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (managed.Count == 0) return new List<SailOrgNode>();

        // 只把「父不在 managed 内」的节点当种子，避免下级被拍成根
        var seeds = managed
            .Select(id => byId[id])
            .Where(o => string.IsNullOrWhiteSpace(o.ParentId) || !managed.Contains(o.ParentId!))
            .OrderBy(o => o.Idx ?? 0)
            .ThenBy(o => o.Name)
            .Select(o => o.Id)
            .ToList();

        return CollectSubtree(seeds, byId, children, clearSeedParent: true);
    }

    List<SailOrgNode> LoadActiveOrgs()
    {
        try
        {
            var kit = DBCash.useSQL(MasterDbPosition);
            var rows = kit.clear()
                .setTable("HR_Organize")
                .select("HR_OrganizeOID, HR_Parent_FK, O_OrgName, O_OrgNo, O_Lv, O_Idx, O_Active, SYS_Deleted")
                .whereIsOrNull("SYS_Deleted", 0)
                .query();
            var list = new List<SailOrgNode>();
            for (var i = 0; i < rows.Rows.Count; i++)
            {
                var r = rows.Rows[i];
                var active = r["O_Active"];
                if (active != null && active != DBNull.Value)
                {
                    if (active is bool b && !b) continue;
                    if (active is int n && n == 0) continue;
                    if (string.Equals(Convert.ToString(active), "0", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Convert.ToString(active), "false", StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                var id = Convert.ToString(r["HR_OrganizeOID"])?.Trim();
                if (string.IsNullOrWhiteSpace(id)) continue;
                var parent = r["HR_Parent_FK"] == null || r["HR_Parent_FK"] == DBNull.Value
                    ? null
                    : Convert.ToString(r["HR_Parent_FK"])?.Trim();
                list.Add(new SailOrgNode
                {
                    Id = id,
                    ParentId = string.IsNullOrWhiteSpace(parent) ? null : parent,
                    Name = Convert.ToString(r["O_OrgName"])?.Trim() ?? id,
                    OrgNo = Convert.ToString(r["O_OrgNo"])?.Trim(),
                    Lv = r["O_Lv"] == null || r["O_Lv"] == DBNull.Value ? null : Convert.ToInt32(r["O_Lv"]),
                    Idx = r["O_Idx"] == null || r["O_Idx"] == DBNull.Value ? null : Convert.ToInt32(r["O_Idx"])
                });
            }
            return list;
        }
        catch
        {
            return new List<SailOrgNode>();
        }
    }

    static List<SailOrgNode> CollectSubtree(
        List<string> seeds,
        Dictionary<string, SailOrgNode> byId,
        Dictionary<string, List<SailOrgNode>> children,
        bool clearSeedParent)
    {
        var seedSet = seeds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = new List<SailOrgNode>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(seeds);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!seen.Add(id) || !byId.TryGetValue(id, out var org)) continue;
            result.Add(clearSeedParent && seedSet.Contains(id)
                ? new SailOrgNode
                {
                    Id = org.Id,
                    ParentId = null,
                    Name = org.Name,
                    OrgNo = org.OrgNo,
                    Lv = org.Lv,
                    Idx = org.Idx
                }
                : org);
            if (children.TryGetValue(id, out var kids))
            {
                foreach (var child in kids)
                    queue.Enqueue(child.Id);
            }
        }
        return Order(result);
    }

    static List<SailOrgNode> Order(IEnumerable<SailOrgNode> source) =>
        source.OrderBy(x => x.Lv ?? 0).ThenBy(x => x.Idx ?? 0).ThenBy(x => x.Name).ToList();
}
