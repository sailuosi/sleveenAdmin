using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortal")]
public class SysPortalController : ControllerBase
{
    private readonly SysPortalService _service;

    public SysPortalController(SysPortalService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query)
    {
        // sysext vclist：requestAPI6 解包后读 res.data / res.total
        var page = _service.GetPage(query);
        return Ok(new { data = page.Records, total = page.Total });
    }

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _service.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("门户不存在")) : Ok(dto);
    }

    [HttpPost]
    [ApiResult]
    public IActionResult Create([FromBody] List<SysPortal> rows)
    {
        _service.SaveBatch(rows ?? [], User);
        return Ok("保存成功");
    }

    [HttpPut]
    [ApiResult]
    public IActionResult Update([FromBody] List<SysPortal> rows)
    {
        _service.SaveBatch(rows ?? [], User);
        return Ok("保存成功");
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.Delete(id);
        return Ok();
    }
}
