using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Controller;

[Route("biz/sysportcelllinklist")]
public class BizSysPortCellLinkListController : BizControllerBase<BizSysPortCellLinkRow>
{
    private readonly BizSysPortCellLinkService _service;

    public BizSysPortCellLinkListController(BizSysPortCellLinkService service) => _service = service;

    protected override BizServiceBase<BizSysPortCellLinkRow> Service => _service;

    public override string BizName => "Biz_Sys_PortCellLinkList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysPortCellLinkRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
