using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Dict.Service;

namespace SleveenAdmin.System.Dict.Controller;

[Route("biz/sysdictdatalist")]
public class BizSysDictDataListController : BizControllerBase<BizSysDictDataRow>
{
    private readonly BizSysDictDataService _service;

    public BizSysDictDataListController(BizSysDictDataService service) => _service = service;

    protected override BizServiceBase<BizSysDictDataRow> Service => _service;

    public override string BizName => "Biz_Sys_DictDataList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysDictDataRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
