using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.AuthBag.DBLayer;
using SleveenAdmin.System.Menu.DBLayer;
using SleveenAdmin.System.Menu.Dtos;

namespace SleveenAdmin.System.Menu.Service;

public sealed class MenuAdminService : IScoped
{
    private readonly MooRepository<SysMenu> _menus;
    private readonly MooRepository<AuthGoodsItem> _goodsItems;

    public MenuAdminService(MooRepository<SysMenu> menus, MooRepository<AuthGoodsItem> goodsItems)
    {
        _menus = menus;
        _goodsItems = goodsItems;
    }

    public List<SysMenuDto> GetTree()
    {
        var all = _menus.Repo.GetList().OrderBy(m => m.OrderNo).ToList();
        return BuildMenuTree(all, null);
    }

    public SysMenuDto? GetById(Guid id)
    {
        var menu = _menus.Repo.GetById(id);
        return menu is null ? null : ToDto(menu);
    }

    public Guid Create(SysMenuInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("菜单名称和标题不能为空");
        if (_menus.Repo.GetFirst(m => m.Name == input.Name) is not null)
            throw new InvalidOperationException("菜单路由名已存在");

        var id = Guid.CreateVersion7();
        _menus.Repo.Insert(ApplyInput(new SysMenu
        {
            Id = id,
            CreateTime = DateTime.Now,
            IsKeepAlive = true,
            Status = 1
        }, input));
        return id;
    }

    public void Update(SysMenuInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var menu = _menus.Repo.GetById(input.Id.Value)
                   ?? throw new InvalidOperationException("菜单不存在");

        if (!string.IsNullOrWhiteSpace(input.Name) && input.Name != menu.Name)
        {
            if (_menus.Repo.GetFirst(m => m.Name == input.Name && m.Id != menu.Id) is not null)
                throw new InvalidOperationException("菜单路由名已存在");
        }

        ApplyInput(menu, input);
        _menus.Repo.Update(menu);
    }

    public void Delete(Guid id)
    {
        var children = _menus.Repo.GetList(m => m.Pid == id);
        if (children.Count > 0)
            throw new InvalidOperationException("请先删除子菜单");

        var oid = id.ToString();
        var links = _goodsItems.Repo.GetList(g => g.Type == "1" && g.Oid == oid);
        foreach (var link in links)
            _goodsItems.Repo.Delete(link);

        if (!_menus.Repo.DeleteById(id))
            throw new InvalidOperationException("菜单不存在");
    }

    private static SysMenu ApplyInput(SysMenu menu, SysMenuInput input)
    {
        menu.Pid = input.Pid;
        menu.Name = input.Name.Trim();
        menu.Title = input.Title.Trim();
        menu.Path = input.Path;
        menu.Component = input.Component;
        menu.Icon = input.Icon;
        menu.Type = input.Type;
        menu.OrderNo = input.OrderNo;
        menu.Permission = input.Permission;
        menu.IsHide = input.IsHide;
        menu.MenuCode = input.MenuCode;
        menu.MenuType = input.MenuType;
        menu.Redirect = input.Redirect;
        menu.OutLink = input.OutLink;
        menu.IsIframe = input.IsIframe;
        menu.IsKeepAlive = input.IsKeepAlive;
        menu.IsAffix = input.IsAffix;
        menu.Status = input.Status <= 0 ? 1 : input.Status;
        menu.Remark = input.Remark;
        menu.ExtPara = input.ExtPara;
        menu.MenuTheme = input.MenuTheme;
        menu.IsApp = input.IsApp;
        return menu;
    }

    private static bool Flag(bool? v, bool fallback = false) => v ?? fallback;

    private static List<SysMenuDto> BuildMenuTree(List<SysMenu> all, Guid? pid)
    {
        return all.Where(m => m.Pid == pid)
            .Select(m =>
            {
                var dto = ToDto(m);
                dto.Children = BuildMenuTree(all, m.Id);
                return dto;
            }).ToList();
    }

    private static SysMenuDto ToDto(SysMenu menu) => new()
    {
        Id = menu.Id,
        Pid = menu.Pid,
        Name = menu.Name,
        Title = menu.Title,
        Path = menu.Path,
        Component = menu.Component,
        Icon = menu.Icon,
        Type = menu.Type,
        OrderNo = menu.OrderNo,
        Permission = menu.Permission,
        IsHide = menu.IsHide,
        CreateTime = menu.CreateTime,
        MenuCode = menu.MenuCode,
        MenuType = menu.MenuType,
        Redirect = menu.Redirect,
        OutLink = menu.OutLink,
        IsIframe = Flag(menu.IsIframe),
        IsKeepAlive = Flag(menu.IsKeepAlive, true),
        IsAffix = Flag(menu.IsAffix),
        Status = menu.Status is null or <= 0 ? 1 : menu.Status.Value,
        Remark = menu.Remark,
        ExtPara = menu.ExtPara,
        MenuTheme = menu.MenuTheme,
        IsApp = Flag(menu.IsApp)
    };
}
