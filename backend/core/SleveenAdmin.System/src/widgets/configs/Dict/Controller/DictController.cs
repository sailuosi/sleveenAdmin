using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.System.Dict.Service;

namespace SleveenAdmin.System.Dict.Controller;

[Authorize]
[ApiController]
[Route("api/sysDict")]
public class DictController : ControllerBase
{
    private readonly DictService _service;

    public DictController(DictService service) => _service = service;

    [HttpGet("data/byCode")]
    [ApiResult]
    public IActionResult DataByCode([FromQuery] string code) => Ok(_service.GetDataByCode(code));
}
