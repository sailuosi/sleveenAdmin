using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Controller;

[Route("biz/sysportcelllist")]
public class BizSysPortCellListController : BizControllerBase
{
    private readonly BizSysPortCellService _service;

    public BizSysPortCellListController(BizSysPortCellService service) => _service = service;

    protected override BizServiceBase Service => _service;

    public override string BizName => "Biz_Sys_PortCellList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] JsonElement rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
