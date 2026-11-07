using mooSQL.data;

namespace SleveenAdmin.Saiflow.Entity;

/// <summary>流程分类目录（产品表；流程定义用 AppType 存分类 Code）。</summary>
[SooTable(Name = "sf_process_category", Caption = "流程分类", Type = DBTableType.Table)]
public class SailProcessCategoryEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "code", Length = 64)]
    public string Code { get; set; } = "";

    [SooColumn(Name = "name", Length = 128)]
    public string Name { get; set; } = "";

    [SooColumn(Name = "sort_order")]
    public int SortOrder { get; set; }

    [SooColumn(Name = "remark", Length = 255)]
    public string Remark { get; set; } = "";

    [SooColumn(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }
}
