using mooSQL.data;

namespace SleveenAdmin.System.Tenant.DBLayer;

[SooTable("SysTenant", Caption = "系统租户")]
public class SysTenant
{
    [SooColumn("St_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("St_Name", Length = 64, CanBeNull = false)]
    public string Name { get; set; } = string.Empty;

    [SooColumn("St_Code", Length = 32, CanBeNull = false)]
    public string Code { get; set; } = string.Empty;

    [SooColumn("St_Status", CanBeNull = false)]
    public int Status { get; set; } = 1;

    [SooColumn("St_Remark", Length = 256, CanBeNull = true)]
    public string? Remark { get; set; }

    [SooColumn("St_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }
}
