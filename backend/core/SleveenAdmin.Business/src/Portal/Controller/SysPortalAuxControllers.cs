using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortalRole")]
public class SysPortalRoleController : ControllerBase
{
    private readonly SysPortalAuxService _service;
    public SysPortalRoleController(SysPortalAuxService service) => _service = service;

    [HttpGet("page")]
    public ContentResult Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetRolePage(query);
        return AjaxResult.ToContent(AjaxResult.success(page.Total, page.Items));
    }

    [HttpGet("{id:guid}")]
    public ContentResult Get(Guid id)
        => AjaxResult.ToContent(AjaxResult.success(_service.GetRole(id)!));

    [HttpPost]
    public ContentResult Save([FromBody] List<SysPortalRole> rows)
    {
        _service.SaveRoles(rows);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpDelete("{id:guid}")]
    public ContentResult Delete(Guid id)
    {
        _service.DeleteRole(id);
        return AjaxResult.ToContent(AjaxResult.success("删除成功"));
    }
}

[Authorize]
[ApiController]
[Route("api/sysPortalLink")]
public class SysPortalLinkController : ControllerBase
{
    private readonly SysPortalAuxService _service;
    public SysPortalLinkController(SysPortalAuxService service) => _service = service;

    [HttpGet("page")]
    public ContentResult Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetLinkPage(query);
        return AjaxResult.ToContent(AjaxResult.success(page.Total, page.Items));
    }

    [HttpGet("{id:guid}")]
    public ContentResult Get(Guid id)
        => AjaxResult.ToContent(AjaxResult.success(_service.GetLink(id)!));

    [HttpPost]
    public ContentResult Save([FromBody] List<SysPortalLink> rows)
    {
        _service.SaveLinks(rows);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpDelete("{id:guid}")]
    public ContentResult Delete(Guid id)
    {
        _service.DeleteLink(id);
        return AjaxResult.ToContent(AjaxResult.success("删除成功"));
    }
}

[Authorize]
[ApiController]
[Route("api/sysPageSelect")]
public class SysPageSelectController : ControllerBase
{
    private readonly SysPortalAuxService _service;
    public SysPageSelectController(SysPortalAuxService service) => _service = service;

    [HttpGet("page")]
    public ContentResult Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetSelectPage(query);
        return AjaxResult.ToContent(AjaxResult.success(page.Total, page.Items));
    }

    [HttpGet("{id:guid}")]
    public ContentResult Get(Guid id)
        => AjaxResult.ToContent(AjaxResult.success(_service.GetSelect(id)!));

    [HttpPost]
    public ContentResult Save([FromBody] List<SysPageSelect> rows)
    {
        _service.SaveSelects(rows);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpDelete("{id:guid}")]
    public ContentResult Delete(Guid id)
    {
        _service.DeleteSelect(id);
        return AjaxResult.ToContent(AjaxResult.success("删除成功"));
    }
}

[Authorize]
[ApiController]
[Route("api/sysPageField")]
public class SysPageFieldController : ControllerBase
{
    private readonly SysPortalAuxService _service;
    public SysPageFieldController(SysPortalAuxService service) => _service = service;

    [HttpGet("page")]
    public ContentResult Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetFieldPage(query);
        return AjaxResult.ToContent(AjaxResult.success(page.Total, page.Items));
    }

    [HttpGet("{id:guid}")]
    public ContentResult Get(Guid id)
        => AjaxResult.ToContent(AjaxResult.success(_service.GetField(id)!));

    [HttpPost]
    public ContentResult Save([FromBody] List<SysPageField> rows)
    {
        _service.SaveFields(rows);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpDelete("{id:guid}")]
    public ContentResult Delete(Guid id)
    {
        _service.DeleteField(id);
        return AjaxResult.ToContent(AjaxResult.success("删除成功"));
    }
}

[Authorize]
[ApiController]
[Route("api/sysPageConnect")]
public class SysPageConnectController : ControllerBase
{
    private readonly SysPortalAuxService _service;
    public SysPageConnectController(SysPortalAuxService service) => _service = service;

    [HttpGet("page")]
    public ContentResult Page([FromQuery] PortalPageQuery query)
    {
        var page = _service.GetConnectPage(query);
        return AjaxResult.ToContent(AjaxResult.success(page.Total, page.Items));
    }

    [HttpGet("{id:guid}")]
    public ContentResult Get(Guid id)
        => AjaxResult.ToContent(AjaxResult.success(_service.GetConnect(id)!));

    [HttpPost]
    public ContentResult Save([FromBody] List<SysPageConnect> rows)
    {
        _service.SaveConnects(rows);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpDelete("{id:guid}")]
    public ContentResult Delete(Guid id)
    {
        _service.DeleteConnect(id);
        return AjaxResult.ToContent(AjaxResult.success("删除成功"));
    }
}
