namespace SleveenAdmin.System.Menu.Dtos;

public sealed class MenuRouteDto
{
    public string Id { get; set; } = string.Empty;

    /// <summary>菜单结构类型：1目录 2菜单 3按钮（与 Sm_Type 一致；勿与 Meta.MenuType 页面形态混淆）</summary>
    public int Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Component { get; set; } = string.Empty;

    public MenuRouteMetaDto Meta { get; set; } = new();

    public IReadOnlyList<MenuRouteDto> Children { get; set; } = [];
}
