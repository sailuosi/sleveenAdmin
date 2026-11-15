using System.Data;
using mooSQL.data;
using mooSQL.utils;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.Author;
using SleveenAdmin.System.Author.AuthBag;
using SleveenAdmin.System.SysPost.DBLayer;

namespace SleveenAdmin.System.SysPost.Service;

/// <summary>人员岗位权限分配：组织卡片 + 岗位类别挂接 + 二次分配校验。</summary>
[Scoped]
public sealed class SysPostAuthService
{
    DBInstance Db => DBCash.GetDBInstance(MooSqlSetup.DefaultPosition);

    public object LoadUserAuth(Guid userId)
    {
        var posts = Db.useSQL()
            .select("mtb.*")
            .from("Sys_Post as mtb")
            .whereGuid("mtb.P_UserId", userId)
            .whereIsOrNull("mtb.P_Deleted", 0)
            .orderBy("mtb.P_OrgSrc asc, mtb.P_IsMain desc, mtb.P_Order asc, mtb.P_Created desc")
            .query();

        var postIds = posts.getFieldValues("P_Id");
        var scopes = postIds.Count == 0
            ? new DataTable()
            : Db.useSQL()
                .select("s.*,t.Pt_Title as Pt_Title,t.Pt_IsOn as Pt_IsOn,t.Pt_Cust as Pt_Cust,t.Pt_OSrc as Pt_OSrc,t.Pt_IncludeChild as Pt_IncludeChild")
                .from("Sys_PostScope s")
                .join("left join Sys_PostType t on s.Ps_PostTypeId=t.Pt_Id")
                .whereIn("s.Ps_PostId", postIds)
                .query();

        return new { posts = ToMaps(posts), scopes = ToMaps(scopes) };
    }

    public object SavePosts(Guid userId, List<BizSysPostRow> posts, string? operatorName = null)
    {
        if (posts == null) throw new ArgumentNullException(nameof(posts));
        var repo = Db.useRepo<SysPostEntity>();
        var now = DateTime.Now;
        foreach (var row in posts)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
            row.UserId = userId;
            row.Deleted ??= false;
            row.IsOn ??= true;
            row.OrgSrc = string.IsNullOrWhiteSpace(row.OrgSrc) ? PostOrgSrc.AdminOrg : row.OrgSrc!.Trim();
            row.GiveMan ??= operatorName;
            row.Updated = now;
            if (!repo.Exist(row.Id))
            {
                row.Created = now;
                repo.Insert(row);
            }
            else
            {
                repo.Update(row);
            }

            EnforceSingleMain(userId, row.OrgSrc!, row.Id, row.IsMain == true);
        }

