using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthDutyItem", Caption = "权限条目")]
public class AuthDutyItem
{
    [SooColumn("Adi_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Adi_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Adi_Code", Length = 50)]
    public string? Code { get; set; }

    [SooColumn("Adi_Note", Length = 500)]
    public string? Note { get; set; }

    [SooColumn("Adi_Remark", Length = 200)]
    public string? Remark { get; set; }

    [SooColumn("Adi_Idx")]
    public int Idx { get; set; }

    [SooColumn("Adi_VisitorBagId", CanBeNull = true)]
    public Guid? VisitorBagId { get; set; }

    [SooColumn("Adi_WordBagId", CanBeNull = true)]
    public Guid? WordBagId { get; set; }

    [SooColumn("Adi_GoodsBagId", CanBeNull = true)]
    public Guid? GoodsBagId { get; set; }

    [SooColumn("Adi_IsDeleted")]
    public bool IsDeleted { get; set; }

    [SooColumn("Adi_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Adi_CreateTime")]
    public DateTime CreateTime { get; set; }

    [SooColumn("Adi_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Adi_CreateUserId", CanBeNull = true)]
    public Guid? CreateUserId { get; set; }

    [SooColumn("Adi_UpdateUserId", CanBeNull = true)]
    public Guid? UpdateUserId { get; set; }
}
