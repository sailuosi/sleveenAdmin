using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Officeman.Ebook.Service;

namespace SleveenAdmin.Officeman.Ebook.Controller;

[Route("biz/saebcatalist")]
public class BizSaEbCataListController : BizControllerBase
{
    private readonly BizSaEbCataService _service;

    public BizSaEbCataListController(BizSaEbCataService service) => _service = service;

    protected override BizServiceBase Service => _service;

    public override string BizName => "Biz_Sa_EbCataList";

    [HttpPost("ebook/ebcata/qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("ebook/ebcata/qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("ebook/ebcata/save")]
    public IActionResult Save([FromBody] JsonElement rows) => SaveCore(rows);

    [HttpPost("ebook/ebcata/rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
