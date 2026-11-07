using mooSQL.data;

namespace SleveenAdmin.System.Org.DBLayer;

[SooTable("SysOrg", Caption = "组织机构")]
public class SysOrg
{
    [SooColumn("So_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("So_Pid", CanBeNull = true)]
    public Guid? Pid { get; set; }

    [SooColumn("So_Name", Length = 64, CanBeNull = false)]
    public string Name { get; set; } = string.Empty;

    [SooColumn("So_Code", Length = 64, CanBeNull = true)]
    public string? Code { get; set; }

    /// <summary>组织层级编码，供 AuthBag Vi_Type=3 前缀匹配。</summary>
    [SooColumn("So_ClassCode", Length = 128, CanBeNull = true)]
    public string? ClassCode { get; set; }

    [SooColumn("So_OrderNo", CanBeNull = false)]
    public int OrderNo { get; set; }

    [SooColumn("So_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }
}
