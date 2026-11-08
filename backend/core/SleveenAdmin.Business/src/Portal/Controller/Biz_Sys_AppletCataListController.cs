using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Portal.Service;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Controller;

[Route("biz/sysappletcatalist")]
public class BizSysAppletCataListController : BizControllerBase<BizSysAppletCataRow>
{
    private readonly BizSysAppletCataService _service;

    public BizSysAppletCataListController(BizSysAppletCataService service) => _service = service;

    protected override BizServiceBase<BizSysAppletCataRow> Service => _service;

    public override string BizName => "Biz_Sys_AppletCataList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysAppletCataRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
