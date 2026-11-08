using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Org.Service;

namespace SleveenAdmin.System.Org.Controller;

[Route("biz/sysorglist")]
public class BizSysOrgListController : BizControllerBase<BizSysOrgRow>
{
    private readonly BizSysOrgService _service;

    public BizSysOrgListController(BizSysOrgService service) => _service = service;

    protected override BizServiceBase<BizSysOrgRow> Service => _service;

    public override string BizName => "Biz_Sys_OrgList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysOrgRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("tree")]
    public IActionResult Tree() => Ok(_service.LoadTree());
}
