using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.System.Monitor.Service;

namespace SleveenAdmin.System.Monitor.Controller;

[Authorize]
[ApiController]
[Route("api/sysServer")]
public class MonitorController : ControllerBase
{
    private readonly MonitorService _monitorService;

    public MonitorController(MonitorService monitorService) => _monitorService = monitorService;

    [HttpGet("info")]
    [ApiResult]
    public IActionResult Info() => Ok(_monitorService.GetServerInfo());
}
