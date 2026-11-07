using mooSQL.data;

namespace SleveenAdmin.System.Menu.DBLayer;

/// <summary>系统菜单（字段对标 api10 SysMenu）。</summary>
[SooTable("SysMenu", Caption = "系统菜单")]
public class SysMenu
{
    [SooColumn("Sm_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sm_Pid", CanBeNull = true)]
    public Guid? Pid { get; set; }

    /// <summary>菜单类型：1目录 2菜单 3按钮</summary>
    [SooColumn("Sm_Type", CanBeNull = false)]
    public int Type { get; set; }

    [SooColumn("Sm_Name", Length = 64, CanBeNull = false)]
    public string Name { get; set; } = string.Empty;

    [SooColumn("Sm_Title", Length = 64, CanBeNull = false)]
    public string Title { get; set; } = string.Empty;

    [SooColumn("Sm_Path", Length = 128, CanBeNull = true)]
    public string? Path { get; set; }

    [SooColumn("Sm_Component", Length = 256, CanBeNull = true)]
    public string? Component { get; set; }

    [SooColumn("Sm_Icon", Length = 128, CanBeNull = true)]
    public string? Icon { get; set; }

    [SooColumn("Sm_OrderNo", CanBeNull = false)]
    public int OrderNo { get; set; }

    [SooColumn("Sm_Permission", Length = 128, CanBeNull = true)]
    public string? Permission { get; set; }

    [SooColumn("Sm_IsHide", CanBeNull = false)]
    public bool IsHide { get; set; }

    [SooColumn("Sm_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }

    // —— 以下对齐 api10 ——

    [SooColumn("Sm_MenuCode", Length = 100, CanBeNull = true)]
    public string? MenuCode { get; set; }

    /// <summary>展示形态：1 Vue页面 / 2 内嵌 / 3 网页链接</summary>
    [SooColumn("Sm_MenuType", Length = 10, CanBeNull = true)]
    public string? MenuType { get; set; }

    [SooColumn("Sm_Redirect", Length = 128, CanBeNull = true)]
    public string? Redirect { get; set; }

    [SooColumn("Sm_OutLink", Length = 1500, CanBeNull = true)]
    public string? OutLink { get; set; }

    // SQLite 对已有表 ADD NOT NULL 列会失败，故新增标量列一律可空；业务层按默认值解读
    [SooColumn("Sm_IsIframe", CanBeNull = true)]
    public bool? IsIframe { get; set; }

    [SooColumn("Sm_IsKeepAlive", CanBeNull = true)]
    public bool? IsKeepAlive { get; set; } = true;

    [SooColumn("Sm_IsAffix", CanBeNull = true)]
    public bool? IsAffix { get; set; }

    /// <summary>1 启用 / 2 停用</summary>
    [SooColumn("Sm_Status", CanBeNull = true)]
    public int? Status { get; set; } = 1;

    [SooColumn("Sm_Remark", Length = 256, CanBeNull = true)]
    public string? Remark { get; set; }

    [SooColumn("Sm_ExtPara", Length = 500, CanBeNull = true)]
    public string? ExtPara { get; set; }

    [SooColumn("Sm_MenuTheme", Length = 200, CanBeNull = true)]
    public string? MenuTheme { get; set; }

    [SooColumn("Sm_IsApp", CanBeNull = true)]
    public bool? IsApp { get; set; }
}
