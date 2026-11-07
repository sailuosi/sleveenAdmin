using mooSQL.data;

namespace SleveenAdmin.System.Dict.DBLayer;

[SooTable("SysDictType", Caption = "字典类型")]
public class SysDictType
{
    [SooColumn("Sdt_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sdt_Code", Length = 64, CanBeNull = false)]
    public string Code { get; set; } = string.Empty;

    [SooColumn("Sdt_Name", Length = 64, CanBeNull = false)]
    public string Name { get; set; } = string.Empty;

    [SooColumn("Sdt_Remark", Length = 256, CanBeNull = true)]
    public string? Remark { get; set; }
}
