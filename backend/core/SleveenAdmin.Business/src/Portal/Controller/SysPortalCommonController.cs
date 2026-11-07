using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation;
using System.Text.Json;
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
    public ContentResult LoadPortalConfig([FromBody] StringValueInput input)
    {
        var data = _service.LoadPortalConfig(input.Value ?? "", User);
        return AjaxResult.ToContent(WrapConfig(data));
    }

    [HttpPost("loadportalconfigbyoid")]
    public ContentResult LoadPortalConfigByOid([FromBody] StringValueInput input)
    {
        if (!Guid.TryParse(input.Value, out var oid) || oid == Guid.Empty)
            return AjaxResult.ToContent(AjaxResult.success(new JObject()));

        var data = _service.LoadPortalConfigByOid(oid, User);
        return AjaxResult.ToContent(WrapConfig(data));
    }

    [HttpPost("loadhomeportal")]
    public ContentResult LoadHomePortal([FromBody] StringValueInput input)
    {
        var data = _service.LoadHomePortal(input.Value ?? "home", User);
        return AjaxResult.ToContent(WrapConfig(data));
    }

    [HttpPost("savecell")]
    public ContentResult SaveCell([FromBody] JsonObject? body)
    {
        var (portalId, cells) = ParseSaveCellsBody(body);
        if (portalId == Guid.Empty)
            return AjaxResult.ToContent(AjaxResult.error("portalId 无效"));
        _service.SaveCells(portalId, cells, User);
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpPost("saveapplets")]
    public ContentResult SaveApplets([FromBody] SaveAppletsInput input)
    {
        _service.SaveApplets(input.PortalId, input.Applets ?? new JsonArray());
        return AjaxResult.ToContent(AjaxResult.success("保存成功"));
    }

    [HttpPost("clearportalconfigbyoid")]
    public ContentResult ClearPortalConfigByOid([FromBody] StringValueInput input)
    {
        Guid.TryParse(input.Value, out var oid);
        _service.InvalidatePortalConfig(oid, input.Key);
        return AjaxResult.ToContent(AjaxResult.success("缓存已清除"));
    }

    static JObject WrapConfig(JsonObject? data)
    {
        if (data is null)
            return AjaxResult.success(new JObject());
        var token = JToken.Parse(data.ToJsonString());
        return token is JObject jo
            ? AjaxResult.success(jo)
            : AjaxResult.success(new JObject { ["value"] = token });
    }

    private static (Guid portalId, JsonArray cells) ParseSaveCellsBody(JsonObject? body)
    {
        if (body is null) return (Guid.Empty, new JsonArray());

        Guid portalId = Guid.Empty;
        if (body.TryGetPropertyValue("portalId", out var pidNode) && Guid.TryParse(pidNode?.ToString(), out var g1))
            portalId = g1;
        else if (body.TryGetPropertyValue("key", out var keyNode) && Guid.TryParse(keyNode?.ToString(), out var g2))
            portalId = g2;

        JsonArray cells = new();
        if (body.TryGetPropertyValue("cells", out var cellsNode) && cellsNode is JsonArray arr1)
            cells = arr1;
        else if (body.TryGetPropertyValue("value", out var valNode))
        {
            if (valNode is JsonArray arr2)
                cells = arr2;
            else if (valNode is JsonValue jv)
            {
                var raw = jv.ToString();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    try
                    {
                        var parsed = JsonNode.Parse(raw);
                        if (parsed is JsonArray arr3) cells = arr3;
                    }
                    catch
                    {
                        // ignore malformed legacy payload
                    }
                }
            }
        }

        return (portalId, cells);
    }
}
