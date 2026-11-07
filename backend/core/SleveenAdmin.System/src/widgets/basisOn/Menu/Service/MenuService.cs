using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.Author.AuthBag;
using SleveenAdmin.System.Bootstrap.Constants;
using SleveenAdmin.System.Menu.DBLayer;
using SleveenAdmin.System.Menu.Dtos;
using SleveenAdmin.System.User.DBLayer;

namespace SleveenAdmin.System.Menu.Service;

public sealed class MenuService : IScoped
{
    private readonly MooRepository<SysUser> _users;
    private readonly MooRepository<SysMenu> _menus;

    public MenuService(
        MooRepository<SysUser> users,
        MooRepository<SysMenu> menus)
    {
        _users = users;
        _menus = menus;
    }

    public Task<LoginMenuTreeResult> GetLoginMenuTreeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = _users.Repo.GetById(userId)
                   ?? throw new InvalidOperationException("用户不存在");

        var isSuperAdmin = string.Equals(user.Account, AuthRoleCodes.AdminAccount, StringComparison.OrdinalIgnoreCase);

        List<Guid> menuIds;
        if (isSuperAdmin)
        {
            menuIds = _menus.Repo.GetList(m => m.Type != MenuTypes.Button && !m.IsHide)
                .Select(m => m.Id)
                .ToList();
        }
        else
        {
            var ctx = new AuthUserContext
            {
                UserId = user.Id,
                Account = user.Account,
                RealName = user.RealName,
                OrgId = user.OrgId,
                TenantId = user.TenantId,
                IsSuperAdmin = false
            };

            var allowed = new List<string>();
            DBCash.useSQL(MooSqlSetup.DefaultPosition).useDutyGoods(ctx, "1", ids => allowed.AddRange(ids));
            menuIds = allowed
                .Select(s => Guid.TryParse(s, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        var menuList = _menus.Repo.GetList(m => menuIds.Contains(m.Id) && m.Type != MenuTypes.Button && !m.IsHide)
            .OrderBy(m => m.OrderNo)
            .ToList();

        var routes = BuildLoginRoutes(menuList, null);

        return Task.FromResult(new LoginMenuTreeResult
        {
            Routes = routes,
            Home = FindHomeRoute(routes)
        });
    }

    public async Task<bool> IsRouteExistAsync(Guid userId, string routeName, CancellationToken cancellationToken = default)
    {
        var tree = await GetLoginMenuTreeAsync(userId, cancellationToken);
        return ContainsRouteName(tree.Routes, routeName);
    }

    private static string FindHomeRoute(IReadOnlyList<MenuRouteDto> routes)
    {
        var home = FindRouteByName(routes, "home");
        return home?.Name ?? routes.FirstOrDefault()?.Name ?? "home";
    }

    private static bool ContainsRouteName(IReadOnlyList<MenuRouteDto> routes, string routeName)
    {
        foreach (var route in routes)
        {
            if (route.Name == routeName)
                return true;
            if (route.Children.Count > 0 && ContainsRouteName(route.Children, routeName))
                return true;
        }

        return false;
    }

    private static MenuRouteDto? FindRouteByName(IReadOnlyList<MenuRouteDto> routes, string routeName)
    {
        foreach (var route in routes)
        {
            if (route.Name == routeName)
                return route;
            if (route.Children.Count > 0)
            {
                var child = FindRouteByName(route.Children, routeName);
                if (child is not null)
                    return child;
            }
        }

        return null;
    }

    private static List<MenuRouteDto> BuildLoginRoutes(List<SysMenu> all, Guid? pid)
    {
        return all.Where(m => m.Pid == pid).Select(m =>
        {
            var children = BuildLoginRoutes(all, m.Id);
            var isDirectory = m.Type == MenuTypes.Directory;
            return new MenuRouteDto
            {
                Id = m.Id.ToString(),
                Type = m.Type,
                Name = m.Name,
                Path = m.Path ?? $"/{m.Name}",
                Component = isDirectory ? "layout.base" : (m.Component ?? string.Empty),
                Meta = new MenuRouteMetaDto
                {
                    Title = m.Title,
                    I18nKey = $"route.{m.Name}",
                    Icon = m.Icon,
                    Order = m.OrderNo,
                    HideInMenu = m.IsHide,
                    KeepAlive = m.IsKeepAlive ?? true,
                    Affix = m.IsAffix ?? false,
                    IsLink = m.OutLink,
                    // 页面形态：1 Vue / 2 内嵌 / 3 外链（不是目录/菜单/按钮）
                    MenuType = m.MenuType
                },
                Children = children
            };
        }).ToList();
    }
}
