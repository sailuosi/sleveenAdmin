using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Notice.Dtos;
using SleveenAdmin.System.Notice.Service;

namespace SleveenAdmin.System.Notice.Controller;

[Authorize]
[ApiController]
[Route("api/sysNotice")]
public class NoticeController : ControllerBase
{
    private readonly NoticeService _service;

    public NoticeController(NoticeService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PageQuery query) => Ok(_service.GetPage(query));

    [HttpGet("my/page")]
    [ApiResult]
    public IActionResult MyPage([FromQuery] PageQuery query) => Ok(_service.GetMyPage(query));

    [HttpGet("my/unread-count")]
    [ApiResult]
    public IActionResult UnreadCount() => Ok(new { count = _service.GetUnreadCount() });

    [HttpPost]
    [ApiResult]
    public IActionResult Create([FromBody] SysNoticeInput input) => Ok(new { id = _service.Create(input) });

    [HttpPost("{id:guid}/publish")]
    [ApiResult]
    public IActionResult Publish(Guid id)
    {
        _service.Publish(id);
        return Ok();
    }

    [HttpPost("{id:guid}/read")]
    [ApiResult]
    public IActionResult MarkRead(Guid id)
    {
        _service.MarkRead(id);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.Delete(id);
        return Ok();
    }
}
