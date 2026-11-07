using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.DBLayer;

[SooTable("AuthGoodsItem", Caption = "资源项")]
public class AuthGoodsItem
{
    [SooColumn("Agi_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Agi_GoodsBagId")]
    public Guid GoodsBagId { get; set; }

    /// <summary>资源类型：1=菜单。</summary>
    [SooColumn("Agi_Type", Length = 20)]
    public string Type { get; set; } = "1";

    [SooColumn("Agi_Oid", Length = 64)]
    public string Oid { get; set; } = string.Empty;

    [SooColumn("Agi_Name", Length = 200)]
    public string? Name { get; set; }

    [SooColumn("Agi_Code", Length = 200)]
    public string? Code { get; set; }

    [SooColumn("Agi_Idx")]
    public int Idx { get; set; }

    [SooColumn("Agi_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Agi_CreateTime")]
    public DateTime CreateTime { get; set; }
}
