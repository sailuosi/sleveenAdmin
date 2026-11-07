using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.File.Service;

namespace SleveenAdmin.System.File.Controller;

[Authorize]
[ApiController]
[Route("api/sysFile")]
public class FileController : ControllerBase
{
    private readonly FileService _service;

    public FileController(FileService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PageQuery query) => Ok(_service.GetPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _service.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("文件不存在")) : Ok(dto);
    }

    [HttpPost("upload")]
    [ApiResult]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResult.Fail("请选择文件"));

        var userId = CurrentUser.GetUserId(User);
        var dto = await _service.UploadAsync(file, userId, cancellationToken);
        return Ok(dto);
    }

    [HttpGet("download/{id:guid}")]
    public IActionResult Download(Guid id)
    {
        var info = _service.GetDownload(id);
        if (info is null)
            return NotFound();

        return PhysicalFile(info.Value.fullPath, info.Value.contentType, info.Value.downloadName);
    }

    [HttpDelete("{id:guid}")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.Delete(id);
        return Ok();
    }
}
