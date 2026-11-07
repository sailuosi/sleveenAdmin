using System.Data;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using mooSQL.data;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysPortalCommonService : IScoped
{
    private readonly IMemoryCache _cache;

    public SysPortalCommonService(IMemoryCache cache) => _cache = cache;

    public JsonObject? LoadPortalConfig(string key, ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var account = CurrentUser.GetAccount(user) ?? "anon";
        var ver = ConfigVersion($"key:{key}");
        var cacheKey = $"portalconfig:{key}_{account}_v{ver}";
        if (_cache.TryGetValue(cacheKey, out JsonObject? cached) && cached is not null)
            return cached;

        var userId = CurrentUser.GetUserId(user);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var orgClassCode = ResolveUserOrgClassCode(userId);
        const string portalCols =
            "p.Sp_Id as oid,p.P_Name as name,p.P_ColNum as colnum,p.P_Rows as rowheight,p.P_Display as display,p.P_Css as css,p.P_Style as style,p.P_Theme as theme,p.Sp_UserId as uid,P_Para as para,P_Editable as editable,p.P_IsOn,p.P_Note,p.P_Task,p.P_Key,p.P_Pulic,p.P_Idx,p.P_Group,p.P_ShowTitle,p.P_Icon,p.P_Remark";

        DataRow? mrow = null;
        if (userId.HasValue)
        {
            mrow = kit.select(portalCols)
                .from($"{PortalSchema.TablePortal} p")
                .where("P_Key", key)
                .where("P_IsOn", true)
                .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
                .where("Sp_UserId", userId.Value)
                .whereFormat("(P_Pulic is null or P_Pulic='' or P_Pulic='2')")
                .orderBy("P_Idx asc")
                .queryRow();
        }

        mrow ??= kit.clear()
            .select(portalCols)
            .from($"{PortalSchema.TablePortal} p")
            .join($"left join {PortalSchema.TableSysOrg} po on p.Sp_OrgId=po.So_Id")
            .where("P_Key", key)
            .where("P_IsOn", true)
            .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
            .whereFormat("(P_Pulic is null or P_Pulic='' or P_Pulic='1')")
            .whereFormat(BuildOrgScopeWhere(orgClassCode))
            .orderBy("P_Idx asc")
            .queryRow();

        // 宽松兜底：同 key 启用即可（新建门户常未填共享）
        mrow ??= kit.clear()
            .select(portalCols)
            .from($"{PortalSchema.TablePortal} p")
            .where("P_Key", key)
            .where("P_IsOn", true)
            .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
            .orderBy("P_Idx asc")
            .queryRow();

        if (mrow is null) return null;

        var res = BuildPortalConfig(mrow);
        _cache.Set(cacheKey, res, TimeSpan.FromMinutes(10));
        return res;
    }

    /// <summary>按 OID 加载完整配置（对齐 minehub：不二次走 key+共享过滤）。</summary>
    public JsonObject? LoadPortalConfigByOid(Guid oid, ClaimsPrincipal user)
    {
        if (oid == Guid.Empty) return null;
        var account = CurrentUser.GetAccount(user) ?? "anon";
        var ver = ConfigVersion($"oid:{oid}");
        var cacheKey = $"portalconfig_oid:{oid}_{account}_v{ver}";
        if (_cache.TryGetValue(cacheKey, out JsonObject? cached) && cached is not null)
            return cached;

        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var mrow = kit.select(
                "p.Sp_Id as oid,p.P_Name as name,p.P_ColNum as colnum,p.P_Rows as rowheight,p.P_Display as display,p.P_Css as css,p.P_Style as style,p.P_Theme as theme,p.Sp_UserId as uid,P_Para as para,P_Editable as editable,p.P_IsOn,p.P_Note,p.P_Task,p.P_Key,p.P_Pulic,p.P_Idx,p.P_Group,p.P_ShowTitle,p.P_Icon,p.P_Remark")
            .from($"{PortalSchema.TablePortal} p")
            .where("p.Sp_Id", oid)
            .where("p.P_IsOn", true)
            .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
            .queryRow();
        if (mrow is null) return null;

        var res = BuildPortalConfig(mrow);
        _cache.Set(cacheKey, res, TimeSpan.FromMinutes(10));
        return res;
    }

    private JsonObject BuildPortalConfig(DataRow mrow)
    {
        var res = new JsonObject { ["portal"] = RowToJson(mrow) };
        var oid = mrow["oid"].ToString()!;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);

        var cells = kit.select("c.Spc_Id as oid,c.PC_Name as name,c.PC_X as x,c.PC_Y as y,c.PC_W as w,c.PC_H as h,c.PC_I as i,c.PC_Key as ckey,c.PC_Many as mode,c.PC_Theme as theme,c.PC_Style as style,c.PC_Css as css,c.PC_TitleAlign as titleAlign,c.PC_Idx,c.PC_IsOn,c.PC_Margin,c.PC_Padding,c.PC_Remark,c.PC_CellPara")
            .from($"{PortalSchema.TablePortCell} c")
            .where("Spc_PortalId", oid)
            .where("PC_IsOn", true)
            .query();

        var cellArr = new JsonArray();
        foreach (DataRow cell in cells.Rows)
            cellArr.Add(RowToJson(cell));
        res["cells"] = cellArr;

        var cellIds = cells.Rows.Cast<DataRow>().Select(r => r["oid"].ToString()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        kit.clear();
        var linkKit = kit.select("a.Spcl_Id as oid,a.CL_Icon as icon,a.CL_Key as appletKey,a.CL_Link as link,a.CL_Para as para,a.CL_Para2 as para2,a.CL_ParaVal as paraval")
            .select("a.CL_Slot as slot,a.CL_BodyStyle as style,a.CL_Title as title,a.CL_TitleAlign as titleAlign,a.CL_TitleStyle as titleStyle,a.CL_Type as type,a.CL_Tool as tools")
            .select("a.CL_Color as tcolor,a.CL_Theme as theme,a.CL_Disabled as disabled,a.CL_IsOn,a.CL_Idx,a.CL_Remark,a.Spcl_AppletId as appletId")
            .from($"{PortalSchema.TablePortCellLink} a")
            .where("Spcl_PortalId", oid)
            .where("CL_IsOn", true)
            .orderBy("a.CL_Idx asc");
        if (cellIds.Count > 0)
            linkKit.whereIn("a.Spcl_PortCellId", cellIds);
        var links = linkKit.query();

        var appletArr = new JsonArray();
        foreach (DataRow link in links.Rows)
            appletArr.Add(RowToJson(link));
        res["applets"] = appletArr;
        return res;
    }

    /// <summary>
    /// 首页入口选门户（对齐 minehub）：先按 P_Key=编码，再按 PortPage.PP_Code 绑定。
    /// </summary>
    public JsonObject? LoadHomePortal(string pageCode, ClaimsPrincipal user)
    {
        var code = string.IsNullOrWhiteSpace(pageCode) ? "home" : pageCode.Trim();
        var userId = CurrentUser.GetUserId(user);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var orgClassCode = ResolveUserOrgClassCode(userId);

        // 0) 优先按门户 P_Key（首页导航存的就是 key）
        var hit = FindPortalByKey(kit, code, userId, orgClassCode, preferPersonal: true);
        hit ??= FindPortalByKey(kit, code, userId, orgClassCode, preferPersonal: false);
        // 宽松兜底：同 key 且启用即可（开发期未填共享状态也能出首页）
        hit ??= kit.clear()
            .select("p.Sp_Id as oid,p.P_Key as portalKey,p.P_Name as portalName,p.P_Pulic as pPublic")
            .from($"{PortalSchema.TablePortal} p")
            .where("p.P_Key", code)
            .where("p.P_IsOn", true)
            .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
            .orderBy("p.P_Idx asc")
            .queryRow();

        // 1) 按页面编码 PP_Code → 绑定门户
        if (hit is null)
        {
            var page = kit.clear()
                .select("pp.Spp_Id as oid")
                .from($"{PortalSchema.TablePortPage} pp")
                .where("PP_Code", code)
                .where("(pp.Spp_Deleted is null or pp.Spp_Deleted=0)")
                .queryRow();
            if (page is not null)
            {
                var pageOid = page["oid"]?.ToString();
                if (!string.IsNullOrEmpty(pageOid))
                {
                    if (userId.HasValue)
                    {
                        hit = kit.clear()
                            .select("p.Sp_Id as oid,p.P_Key as portalKey,p.P_Name as portalName,p.P_Pulic as pPublic")
                            .from($"{PortalSchema.TablePortal} p")
                            .where("p.Sp_PortPageId", pageOid)
                            .where("p.P_IsOn", true)
                            .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
                            .where("p.Sp_UserId", userId.Value)
                            .orderBy("p.P_Idx asc")
                            .queryRow();
                    }

                    hit ??= kit.clear()
                        .select("p.Sp_Id as oid,p.P_Key as portalKey,p.P_Name as portalName,p.P_Pulic as pPublic")
                        .from($"{PortalSchema.TablePortal} p")
                        .join($"left join {PortalSchema.TableSysOrg} po on p.Sp_OrgId=po.So_Id")
                        .where("p.Sp_PortPageId", pageOid)
                        .where("p.P_IsOn", true)
                        .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
                        .whereFormat("(p.P_Pulic is null or p.P_Pulic='' or p.P_Pulic='1')")
                        .whereFormat(BuildOrgScopeWhere(orgClassCode))
                        .orderBy("p.P_Idx asc")
                        .queryRow();

                    hit ??= kit.clear()
                        .select("p.Sp_Id as oid,p.P_Key as portalKey,p.P_Name as portalName,p.P_Pulic as pPublic")
                        .from($"{PortalSchema.TablePortal} p")
                        .where("p.Sp_PortPageId", pageOid)
                        .where("p.P_IsOn", true)
                        .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)")
                        .orderBy("p.P_Idx asc")
                        .queryRow();
                }
            }
        }

        if (hit is null)
            return null;

        return new JsonObject
        {
            ["oid"] = hit["oid"]?.ToString(),
            ["portalKey"] = hit["portalKey"]?.ToString(),
            ["portalName"] = hit["portalName"]?.ToString(),
            ["pPublic"] = hit["pPublic"]?.ToString()
        };
    }

    private static DataRow? FindPortalByKey(SQLBuilder kit, string code, Guid? userId, string? orgClassCode, bool preferPersonal)
    {
        kit.clear()
            .select("p.Sp_Id as oid,p.P_Key as portalKey,p.P_Name as portalName,p.P_Pulic as pPublic")
            .from($"{PortalSchema.TablePortal} p")
            .join($"left join {PortalSchema.TableSysOrg} po on p.Sp_OrgId=po.So_Id")
            .where("p.P_Key", code)
            .where("p.P_IsOn", true)
            .where("(p.Sp_Deleted is null or p.Sp_Deleted=0)");

        if (preferPersonal)
        {
            if (!userId.HasValue) return null;
            return kit.where("p.Sp_UserId", userId.Value)
                .whereFormat("(p.P_Pulic is null or p.P_Pulic='' or p.P_Pulic='2')")
                .orderBy("p.P_Idx asc")
                .queryRow();
        }

        return kit.whereFormat("(p.P_Pulic is null or p.P_Pulic='' or p.P_Pulic='1')")
            .whereFormat(BuildOrgScopeWhere(orgClassCode))
            .orderBy("p.P_Idx asc")
            .queryRow();
    }

    public void SaveCells(Guid portalId, JsonArray cells, ClaimsPrincipal user)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var now = DateTime.Now;
        foreach (var node in cells)
        {
            if (node is not JsonObject obj) continue;
            var id = obj["oid"]?.GetValue<string>();
            var cellId = string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var gid) ? Guid.CreateVersion7() : gid;
            var exists = kit.clear().from(PortalSchema.TablePortCell).where("Spc_Id", cellId).count() > 0;
            kit.clear().setTable(PortalSchema.TablePortCell)
                .set("Spc_Id", cellId)
                .set("Spc_PortalId", portalId)
                .set("Spc_UpdateTime", now)
                .set("PC_X", obj["x"]?.ToString())
                .set("PC_Y", obj["y"]?.ToString())
                .set("PC_W", obj["w"]?.ToString())
                .set("PC_H", obj["h"]?.ToString())
                .set("PC_I", obj["i"]?.ToString())
                .set("PC_Key", obj["ckey"]?.ToString())
                .set("PC_IsOn", true);
            if (exists)
                kit.where("Spc_Id", cellId).doUpdate();
            else
            {
                kit.set("Spc_CreateTime", now);
                kit.doInsert();
            }
        }
        InvalidatePortalConfig(portalId);
    }

    public void SaveApplets(Guid portalId, JsonArray applets)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.setTable(PortalSchema.TablePortCellLink).where("Spcl_PortalId", portalId).doDelete();
        var now = DateTime.Now;
        foreach (var node in applets)
        {
            if (node is not JsonObject obj) continue;
            kit.clear().setTable(PortalSchema.TablePortCellLink)
                .set("Spcl_Id", Guid.CreateVersion7())
                .set("Spcl_PortalId", portalId)
                .set("Spcl_CreateTime", now)
                .set("Spcl_UpdateTime", now)
                .set("CL_Link", obj["link"]?.ToString())
                .set("CL_Key", obj["appletKey"]?.ToString())
                .set("CL_Title", obj["title"]?.ToString())
                .set("CL_Type", obj["type"]?.ToString())
                .set("CL_Para", obj["para"]?.ToString())
                .set("CL_IsOn", true)
                .doInsert();
        }
        InvalidatePortalConfig(portalId);
    }

    /// <summary>
    /// 使门户配置缓存失效（版本号 bump，对所有账号立即生效）。
    /// </summary>
    public void InvalidatePortalConfig(Guid portalId, string? portalKey = null)
    {
        if (portalId == Guid.Empty && string.IsNullOrWhiteSpace(portalKey))
            return;

        var key = portalKey;
        if (portalId != Guid.Empty)
        {
            BumpConfigVersion($"oid:{portalId}");
            if (string.IsNullOrWhiteSpace(key))
            {
                var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
                key = kit.clear().select("P_Key").from(PortalSchema.TablePortal).where("Sp_Id", portalId).queryRowString("");
            }
        }

        if (!string.IsNullOrWhiteSpace(key))
            BumpConfigVersion($"key:{key.Trim()}");
    }

    static string VerCacheKey(string scope) => $"portalconfig_ver:{scope}";

    long ConfigVersion(string scope) =>
        _cache.TryGetValue(VerCacheKey(scope), out long v) ? v : 0;

    void BumpConfigVersion(string scope) =>
        _cache.Set(VerCacheKey(scope), ConfigVersion(scope) + 1);

    private static string? ResolveUserOrgClassCode(Guid? userId)
    {
        if (!userId.HasValue) return null;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        return kit.clear()
            .select("o.So_ClassCode")
            .from("SysUser u")
            .join("left join SysOrg o on u.Su_OrgId=o.So_Id")
            .where("u.Su_Id", userId.Value)
            .queryRowString("");
    }

    private static string BuildOrgScopeWhere(string? classCode)
    {
        if (string.IsNullOrWhiteSpace(classCode))
            return "(p.Sp_OrgId is null)";
        var escaped = classCode.Replace("'", "''");
        return $"(p.Sp_OrgId is null or po.So_ClassCode is null or po.So_ClassCode='' or po.So_ClassCode like '{escaped}%')";
    }

    private static JsonObject RowToJson(DataRow row)
    {
        var obj = new JsonObject();
        foreach (DataColumn col in row.Table.Columns)
            obj[col.ColumnName] = row[col] == DBNull.Value ? null : JsonValue.Create(row[col]);
        return obj;
    }
}
