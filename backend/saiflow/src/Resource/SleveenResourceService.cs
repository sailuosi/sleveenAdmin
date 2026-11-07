using System.Data;
using mooSQL.data;
using saiflow.hub.Resource;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Saiflow.Assignee;

namespace SleveenAdmin.Saiflow.Resource;

/// <summary>
/// 宿主具化的 <see cref="IResourceService"/>：SysUser + AuthVisitorBag/Duty，不依赖引擎组织人员表。
/// </summary>
public sealed class SleveenResourceService : IResourceService
{
    readonly IDeptService _dept;
    readonly int _masterDbPosition;

    public SleveenResourceService(IDeptService dept, int masterDbPosition = MooSqlSetup.DefaultPosition)
    {
        _dept = dept ?? throw new ArgumentNullException(nameof(dept));
        _masterDbPosition = masterDbPosition;
    }

    public IList<Role> GetRoleAll()
    {
        var list = new List<Role>();
        try
        {
            var kit = DBCash.useSQL(_masterDbPosition);
            var rows = kit.clear()
                .setTable("AuthVisitorBag")
                .select("Avb_Id,Avb_Code,Avb_Name")
                .where("(Avb_IsDeleted=0 or Avb_IsDeleted is null)")
                .query();
            for (var i = 0; i < rows.Rows.Count; i++)
            {
                var id = Convert.ToString(rows.Rows[i]["Avb_Id"]);
                var code = Convert.ToString(rows.Rows[i]["Avb_Code"]);
                var name = Convert.ToString(rows.Rows[i]["Avb_Name"]);
                if (string.IsNullOrWhiteSpace(id)) continue;
                list.Add(new Role
                {
                    Id = id!,
                    RoleCode = code ?? id!,
                    RoleName = string.IsNullOrWhiteSpace(name) ? (code ?? id!) : name!
                });
            }
        }
        catch
        {
            /* 主库未就绪 */
        }
        return list;
    }

    public IList<User> GetUserAll() => LoadHostUsers();

    public User GetUserById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null!;
        var accounts = SleveenUserIdentity.LoadActiveAccounts();
        if (accounts.TryGetValue(id, out var name))
            return new User { UserId = id, UserName = name };
        return null!;
    }

    public IList<User> GetUserListByRoles(string[] roleIds)
        => ExpandHostRoles(roleIds);

    public IList<User> GetUserListByRole(string roleId)
        => GetUserListByRoles(new[] { roleId });

    public IList<User> GetUserListByRoleCode(string roleCode)
        => GetUserListByRoles(new[] { roleCode });

    public IList<User> GetUserListByRoleReceiverType(string[] roleIds, string curUserId, int receiverType)
    {
        if (receiverType == 0)
            return GetUserListByRoles(roleIds);
        return _dept.GetUserListByDeptRank(roleIds, curUserId, receiverType);
    }

    public IList<Role> FillUsersIntoRoles(string[] roleIds)
    {
        var result = new List<Role>();
        if (roleIds == null || roleIds.Length == 0)
            return result;

        foreach (var roleId in roleIds.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var users = ExpandHostRoles(new[] { roleId });
            result.Add(new Role
            {
                Id = roleId,
                RoleCode = roleId,
                RoleName = roleId,
                UserList = users.ToList()
            });
        }
        return result;
    }

    public Role GetRoleByCode(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            return null!;
        try
        {
            var kit = DBCash.useSQL(_masterDbPosition);
            var byCode = kit.clear()
                .setTable("AuthVisitorBag")
                .select("Avb_Id,Avb_Code,Avb_Name")
                .where("Avb_Code", roleCode)
                .where("(Avb_IsDeleted=0 or Avb_IsDeleted is null)")
                .query();
            if (byCode.Rows.Count > 0)
            {
                var row = byCode.Rows[0];
                return new Role
                {
                    Id = Convert.ToString(row["Avb_Id"]) ?? roleCode,
                    RoleCode = Convert.ToString(row["Avb_Code"]) ?? roleCode,
                    RoleName = Convert.ToString(row["Avb_Name"]) ?? roleCode
                };
            }
        }
        catch
        {
            /* ignore */
        }

        return new Role { Id = roleCode, RoleCode = roleCode, RoleName = roleCode };
    }

    static IList<User> ExpandHostRoles(string[]? roleIds)
    {
        if (roleIds == null || roleIds.Length == 0)
            return new List<User>();
        return new SleveenRoleGroupAssigneeResolver().PreviewUsersByRoleIds(roleIds);
    }

    static IList<User> LoadHostUsers()
    {
        var accounts = SleveenUserIdentity.LoadActiveAccounts();
        return accounts
            .Select(kv => new User { UserId = kv.Key, UserName = kv.Value })
            .ToList();
    }
}
