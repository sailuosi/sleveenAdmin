using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Menu.Dtos;
using SleveenAdmin.System.Menu.Service;

namespace SleveenAdmin.System.Menu.Controller;

[ApiController]
[Route("api/sysMenu")]
public class MenuController : ControllerBase
{
    private readonly MenuService _menuService;
    private readonly MenuAdminService _adminService;

    public MenuController(MenuService menuService, MenuAdminService adminService)
    {
        _menuService = menuService;
        _adminService = adminService;
    }

    [HttpGet("constant-routes")]
    [ApiResult]
    public IActionResult ConstantRoutes() => Ok(Array.Empty<object>());

    [Authorize]
    [HttpGet("login-menu-tree")]
    [ApiResult]
    public async Task<IActionResult> LoginMenuTree(CancellationToken cancellationToken)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null)
            return Unauthorized(ApiResult.Fail("未登录", ApiResultCodes.Unauthorized));

        var tree = await _menuService.GetLoginMenuTreeAsync(userId.Value, cancellationToken);
        return Ok(new { routes = tree.Routes, home = tree.Home });
    }

    [Authorize]
    [HttpGet("isRouteExist")]
    [ApiResult]
    public async Task<IActionResult> IsRouteExist([FromQuery] string routeName, CancellationToken cancellationToken)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null)
            return Unauthorized(ApiResult.Fail("未登录", ApiResultCodes.Unauthorized));

        var exists = await _menuService.IsRouteExistAsync(userId.Value, routeName, cancellationToken);
        return Ok(exists);
    }

    [Authorize]
    [HttpGet("tree")]
    [ApiResult]
    public IActionResult Tree() => Ok(_adminService.GetTree());

    [Authorize]
    [HttpGet("detail/{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _adminService.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("菜单不存在")) : Ok(dto);
    }

    [Authorize]
    [HttpPost("add")]
    [ApiResult]
    public IActionResult Create([FromBody] SysMenuInput input) => Ok(new { id = _adminService.Create(input) });

    [Authorize]
    [HttpPut("update")]
    [ApiResult]
    public IActionResult Update([FromBody] SysMenuInput input)
    {
        _adminService.Update(input);
        return Ok();
    }

    [Authorize]
    [HttpDelete("delete/{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _adminService.Delete(id);
        return Ok();
    }
}
