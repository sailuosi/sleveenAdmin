using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthWordItem", Caption = "词条项")]
public class AuthWordItem
{
    [SooColumn("Awi_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Awi_WordBagId")]
    public Guid WordBagId { get; set; }

    [SooColumn("Awi_WordDefineId", CanBeNull = true)]
    public Guid? WordDefineId { get; set; }

    [SooColumn("Awi_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Awi_Code", Length = 50)]
    public string? Code { get; set; }

    [SooColumn("Awi_Type", Length = 20)]
    public string? Type { get; set; }

    [SooColumn("Awi_Idx")]
    public int Idx { get; set; }

    [SooColumn("Awi_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }
}
