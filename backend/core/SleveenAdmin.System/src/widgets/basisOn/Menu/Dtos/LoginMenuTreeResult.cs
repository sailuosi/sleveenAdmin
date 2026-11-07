namespace SleveenAdmin.System.Menu.Dtos;

public sealed class LoginMenuTreeResult
{
    public IReadOnlyList<MenuRouteDto> Routes { get; set; } = [];

    public string Home { get; set; } = "home";
}
