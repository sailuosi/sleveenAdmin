namespace SleveenAdmin.Business.Portal;

/// <summary>
/// sys_* 门户表与列映射（由原 zh_* / HH_* 双栈合并）。
/// </summary>
public static class PortalSchema
{
    public const string TablePortal = "sys_portal";
    public const string TablePortPage = "sys_portpage";
    public const string TablePortCell = "sys_portcell";
    public const string TablePortCellLink = "sys_portcell_link";
    public const string TableApplet = "sys_applet";
    public const string TableAppletCata = "sys_applet_cata";
    public const string TablePortalRole = "sys_portal_role";
    public const string TablePortalLink = "sys_portal_link";
    public const string TablePageSelect = "sys_page_select";
    public const string TablePageField = "sys_page_field";
    public const string TablePageConnect = "sys_page_connect";

    public const string TableSysOrg = "SysOrg";
    public const string TableSysMenu = "SysMenu";
    public const string TableSysDictType = "SysDictType";
    public const string TableSysDictData = "SysDictData";
}
