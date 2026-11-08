using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Notice.Service;

namespace SleveenAdmin.System.Notice.Controller;

[Route("biz/sysnoticelist")]
public class BizSysNoticeListController : BizControllerBase<BizSysNoticeRow>
{
    private readonly BizSysNoticeService _service;

    public BizSysNoticeListController(BizSysNoticeService service) => _service = service;

    protected override BizServiceBase<BizSysNoticeRow> Service => _service;

    public override string BizName => "Biz_Sys_NoticeList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysNoticeRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("publish/{id:guid}")]
    public IActionResult Publish(Guid id)
    {
        _service.Publish(id);
        return Ok();
    }
}
