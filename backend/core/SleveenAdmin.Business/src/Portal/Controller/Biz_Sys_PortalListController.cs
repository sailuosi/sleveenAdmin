using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Controller;

[Route("biz/sysportallist")]
public class BizSysPortalListController : BizControllerBase<BizSysPortalRow>
{
    private readonly BizSysPortalService _service;

    public BizSysPortalListController(BizSysPortalService service) => _service = service;

    protected override BizServiceBase<BizSysPortalRow> Service => _service;

    public override string BizName => "Biz_Sys_PortalList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysPortalRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