        ClearUserDutyCache(userId);
        return LoadUserAuth(userId);
    }

    public int RemovePosts(IEnumerable<Guid> postIds)
    {
        var n = 0;
        var postRepo = Db.useRepo<SysPostEntity>();
        foreach (var id in postIds)
        {
            if (id == Guid.Empty) continue;
            var post = postRepo.GetById(id);
            Db.useSQL().setTable("Sys_PostScope").whereGuid("Ps_PostId", id).doDelete();
            Db.useSQL().setTable("Sys_Post").whereGuid("P_Id", id).doDelete();
            n++;
            if (post?.UserId != null) ClearUserDutyCache(post.UserId.Value);
        }
        return n;
    }

    public object SaveScopes(Guid userId, List<BizSysPostScopeRow> scopes, Guid? actorUserId = null)
    {
        if (scopes == null) throw new ArgumentNullException(nameof(scopes));
        ValidateScopeAssign(userId, scopes, actorUserId);
        var repo = Db.useRepo<SysPostScope>();
        foreach (var row in scopes)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
            row.UserId = userId;
            if (row.PostId != null)
            {
                var post = Db.useRepo<SysPostEntity>().GetById(row.PostId.Value);
                if (post != null)
                {
                    row.OrgSrc ??= post.OrgSrc;
                    row.OrgId ??= post.OrgId;
                }
            }
            if (!repo.Exist(row.Id))
                repo.Insert(row);
            else
                repo.Update(row);
        }
        ClearUserDutyCache(userId);
        return LoadUserAuth(userId);
    }

    public int RemoveScopes(IEnumerable<Guid> scopeIds, Guid? userId = null)
    {
        var n = 0;
        var repo = Db.useRepo<SysPostScope>();
        foreach (var id in scopeIds)
        {
            if (id == Guid.Empty) continue;
            Db.useSQL().setTable("Sys_PostScope").whereGuid("Ps_Id", id).doDelete();
            n++;
        }
        if (userId != null) ClearUserDutyCache(userId.Value);
        return n;
    }

    public List<Guid> LoadAssignableChildTypeIds(Guid actorUserId, Guid postId)
    {
        var post = Db.useRepo<SysPostEntity>().GetById(postId)
                   ?? throw new InvalidOperationException("岗位不存在");
        var dt = Db.useSQL()
            .select("c.Pc_PostTypeId as ChildOID, child.Pt_Cust as ChildCust, child.Pt_OSrc as ChildOSrc, child.Pt_IsOn as ChildIsOn")
            .from("Sys_PostTypeChild c")
            .join("INNER JOIN Sys_PostScope s on s.Ps_PostTypeId = c.Pc_BossId")
            .join("INNER JOIN Sys_Post p on s.Ps_PostId = p.P_Id")
            .join("INNER JOIN Sys_PostType child on c.Pc_PostTypeId = child.Pt_Id")
            .whereGuid("p.P_UserId", actorUserId)
            .whereIsOrNull("p.P_Deleted", 0)
            .whereIsOrNull("p.P_IsOn", 1)
            .whereGuid("p.P_OrgId", post.OrgId ?? Guid.Empty)
            .where("p.P_OrgSrc", post.OrgSrc ?? "")
            .query();

        var list = new List<Guid>();
        foreach (DataRow row in dt.Rows)
        {
            if (row["ChildIsOn"] is bool on && !on) continue;
            if (Guid.TryParse($"{row["ChildOID"]}", out var id))
                list.Add(id);
        }
        return list;
    }

    public List<object> LoadDirectory(string orgSrc, Guid orgId)
    {
        var kit = Db.useSQL()
            .select("p.P_Id,p.P_Name,p.P_UserId,p.P_OrgId,p.P_OrgSrc,p.P_OrgName,p.P_IsMain,p.P_IsDuty,u.Su_Account,u.Su_RealName")
            .from("Sys_Post p")
            .join("left join SysUser u on p.P_UserId=u.Su_Id")
            .where("p.P_OrgSrc", orgSrc)
            .whereGuid("p.P_OrgId", orgId)
            .whereIsOrNull("p.P_Deleted", 0)
            .whereIsOrNull("p.P_IsOn", 1);
        var dt = kit.query();
        return ToMaps(dt).Cast<object>().ToList();
    }

    void ValidateScopeAssign(Guid targetUserId, List<BizSysPostScopeRow> scopes, Guid? actorUserId)
    {
        // 超管 / 未传 actor：跳过二次分配限制
        if (actorUserId == null || actorUserId == Guid.Empty) return;

        foreach (var row in scopes)
        {
            if (row.PostTypeId == null || row.PostId == null) continue;
            var type = Db.useRepo<SysPostType>().GetById(row.PostTypeId.Value);
            if (type == null) throw new InvalidOperationException("岗位类别不存在");
            if (type.IsOn == false) throw new InvalidOperationException($"岗位类别「{type.Title}」已停用");

            var post = Db.useRepo<SysPostEntity>().GetById(row.PostId.Value)
                       ?? throw new InvalidOperationException("岗位不存在");

            if (!string.IsNullOrWhiteSpace(type.OSrc) &&
                !string.IsNullOrWhiteSpace(post.OrgSrc) &&
                type.OSrc.IndexOf(post.OrgSrc, StringComparison.Ordinal) < 0)
                throw new InvalidOperationException($"岗位类别「{type.Title}」不适用于当前组织来源");

            // 单卡片二次分配：Pt_Cust 含 single 时禁止再分配给他人（简化对齐 minehub）
            var cust = type.Cust ?? "";
            if (cust.Contains("single", StringComparison.OrdinalIgnoreCase) && targetUserId != actorUserId)
                throw new InvalidOperationException($"{type.Title}为「单卡片」类别，不允许二次分配");

            var allowed = LoadAssignableChildTypeIds(actorUserId.Value, row.PostId.Value);
            // actor 自己已有该类别则允许；否则必须在可分配子类中
            var actorHas = Db.useSQL().from("Sys_PostScope s")
                .join("join Sys_Post p on s.Ps_PostId=p.P_Id")
                .whereGuid("p.P_UserId", actorUserId.Value)
                .whereGuid("s.Ps_PostTypeId", row.PostTypeId.Value)
                .whereGuid("p.P_OrgId", post.OrgId ?? Guid.Empty)
                .where("p.P_OrgSrc", post.OrgSrc ?? "")
                .whereIsOrNull("p.P_Deleted", 0)
                .count() > 0;

            if (!actorHas && !allowed.Contains(row.PostTypeId.Value))
                throw new InvalidOperationException("该岗位类别仅可在已授予的组织/地点下分配");
        }
    }

    void EnforceSingleMain(Guid userId, string orgSrc, Guid keepPostId, bool isMain)
    {
        if (!isMain) return;
        Db.useSQL().setTable("Sys_Post")
            .set("P_IsMain", 0)
            .whereGuid("P_UserId", userId)
            .where("P_OrgSrc", orgSrc)
            .whereFormat("P_Id <> {0}", keepPostId)
            .whereIsOrNull("P_Deleted", 0)
            .doUpdate();
    }

    public static void ClearUserDutyCache(Guid userId)
    {
        var accountRow = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("Su_Account")
            .from("SysUser")
            .whereGuid("Su_Id", userId)
            .queryRow();
        var account = accountRow?.getString("Su_Account", "") ?? "";
        if (string.IsNullOrWhiteSpace(account)) return;
        AuthorServices.Cache.Remove($"userDutys:{account}");
        AuthorServices.Cache.Remove($"userVisitorBags:{account}");
    }

    static List<Dictionary<string, object?>> ToMaps(DataTable dt)
    {
        var list = new List<Dictionary<string, object?>>();
        foreach (DataRow row in dt.Rows)
        {
            var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn col in dt.Columns)
                map[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            list.Add(map);
        }
        return list;
    }
}
