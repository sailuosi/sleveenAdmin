using System.Data;

namespace SleveenAdmin.Business.Portal.Service;

/// <summary>把库列名改成前端表单/列表用的别名（spcId、saId、SYS_Created…）。</summary>
internal static class PortalRowAlias
{
    public static DataTable? Alias(DataTable? dt, params (string from, string to)[] map)
    {
        if (dt is null) return dt;
        foreach (var (from, to) in map)
        {
            if (dt.Columns.Contains(from) && !dt.Columns.Contains(to))
                dt.Columns[from]!.ColumnName = to;
        }
        return dt;
    }

    public static DataRow? Alias(DataRow? row, params (string from, string to)[] map)
    {
        if (row is null) return null;
        Alias(row.Table, map);
        return row;
    }

    public static readonly (string, string)[] Portal =
    [
        ("Sp_Id", "spId"),
        ("Sp_OrgId", "spOrgId"),
        ("Sp_UserId", "spUserId"),
        ("Sp_PortPageId", "spPortPageId"),
        ("Sp_SubjectId", "spSubjectId"),
        ("Sp_CreateTime", "SYS_Created"),
        ("Sp_UpdateTime", "SYS_LAST_UPD"),
        ("Sp_Deleted", "SYS_Deleted"),
    ];

    public static readonly (string, string)[] Cell =
    [
        ("Spc_Id", "spcId"),
        ("Spc_PortalId", "spPortalId"),
        ("Spc_CreateTime", "SYS_Created"),
        ("Spc_UpdateTime", "SYS_LAST_UPD"),
        ("Spc_Deleted", "SYS_Deleted"),
    ];

    /// <summary>在字典上追加别名（保留原列，供 Biz 输出）。</summary>
    public static void ApplyToMap(Dictionary<string, object?> map, params (string from, string to)[] pairs)
    {
        foreach (var (from, to) in pairs)
        {
            if (map.TryGetValue(from, out var v) && !map.ContainsKey(to))
                map[to] = v;
        }
    }

    public static readonly (string, string)[] CellLink =
    [
        ("Spcl_Id", "spclId"),
        ("Spcl_PortalId", "spPortalId"),
        ("Spcl_PortCellId", "spPortCellId"),
        ("Spcl_AppletId", "saAppletId"),
        ("Spcl_CreateTime", "SYS_Created"),
        ("Spcl_UpdateTime", "SYS_LAST_UPD"),
        ("Spcl_Deleted", "SYS_Deleted"),
    ];

    public static readonly (string, string)[] Applet =
    [
        ("Sa_Id", "saId"),
        ("Sa_AppletCataId", "saAppletCataId"),
        ("Sa_CreateTime", "SYS_Created"),
        ("Sa_UpdateTime", "SYS_LAST_UPD"),
        ("Sa_Deleted", "SYS_Deleted"),
    ];

    public static readonly (string, string)[] PortPage =
    [
        ("Spp_Id", "sppId"),
        ("Spp_OrgId", "spOrgId"),
        ("Spp_CreateTime", "SYS_Created"),
        ("Spp_UpdateTime", "SYS_LAST_UPD"),
        ("Spp_Deleted", "SYS_Deleted"),
    ];

    public static readonly (string, string)[] AppletCata =
    [
        ("Sac_Id", "sacId"),
        ("Sac_Pid", "sacPid"),
        ("Sac_CreateTime", "SYS_Created"),
        ("Sac_UpdateTime", "SYS_LAST_UPD"),
        ("Sac_Deleted", "SYS_Deleted"),
    ];
}
