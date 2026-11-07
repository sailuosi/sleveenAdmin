using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthVisitorBag", Caption = "访客集合")]
public class AuthVisitorBag
{
    [SooColumn("Avb_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Avb_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Avb_Code", Length = 200)]
    public string? Code { get; set; }

    [SooColumn("Avb_Note", Length = 500)]
    public string? Note { get; set; }

    [SooColumn("Avb_Remark", Length = 200)]
    public string? Remark { get; set; }

    [SooColumn("Avb_Type", Length = 20)]
    public string? Type { get; set; }

    [SooColumn("Avb_KeyWord", Length = 500)]
    public string? KeyWord { get; set; }

    [SooColumn("Avb_Idx")]
    public int Idx { get; set; }

    [SooColumn("Avb_ParentId", CanBeNull = true)]
    public Guid? ParentId { get; set; }

    [SooColumn("Avb_IsDeleted")]
    public bool IsDeleted { get; set; }

    [SooColumn("Avb_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Avb_CreateTime")]
    public DateTime CreateTime { get; set; }

    [SooColumn("Avb_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }
}
