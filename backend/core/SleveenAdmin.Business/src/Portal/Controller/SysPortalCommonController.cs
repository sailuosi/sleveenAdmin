using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using System.Text.Json.Nodes;

namespace SleveenAdmin.Business.Portal.Controller;

[Authorize]
[ApiController]
[Route("api/sysPortal/common")]
[Route("portal/common")] // minehub / sysext 门户组件兼容路径
public class SysPortalCommonController : ControllerBase
{
    private readonly SysPortalCommonService _service;

    public SysPortalCommonController(SysPortalCommonService service) => _service = service;

    [HttpPost("loadportalconfig")]
    [ApiResult]
    public IActionResult LoadPortalConfig([FromBody] StringValueInput input)
    {
        var data = _service.LoadPortalConfig(input.Value ?? "", User);
        return Ok(data);
    }

    [HttpPost("loadportalconfigbyoid")]
    [ApiResult]
    public IActionResult LoadPortalConfigByOid([FromBody] StringValueInput input)
    {
        // minehub ParaValue.value 为字符串 OID；避免 Guid 绑定失败 → Empty → 假 404
        if (!Guid.TryParse(input.Value, out var oid) || oid == Guid.Empty)
            return Ok(null);

        var data = _service.LoadPortalConfigByOid(oid, User);
        return Ok(data);
    }

    [HttpPost("loadhomeportal")]
    [ApiResult]
    public IActionResult LoadHomePortal([FromBody] StringValueInput input)
    {
        // 未命中时仍 200 + data=null，避免首页被 request 当成 HTTP 404；前端 parse 后空渲染
        var data = _service.LoadHomePortal(input.Value ?? "home", User);
        return Ok(data);
    }

    [HttpPost("savecell")]
    [ApiResult]
    public IActionResult SaveCell([FromBody] SaveCellsInput input)
    {
        _service.SaveCells(input.PortalId, input.Cells ?? new JsonArray(), User);
        return Ok();
    }

    [HttpPost("saveapplets")]
    [ApiResult]
    public IActionResult SaveApplets([FromBody] SaveAppletsInput input)
    {
        _service.SaveApplets(input.PortalId, input.Applets ?? new JsonArray());
        return Ok();
    }
}
