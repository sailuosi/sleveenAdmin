using mooSQL.data;
using SleveenAdmin.System.SysPost.DBLayer;

namespace SleveenAdmin.System.SysPost.Service;

[SooTable(Name = "Sys_Post", Alias = "mtb", Type = DBTableType.Select, Caption = "岗位")]
public class BizSysPostRow : SysPostEntity
{
}

[SooTable(Name = "Sys_PostType", Alias = "mtb", Type = DBTableType.Select, Caption = "岗位类别")]
public class BizSysPostTypeRow : SysPostType
{
}

[SooTable(Name = "Sys_PostScope", Alias = "mtb", Type = DBTableType.Select, Caption = "岗位授权范围")]
public class BizSysPostScopeRow : SysPostScope
{
    public string? Pt_Title { get; set; }
    public bool? Pt_IsOn { get; set; }
    public string? Pt_Cust { get; set; }
    public string? Pt_OSrc { get; set; }
}

[SooTable(Name = "Sys_PostTypeRole", Alias = "mtb", Type = DBTableType.Select, Caption = "岗位类别角色")]
public class BizSysPostTypeRoleRow : SysPostTypeRole
{
    public string? Adi_Name { get; set; }
    public string? Adi_Code { get; set; }
    public int? Adi_Idx { get; set; }
}

[SooTable(Name = "Sys_PostTypeChild", Alias = "mtb", Type = DBTableType.Select, Caption = "下级岗位类别")]
public class BizSysPostTypeChildRow : SysPostTypeChild
{
    public bool? Pt_IsOn { get; set; }
    public string? Pt_Title { get; set; }
    public string? Pt_Note { get; set; }
    public int? Pt_Order { get; set; }
}
