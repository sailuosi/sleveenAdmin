using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthVisitorItem", Caption = "访客项")]
public class AuthVisitorItem
{
    [SooColumn("Avi_Id", IsPrimaryKey = true)]
    public Guid? Id { get; set; }

    [SooColumn("Avi_VisitorBagId")]
    public Guid? VisitorBagId { get; set; }

    /// <summary>1=账号 2=组织OID 3=组织编码前缀</summary>
    [SooColumn("Avi_Type", Length = 20)]
    public string? Type { get; set; }

    [SooColumn("Avi_Name", Length = 500)]
    public string? Name { get; set; }

    [SooColumn("Avi_Code", Length = 200)]
    public string? Code { get; set; }

    [SooColumn("Avi_Oid", Length = 64)]
    public string? Oid { get; set; }

    [SooColumn("Avi_IsOn")]
    public bool IsOn { get; set; } = true;

    [SooColumn("Avi_Idx")]
    public int Idx { get; set; }

    [SooColumn("Avi_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }
}
