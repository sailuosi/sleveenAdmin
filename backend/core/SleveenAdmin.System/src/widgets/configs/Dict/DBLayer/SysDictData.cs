using mooSQL.data;

namespace SleveenAdmin.System.Dict.DBLayer;

[SooTable("SysDictData", Caption = "字典数据")]
public class SysDictData
{
    [SooColumn("Sdd_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sdd_TypeId", CanBeNull = false)]
    public Guid TypeId { get; set; }

    [SooColumn("Sdd_Label", Length = 64, CanBeNull = false)]
    public string Label { get; set; } = string.Empty;

    [SooColumn("Sdd_Value", Length = 64, CanBeNull = false)]
    public string Value { get; set; } = string.Empty;

    [SooColumn("Sdd_OrderNo", CanBeNull = false)]
    public int OrderNo { get; set; }
}
