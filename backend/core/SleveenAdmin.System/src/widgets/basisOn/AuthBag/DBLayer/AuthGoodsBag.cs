using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthGoodsBag", Caption = "资源集合")]
public class AuthGoodsBag
{
    [SooColumn("Agb_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Agb_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Agb_Code", Length = 200)]
    public string? Code { get; set; }

    [SooColumn("Agb_Note", Length = 500)]
    public string? Note { get; set; }

    [SooColumn("Agb_Remark", Length = 200)]
    public string? Remark { get; set; }

    [SooColumn("Agb_Type", Length = 20)]
    public string? Type { get; set; }

    [SooColumn("Agb_Idx")]
    public int Idx { get; set; }

    [SooColumn("Agb_IsDeleted")]
    public bool IsDeleted { get; set; }

    [SooColumn("Agb_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Agb_CreateTime")]
    public DateTime CreateTime { get; set; }

    [SooColumn("Agb_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }
}
