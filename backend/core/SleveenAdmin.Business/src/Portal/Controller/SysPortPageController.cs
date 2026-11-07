using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortPage")]
public class SysPortPageController : ControllerBase
{
    private readonly SysPortPageService _service;
    public SysPortPageController(SysPortPageService service) => _service = service;

    [HttpGet("page")]
    public ContentResult Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetPage(query);
        return AjaxResult.ToContent(AjaxResult.success(page.Total, page.Items));
    }

    [HttpGet("{id:guid}")]
    public ContentResult Get(Guid id)
        => AjaxResult.ToContent(AjaxResult.success(_service.GetById(id)!));

    [HttpPost]
    public ContentResult Save([FromBody] List<SysPortPage> rows)
    {
        _service.SaveBatch(rows);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpDelete("{id:guid}")]
    public ContentResult Delete(Guid id)
    {
        _service.Delete(id);
        return AjaxResult.ToContent(AjaxResult.success("删除成功"));
    }
}
