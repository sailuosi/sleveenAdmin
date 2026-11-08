using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Config.Service;

namespace SleveenAdmin.System.Config.Controller;

[Route("biz/sysconfiglist")]
public class BizSysConfigListController : BizControllerBase<BizSysConfigRow>
{
    private readonly BizSysConfigService _service;

    public BizSysConfigListController(BizSysConfigService service) => _service = service;

    protected override BizServiceBase<BizSysConfigRow> Service => _service;

    public override string BizName => "Biz_Sys_ConfigList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysConfigRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
