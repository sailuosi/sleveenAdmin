using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthWordDefine", Caption = "词条定义")]
public class AuthWordDefine
{
    [SooColumn("Awd_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Awd_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Awd_Code", Length = 50)]
    public string? Code { get; set; }

    [SooColumn("Awd_Type", Length = 20)]
    public string? Type { get; set; }

    [SooColumn("Awd_Para", Length = 500)]
    public string? Para { get; set; }

    [SooColumn("Awd_Parser", Length = 2000)]
    public string? Parser { get; set; }

    [SooColumn("Awd_Note", Length = 500)]
    public string? Note { get; set; }

    [SooColumn("Awd_Idx")]
    public int Idx { get; set; }

    [SooColumn("Awd_IsOn")]
    public bool IsOn { get; set; } = true;

    [SooColumn("Awd_IsDeleted")]
    public bool IsDeleted { get; set; }

    [SooColumn("Awd_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Awd_CreateTime")]
    public DateTime CreateTime { get; set; }

    [SooColumn("Awd_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }
}
