namespace SleveenAdmin.System.Menu.Dtos;

public sealed class MenuRouteMetaDto
{
    public string Title { get; set; } = string.Empty;

    public string? I18nKey { get; set; }

    public string? Icon { get; set; }

    public int Order { get; set; }

    public bool HideInMenu { get; set; }

    public bool KeepAlive { get; set; }

    public bool Affix { get; set; }

    /// <summary>外链/内嵌地址（对标 api10 OutLink）</summary>
    public string? IsLink { get; set; }

    /// <summary>1 Vue / 2 内嵌 / 3 网页链接</summary>
    public string? MenuType { get; set; }
}
