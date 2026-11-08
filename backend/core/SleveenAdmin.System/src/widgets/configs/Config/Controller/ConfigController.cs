using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Config.Service;

namespace SleveenAdmin.System.Config.Controller;

/// <summary>运行时配置读取；管理 CRUD 见 <see cref="BizSysConfigListController"/>。</summary>
[Authorize]
[ApiController]
[Route("api/sysConfig")]
public class ConfigController : ControllerBase
{
    private readonly BizSysConfigService _service;

    public ConfigController(BizSysConfigService service) => _service = service;

    [HttpGet("code/{code}")]
    [ApiResult]
    public IActionResult GetByCode(string code)
    {
        var dto = _service.GetByCode(code);
        return dto is null ? NotFound(ApiResult.Fail("配置不存在")) : Ok(dto);
    }
}
