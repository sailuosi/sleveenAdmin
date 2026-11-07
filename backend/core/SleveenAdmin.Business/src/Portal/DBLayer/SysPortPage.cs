using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortPage, Caption = "门户页面")]
public class SysPortPage
{
    [SooColumn("Spp_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spp_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spp_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spp_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spp_OrgId", CanBeNull = true)]
    public Guid? OrgId { get; set; }

    [SooColumn("PP_Title", Length = 500, CanBeNull = true)]
    public string? Title { get; set; }

    [SooColumn("PP_Theme", Length = 10, CanBeNull = true)]
    public string? Theme { get; set; }

    [SooColumn("PP_Disabled", Length = 10, CanBeNull = true)]
    public string? Disabled { get; set; }

    [SooColumn("PP_Style", CanBeNull = true)]
    public string? Style { get; set; }

    [SooColumn("PP_Css", Length = 500, CanBeNull = true)]
    public string? Css { get; set; }

    [SooColumn("PP_Pose", Length = 10, CanBeNull = true)]
    public string? Pose { get; set; }

    [SooColumn("PP_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [SooColumn("PP_AuthCode", Length = 50, CanBeNull = true)]
    public string? AuthCode { get; set; }

    [SooColumn("PP_Para", CanBeNull = true)]
    public string? Para { get; set; }

    [SooColumn("PP_Note", Length = 500, CanBeNull = true)]
    public string? Note { get; set; }

    [SooColumn("PP_Dept", Length = 200, CanBeNull = true)]
    public string? Dept { get; set; }

    [SooColumn("PP_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [SooColumn("PP_OrgName", Length = 500, CanBeNull = true)]
    public string? OrgName { get; set; }

    [SooColumn("PP_Idx", CanBeNull = true)]
    public int? Idx { get; set; }
}
