using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Menu.Service;

namespace SleveenAdmin.System.Menu.Controller;

[ApiController]
[Route("api/sysMenu")]
public class MenuController : ControllerBase
{
    private readonly MenuService _menuService;

    public MenuController(MenuService menuService) => _menuService = menuService;

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
}
