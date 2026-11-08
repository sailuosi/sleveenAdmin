using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Dict.Service;

namespace SleveenAdmin.System.Dict.Controller;

[Route("biz/sysdicttypelist")]
public class BizSysDictTypeListController : BizControllerBase<BizSysDictTypeRow>
{
    private readonly BizSysDictTypeService _service;

    public BizSysDictTypeListController(BizSysDictTypeService service) => _service = service;

    protected override BizServiceBase<BizSysDictTypeRow> Service => _service;

    public override string BizName => "Biz_Sys_DictTypeList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysDictTypeRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
