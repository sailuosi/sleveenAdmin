using System.Data;
using System.Security.Claims;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using mooSQL.data;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysPortalService : IScoped
{
    public PagedResult<SysPortalDto> GetPage(PortalPageQuery query)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("a.Sp_Id as spId,a.Sp_OrgId as spOrgId,a.Sp_UserId as spUserId,a.Sp_PortPageId as spPortPageId")
            .select("a.P_IsOn as pIsOn,a.P_Name as pName,a.P_Note as pNote,a.P_Theme as pTheme,a.P_Idx as pIdx")
            .select("a.P_Group as pGroup,a.P_ShowTitle as pShowTitle,a.P_Style as pStyle,a.P_Icon as pIcon")
            .select("a.P_Css as pCss,a.P_Display as pDisplay,a.P_ColNum as pColNum,a.P_Rows as pRows")
            .select("a.P_Editable as pEditable,a.P_Remark as pRemark,a.P_Task as pTask,a.P_Key as pKey")
            .select("a.P_Pulic as pPulic,a.P_Para as pPara,p.PP_Title as ppTitle,o.So_Name as orgName")
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
        var dt = kit.setPage(pageSize, pageNum)
            .orderBy("a.Sp_CreateTime desc")
            .query();
        var total = kit.count();
        var rows = MapPortalRows(dt);
        return PagedResult<SysPortalDto>.Create(rows, pageNum, pageSize, total);
    }

    public SysPortalDto? GetById(Guid id)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var row = kit.select("a.Sp_Id as spId,a.Sp_OrgId as spOrgId,a.Sp_UserId as spUserId,a.Sp_PortPageId as spPortPageId")
            .select("a.P_IsOn as pIsOn,a.P_Name as pName,a.P_Note as pNote,a.P_Theme as pTheme,a.P_Idx as pIdx")
            .select("a.P_Group as pGroup,a.P_ShowTitle as pShowTitle,a.P_Style as pStyle,a.P_Icon as pIcon")
            .select("a.P_Css as pCss,a.P_Display as pDisplay,a.P_ColNum as pColNum,a.P_Rows as pRows")
            .select("a.P_Editable as pEditable,a.P_Remark as pRemark,a.P_Task as pTask,a.P_Key as pKey")
            .select("a.P_Pulic as pPulic,a.P_Para as pPara,a.Sp_CreateTime as sysCreated,a.Sp_UpdateTime as sysLastUpd")
            .select("p.PP_Title as ppTitle,o.So_Name as orgName")
            .from($"{PortalSchema.TablePortal} a")
            .join($"left join {PortalSchema.TablePortPage} p on a.Sp_PortPageId=p.Spp_Id")
            .join($"left join {PortalSchema.TableSysOrg} o on a.Sp_OrgId=o.So_Id")
            .where("a.Sp_Id", id)
            .queryRow();
        return row is null ? null : MapPortalRow(row);
    }

    public void SaveBatch(IReadOnlyList<SysPortal> rows, ClaimsPrincipal user)
    {
        if (rows.Count == 0) return;
        var userId = PortalUserHelper.GetUserId(user);
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            // 数字表单字段可能以 number 进来，ColNum/Rows 已是 string?；Idx 保持 int?
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
        // STJ 对 string 属性收到 number 时可能失败；前端 P_ColNum/P_Rows 常为数字——若已绑定为 string 则无需处理。
        // 空 Key 时用名称兜底，避免列表空白难辨。
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

    private static List<SysPortalDto> MapPortalRows(DataTable dt)
    {
        var list = new List<SysPortalDto>();
        foreach (DataRow row in dt.Rows)
            list.Add(MapPortalRow(row));
        return list;
    }

    private static SysPortalDto MapPortalRow(DataRow row) => new()
    {
        SpId = row.Table.Columns.Contains("spId") ? Guid.Parse(row["spId"].ToString()!) : Guid.Parse(row["Sp_Id"].ToString()!),
        SpOrgId = GetGuid(row, "spOrgId", "Sp_OrgId"),
        SpUserId = GetGuid(row, "spUserId", "Sp_UserId"),
        SpPortPageId = GetGuid(row, "spPortPageId", "Sp_PortPageId"),
        PIsOn = GetBool(row, "pIsOn", "P_IsOn"),
        PName = GetStr(row, "pName", "P_Name"),
        PNote = GetStr(row, "pNote", "P_Note"),
        PTheme = GetStr(row, "pTheme", "P_Theme"),
        PIdx = GetInt(row, "pIdx", "P_Idx"),
        PGroup = GetStr(row, "pGroup", "P_Group"),
        PShowTitle = GetStr(row, "pShowTitle", "P_ShowTitle"),
        PStyle = GetStr(row, "pStyle", "P_Style"),
        PIcon = GetStr(row, "pIcon", "P_Icon"),
        PCss = GetStr(row, "pCss", "P_Css"),
        PDisplay = GetStr(row, "pDisplay", "P_Display"),
        PColNum = GetStr(row, "pColNum", "P_ColNum"),
        PRows = GetStr(row, "pRows", "P_Rows"),
        PEditable = GetBool(row, "pEditable", "P_Editable"),
        PRemark = GetStr(row, "pRemark", "P_Remark"),
        PTask = GetStr(row, "pTask", "P_Task"),
        PKey = GetStr(row, "pKey", "P_Key"),
        PPulic = GetStr(row, "pPulic", "P_Pulic"),
        PPara = GetStr(row, "pPara", "P_Para"),
        PpTitle = GetStr(row, "ppTitle", "PP_Title"),
        OrgName = GetStr(row, "orgName", "So_Name"),
        SysCreated = GetDate(row, "sysCreated", "Sp_CreateTime"),
        SysLastUpd = GetDate(row, "sysLastUpd", "Sp_UpdateTime")
    };

    private static DateTime? GetDate(DataRow row, params string[] cols)
    {
        foreach (var c in cols)
        {
            if (!row.Table.Columns.Contains(c) || row[c] == DBNull.Value) continue;
            return Convert.ToDateTime(row[c]);
        }
        return null;
    }

    private static string? GetStr(DataRow row, params string[] cols)
    {
        foreach (var c in cols)
            if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                return row[c]?.ToString();
        return null;
    }

    private static Guid? GetGuid(DataRow row, params string[] cols)
    {
        foreach (var c in cols)
        {
            if (!row.Table.Columns.Contains(c) || row[c] == DBNull.Value) continue;
            return Guid.TryParse(row[c].ToString(), out var g) ? g : null;
        }
        return null;
    }

    private static bool? GetBool(DataRow row, params string[] cols)
    {
        foreach (var c in cols)
            if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                return Convert.ToBoolean(row[c]);
        return null;
    }

    private static int? GetInt(DataRow row, params string[] cols)
    {
        foreach (var c in cols)
            if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                return Convert.ToInt32(row[c]);
        return null;
    }
}
