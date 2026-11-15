using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Hr.Service;

namespace SleveenAdmin.System.Hr.Controller;

[Route("bpo/sys/hr-organize")]
public class BizHrOrganizeController : BizControllerBase<BizHrOrganizeRow>
{
    private readonly BizHrOrganizeService _service;
    public BizHrOrganizeController(BizHrOrganizeService service) => _service = service;
    protected override BizServiceBase<BizHrOrganizeRow> Service => _service;
    public override string BizName => "Biz_HR_Organize";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizHrOrganizeRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("tree")]
    public IActionResult Tree([FromBody] JsonElement? para)
    {
        Guid? root = null;
        if (para is { ValueKind: JsonValueKind.Object } &&
            para.Value.TryGetProperty("oid", out var o) &&
            Guid.TryParse(o.ToString(), out var g))
            root = g;
        return Ok(_service.LoadTree(root));
    }
}
