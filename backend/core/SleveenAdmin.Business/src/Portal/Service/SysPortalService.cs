using System.Data;
using System.Security.Claims;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysPortalService : IScoped
{
    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("a.Sp_Id as spId,a.Sp_OrgId as spOrgId,a.Sp_UserId as spUserId,a.Sp_PortPageId as spPortPageId")
            .select("a.P_IsOn,a.P_Name,a.P_Note,a.P_Theme,a.P_Idx,a.P_Group,a.P_ShowTitle,a.P_Style,a.P_Icon")
            .select("a.P_Css,a.P_Display,a.P_ColNum,a.P_Rows,a.P_Editable,a.P_Remark,a.P_Task,a.P_Key,a.P_Pulic,a.P_Para")
            .select("a.Sp_CreateTime as SYS_Created,a.Sp_UpdateTime as SYS_LAST_UPD")
            .select("p.PP_Title,o.So_Name as OrgName")
            .from($"{PortalSchema.TablePortal} a")
            .join($"left join {PortalSchema.TablePortPage} p on a.Sp_PortPageId=p.Spp_Id")
            .join($"left join {PortalSchema.TableSysOrg} o on a.Sp_OrgId=o.So_Id")
            .where("(a.Sp_Deleted is null or a.Sp_Deleted=0)");

        if (query.SpPortPageId.HasValue)
            kit.where("a.Sp_PortPageId", query.SpPortPageId.Value);
        if (query.SpOrgId.HasValue)
            kit.where("a.Sp_OrgId", query.SpOrgId.Value);
        if (!string.IsNullOrWhiteSpace(query.PTask))
            kit.where("a.P_Task", query.PTask);
        if (query.PIsOn.HasValue)
            kit.where("a.P_IsOn", query.PIsOn.Value);
        if (!string.IsNullOrWhiteSpace(query.PName))
            kit.whereLike("a.P_Name", query.PName);
        if (!string.IsNullOrWhiteSpace(query.PKey))
            kit.whereLike("a.P_Key", query.PKey);

        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        return kit.setPage(pageSize, pageNum).orderBy("a.Sp_CreateTime desc").queryPaged();
    }

    public DataRow? GetById(Guid id) =>
        DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("a.Sp_Id as spId,a.Sp_OrgId as spOrgId,a.Sp_UserId as spUserId,a.Sp_PortPageId as spPortPageId")
            .select("a.P_IsOn,a.P_Name,a.P_Note,a.P_Theme,a.P_Idx,a.P_Group,a.P_ShowTitle,a.P_Style,a.P_Icon")
            .select("a.P_Css,a.P_Display,a.P_ColNum,a.P_Rows,a.P_Editable,a.P_Remark,a.P_Task,a.P_Key,a.P_Pulic,a.P_Para")
            .select("a.Sp_CreateTime as SYS_Created,a.Sp_UpdateTime as SYS_LAST_UPD")
            .select("p.PP_Title,o.So_Name as OrgName")
            .from($"{PortalSchema.TablePortal} a")
            .join($"left join {PortalSchema.TablePortPage} p on a.Sp_PortPageId=p.Spp_Id")
            .join($"left join {PortalSchema.TableSysOrg} o on a.Sp_OrgId=o.So_Id")
            .where("a.Sp_Id", id)
            .queryRow();

    public void SaveBatch(IReadOnlyList<SysPortal> rows, ClaimsPrincipal user)
    {
        if (rows.Count == 0) return;
        var userId = PortalUserHelper.GetUserId(user);
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            NormalizeStringFields(row);

            if (row.Id == Guid.Empty)
                row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            if (row.CreateTime is null)
                row.CreateTime = now;
            if (row.Deleted is null)
                row.Deleted = false;
            if (row.UserId is null && userId.HasValue)
                row.UserId = userId;

            var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
            var exists = kit.from(PortalSchema.TablePortal).where("Sp_Id", row.Id).count() > 0;
            if (exists)
                UpdateEntity(row);
            else
                InsertEntity(row);
        }
    }

    private static void NormalizeStringFields(SysPortal row)
    {
        if (string.IsNullOrWhiteSpace(row.Key) && !string.IsNullOrWhiteSpace(row.Name))
            row.Key = row.Name.Trim();
    }

    public void Delete(Guid id)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.setTable(PortalSchema.TablePortal).where("Sp_Id", id).doDelete();
        kit.clear();
        kit.setTable(PortalSchema.TablePortCell).where("Spc_PortalId", id).doDelete();
        kit.clear();
        kit.setTable(PortalSchema.TablePortCellLink).where("Spcl_PortalId", id).doDelete();
    }

    private static void InsertEntity(SysPortal row)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.setTable(PortalSchema.TablePortal)
            .set("Sp_Id", row.Id)
            .set("Sp_CreateTime", row.CreateTime)
            .set("Sp_UpdateTime", row.UpdateTime)
            .set("Sp_Deleted", row.Deleted)
            .set("Sp_OrgId", row.OrgId)
            .set("Sp_UserId", row.UserId)
            .set("Sp_PortPageId", row.PortPageId)
            .set("Sp_SubjectId", row.SubjectId)
            .set("P_IsOn", row.IsOn)
            .set("P_Name", row.Name)
            .set("P_Note", row.Note)
            .set("P_Theme", row.Theme)
            .set("P_Idx", row.Idx)
            .set("P_Group", row.Group)
            .set("P_ShowTitle", row.ShowTitle)
            .set("P_Style", row.Style)
            .set("P_Icon", row.Icon)
            .set("P_Css", row.Css)
            .set("P_Display", row.Display)
            .set("P_ColNum", row.ColNum)
            .set("P_Rows", row.Rows)
            .set("P_Editable", row.Editable)
            .set("P_Remark", row.Remark)
            .set("P_Task", row.Task)
            .set("P_Key", row.Key)
            .set("P_Pulic", row.Pulic)
            .set("P_Para", row.Para)
            .doInsert();
    }

    private static void UpdateEntity(SysPortal row)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.setTable(PortalSchema.TablePortal)
            .set("Sp_UpdateTime", row.UpdateTime)
            .set("Sp_Deleted", row.Deleted)
            .set("Sp_OrgId", row.OrgId)
            .set("Sp_UserId", row.UserId)
            .set("Sp_PortPageId", row.PortPageId)
            .set("Sp_SubjectId", row.SubjectId)
            .set("P_IsOn", row.IsOn)
            .set("P_Name", row.Name)
            .set("P_Note", row.Note)
            .set("P_Theme", row.Theme)
            .set("P_Idx", row.Idx)
            .set("P_Group", row.Group)
            .set("P_ShowTitle", row.ShowTitle)
            .set("P_Style", row.Style)
            .set("P_Icon", row.Icon)
            .set("P_Css", row.Css)
            .set("P_Display", row.Display)
            .set("P_ColNum", row.ColNum)
            .set("P_Rows", row.Rows)
            .set("P_Editable", row.Editable)
            .set("P_Remark", row.Remark)
            .set("P_Task", row.Task)
            .set("P_Key", row.Key)
            .set("P_Pulic", row.Pulic)
            .set("P_Para", row.Para)
            .where("Sp_Id", row.Id)
            .doUpdate();
    }
}
