using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.AspNet;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortalRole")]
public class SysPortalRoleController : ControllerBase
{
    private readonly SysPortalAuxService _service;
    public SysPortalRoleController(SysPortalAuxService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query) => Ok(_service.GetRolePage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id) => Ok(_service.GetRole(id));

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPortalRole> rows)
    {
        _service.SaveRoles(rows);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteRole(id);
        return Ok();
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
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query) => Ok(_service.GetLinkPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id) => Ok(_service.GetLink(id));

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPortalLink> rows)
    {
        _service.SaveLinks(rows);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteLink(id);
        return Ok();
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
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query) => Ok(_service.GetSelectPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id) => Ok(_service.GetSelect(id));

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPageSelect> rows)
    {
        _service.SaveSelects(rows);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteSelect(id);
        return Ok();
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
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query) => Ok(_service.GetFieldPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id) => Ok(_service.GetField(id));

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPageField> rows)
    {
        _service.SaveFields(rows);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteField(id);
        return Ok();
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
    [ApiResult]
    public IActionResult Page([FromQuery] PortalPageQuery query) => Ok(_service.GetConnectPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id) => Ok(_service.GetConnect(id));

    [HttpPost]
    [ApiResult]
    public IActionResult Save([FromBody] List<SysPageConnect> rows)
    {
        _service.SaveConnects(rows);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteConnect(id);
        return Ok();
    }
}
