using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Officeman.Ebook.Service;

namespace SleveenAdmin.Officeman.Ebook.Controller;

[Route("biz/saebcatalist")]
public class BizSaEbCataListController : BizControllerBase<BizSaEbCataRow>
{
    private readonly BizSaEbCataService _service;

    public BizSaEbCataListController(BizSaEbCataService service) => _service = service;

    protected override BizServiceBase<BizSaEbCataRow> Service => _service;

    public override string BizName => "Biz_Sa_EbCataList";

    [HttpPost("ebook/ebcata/qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("ebook/ebcata/qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("ebook/ebcata/save")]
    public IActionResult Save([FromBody] List<BizSaEbCataRow> rows) => SaveCore(rows);

    [HttpPost("ebook/ebcata/rmbyids")]
    public IActionResult Remove([FromBody] System.Text.Json.JsonElement para) => RemoveCore(para);
}
