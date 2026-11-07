namespace SleveenAdmin.System.Menu.Dtos;

public sealed class SysMenuDto
{
    public Guid Id { get; set; }

    public Guid? Pid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Path { get; set; }

    public string? Component { get; set; }

    public string? Icon { get; set; }

    public int Type { get; set; }

    public int OrderNo { get; set; }

    public string? Permission { get; set; }

    public bool IsHide { get; set; }

    public DateTime CreateTime { get; set; }

    public string? MenuCode { get; set; }

    public string? MenuType { get; set; }

    public string? Redirect { get; set; }

    public string? OutLink { get; set; }

    public bool IsIframe { get; set; }

    public bool IsKeepAlive { get; set; }

    public bool IsAffix { get; set; }

    public int Status { get; set; }

    public string? Remark { get; set; }

    public string? ExtPara { get; set; }

    public string? MenuTheme { get; set; }

    public bool IsApp { get; set; }

    public List<SysMenuDto> Children { get; set; } = [];
}
