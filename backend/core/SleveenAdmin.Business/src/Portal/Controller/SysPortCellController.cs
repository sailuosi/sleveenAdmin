using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.AspNet;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortCell")]
public class SysPortCellController : ControllerBase
{
    private readonly SysPortCellService _service;
    public SysPortCellController(SysPortCellService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query) => Ok(_service.GetPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id) => Ok(_service.GetById(id));

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPortCell> rows)
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
