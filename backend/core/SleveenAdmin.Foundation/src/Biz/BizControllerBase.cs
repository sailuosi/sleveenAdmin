using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;

namespace SleveenAdmin.Foundation.Biz;

/// <summary>
/// Biz 页面 Controller 基类。路由形如 <c>biz/{slug}/</c>，标准端点由子类声明。
/// </summary>
[Authorize]
[ApiController]
[ApiResult]
public abstract class BizControllerBase : ControllerBase
{
    protected abstract BizServiceBase Service { get; }

    public abstract string BizName { get; }

    [HttpPost("init")]
    public virtual IActionResult Init([FromBody] JsonElement? _) => Ok(Service.GetInitData());

    protected IActionResult QListCore([FromBody] BizQueryInput input) => Ok(Service.LoadPageList(input ?? new BizQueryInput()));

    protected IActionResult QOneCore(Guid id) => Ok(Service.LoadRowById(id));

    protected IActionResult SaveCore([FromBody] JsonElement rows) => Ok(new { affected = Service.SaveList(rows) });

    protected IActionResult RemoveCore([FromBody] JsonElement para)
    {
        var ids = ExtractIds(para);
        return Ok(new { affected = Service.RemoveByOIDs(ids) });
    }

    protected List<Guid> ExtractIds(JsonElement para)
    {
        var ids = new List<Guid>();
        var pkName = Service.PrimaryOidColumn;

        if (para.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in para.EnumerateArray())
            {
                if (el.ValueKind == JsonValueKind.String && Guid.TryParse(el.GetString(), out var g))
                    ids.Add(g);
                else if (el.ValueKind == JsonValueKind.Object)
                {
                    if (TryReadOidFromObject(el, pkName, out var g2))
                        ids.Add(g2);
                }
            }

            return ids;
        }

        if (para.ValueKind == JsonValueKind.Object)
        {
            if (para.TryGetProperty("ids", out var arr) || para.TryGetProperty("OIDs", out arr) ||
                para.TryGetProperty("oids", out arr))
            {
                foreach (var el in arr.EnumerateArray())
                {
                    if (Guid.TryParse(el.ToString(), out var g))
                        ids.Add(g);
                }
            }
            else if (TryReadOidFromObject(para, pkName, out var single))
            {
                ids.Add(single);
            }
        }

        return ids;
    }

    /// <summary>对象中只取主键列或 id，不扫任意 *OID 外键。</summary>
    static bool TryReadOidFromObject(JsonElement obj, string pkName, out Guid guid)
    {
        guid = Guid.Empty;
        foreach (var p in obj.EnumerateObject())
        {
            if (p.Name.Equals(pkName, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                if (Guid.TryParse(p.Value.ToString(), out guid) && guid != Guid.Empty)
                    return true;
            }
        }

        return false;
    }
}
