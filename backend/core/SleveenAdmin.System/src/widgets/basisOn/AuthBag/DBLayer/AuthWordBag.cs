using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthWordBag", Caption = "词条集合")]
public class AuthWordBag
{
    [SooColumn("Awb_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Awb_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Awb_Code", Length = 200)]
    public string? Code { get; set; }

    [SooColumn("Awb_Note", Length = 500)]
    public string? Note { get; set; }

    [SooColumn("Awb_Remark", Length = 200)]
    public string? Remark { get; set; }

    [SooColumn("Awb_Type", Length = 50)]
    public string? Type { get; set; }

    [SooColumn("Awb_Idx")]
    public int Idx { get; set; }

    [SooColumn("Awb_IsDeleted")]
    public bool IsDeleted { get; set; }

    [SooColumn("Awb_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Awb_CreateTime")]
    public DateTime CreateTime { get; set; }

    [SooColumn("Awb_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }
}
