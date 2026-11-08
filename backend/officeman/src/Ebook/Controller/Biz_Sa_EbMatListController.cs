using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Officeman.Ebook.Service;

namespace SleveenAdmin.Officeman.Ebook.Controller;

[Route("biz/saebmatlist")]
public class BizSaEbMatListController : BizControllerBase<BizSaEbMatRow>
{
    private readonly BizSaEbMatService _service;

    public BizSaEbMatListController(BizSaEbMatService service) => _service = service;

    protected override BizServiceBase<BizSaEbMatRow> Service => _service;

    public override string BizName => "Biz_Sa_EbMatList";

    [HttpPost("ebook/ebmat/qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("ebook/ebmat/qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("ebook/ebmat/save")]
    public IActionResult Save([FromBody] List<BizSaEbMatRow> rows) => SaveCore(rows);

    [HttpPost("ebook/ebmat/rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("ebook/ebmat/stats")]
    public IActionResult Stats() => Ok(_service.GetStats());
}
