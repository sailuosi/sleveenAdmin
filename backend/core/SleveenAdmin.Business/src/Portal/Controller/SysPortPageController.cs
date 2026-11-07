using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation;
using SleveenAdmin.Foundation.AspNet;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortPage")]
public class SysPortPageController : ControllerBase
{
    private readonly SysPortPageService _service;
    public SysPortPageController(SysPortPageService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public JObject Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetPage(query);
        return AjaxResult.success(page.Total, page.Items);
    }

    [HttpGet("{id:guid}")]
    [ApiResult]
    public JObject Get(Guid id)
        => AjaxResult.success(_service.GetById(id)!);

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPortPage> rows)
    {
        _service.SaveBatch(rows);
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
