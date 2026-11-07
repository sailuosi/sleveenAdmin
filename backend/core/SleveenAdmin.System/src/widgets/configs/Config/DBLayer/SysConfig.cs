using mooSQL.data;

namespace SleveenAdmin.System.Config.DBLayer;

[SooTable("SysConfig", Caption = "系统配置")]
public class SysConfig
{
    [SooColumn("Sc_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sc_Code", Length = 64, CanBeNull = false)]
    public string Code { get; set; } = string.Empty;

    [SooColumn("Sc_Value", Length = 512, CanBeNull = true)]
    public string? Value { get; set; }

    [SooColumn("Sc_Remark", Length = 256, CanBeNull = true)]
    public string? Remark { get; set; }
}
