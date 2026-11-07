using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Officeman.Ebook.Service;

namespace SleveenAdmin.Officeman.Ebook.Controller;

[Route("biz/saebnotelist")]
public class BizSaEbNoteListController : BizControllerBase
{
    private readonly BizSaEbNoteService _service;

    public BizSaEbNoteListController(BizSaEbNoteService service) => _service = service;

    protected override BizServiceBase Service => _service;

    public override string BizName => "Biz_Sa_EbNoteList";

    [HttpPost("ebook/ebnote/qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("ebook/ebnote/qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("ebook/ebnote/save")]
    public IActionResult Save([FromBody] JsonElement rows) => SaveCore(rows);

    [HttpPost("ebook/ebnote/rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
