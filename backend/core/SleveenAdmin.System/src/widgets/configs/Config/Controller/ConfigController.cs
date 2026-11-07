using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Config.Dtos;
using SleveenAdmin.System.Config.Service;

namespace SleveenAdmin.System.Config.Controller;

[Authorize]
[ApiController]
[Route("api/sysConfig")]
public class ConfigController : ControllerBase
{
    private readonly ConfigService _service;

    public ConfigController(ConfigService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PageQuery query) => Ok(_service.GetPage(query));

    [HttpGet("code/{code}")]
    [ApiResult]
    public IActionResult GetByCode(string code)
    {
        var dto = _service.GetByCode(code);
        return dto is null ? NotFound(ApiResult.Fail("配置不存在")) : Ok(dto);
    }

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _service.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("配置不存在")) : Ok(dto);
    }

    [HttpPost]
    [ApiResult]
    public IActionResult Create([FromBody] SysConfigInput input) => Ok(new { id = _service.Create(input) });

    [HttpPut]
    [ApiResult]
    public IActionResult Update([FromBody] SysConfigInput input)
    {
        _service.Update(input);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.Delete(id);
        return Ok();
    }
}
