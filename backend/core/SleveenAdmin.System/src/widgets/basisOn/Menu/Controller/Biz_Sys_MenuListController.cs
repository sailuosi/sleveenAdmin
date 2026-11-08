using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Menu.Service;

namespace SleveenAdmin.System.Menu.Controller;

[Route("biz/sysmenulist")]
public class BizSysMenuListController : BizControllerBase<BizSysMenuRow>
{
    private readonly BizSysMenuService _service;

    public BizSysMenuListController(BizSysMenuService service) => _service = service;

    protected override BizServiceBase<BizSysMenuRow> Service => _service;

    public override string BizName => "Biz_Sys_MenuList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysMenuRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("tree")]
    public IActionResult Tree() => Ok(_service.LoadTree());
}
