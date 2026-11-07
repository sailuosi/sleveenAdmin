using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.System.Online.Service;

namespace SleveenAdmin.System.Online.Controller;

[Authorize]
[ApiController]
[Route("api/sysOnlineUser")]
public class OnlineUserController : ControllerBase
{
    private readonly OnlineUserService _service;

    public OnlineUserController(OnlineUserService service) => _service = service;

    [HttpGet("list")]
    [ApiResult]
    public IActionResult List() => Ok(_service.GetList());

    [HttpGet("count")]
    [ApiResult]
    public IActionResult Count() => Ok(new { count = _service.GetCount() });
}
