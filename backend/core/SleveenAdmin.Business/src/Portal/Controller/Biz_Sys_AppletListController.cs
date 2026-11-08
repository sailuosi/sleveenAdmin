using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Controller;

[Route("biz/sysappletlist")]
public class BizSysAppletListController : BizControllerBase<BizSysAppletRow>
{
    private readonly BizSysAppletService _service;

    public BizSysAppletListController(BizSysAppletService service) => _service = service;

    protected override BizServiceBase<BizSysAppletRow> Service => _service;

    public override string BizName => "Biz_Sys_AppletList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysAppletRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
