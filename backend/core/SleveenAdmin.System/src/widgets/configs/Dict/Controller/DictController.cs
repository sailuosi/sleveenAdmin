using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Dict.Dtos;
using SleveenAdmin.System.Dict.Service;

namespace SleveenAdmin.System.Dict.Controller;

[Authorize]
[ApiController]
[Route("api/sysDict")]
public class DictController : ControllerBase
{
    private readonly DictService _service;

    public DictController(DictService service) => _service = service;

    [HttpGet("type/page")]
    [ApiResult]
    public IActionResult TypePage([FromQuery] PageQuery query) => Ok(_service.GetTypePage(query));

    [HttpGet("type/{id:guid}")]
    [ApiResult]
    public IActionResult GetType(Guid id)
    {
        var dto = _service.GetTypeById(id);
        return dto is null ? NotFound(ApiResult.Fail("字典类型不存在")) : Ok(dto);
    }

    [HttpPost("type")]
    [ApiResult]
    public IActionResult CreateType([FromBody] SysDictTypeInput input) => Ok(new { id = _service.CreateType(input) });

    [HttpPut("type")]
    [ApiResult]
    public IActionResult UpdateType([FromBody] SysDictTypeInput input)
    {
        _service.UpdateType(input);
        return Ok();
    }

    [HttpDelete("type/{id:guid}")]
    [ApiResult]
    public IActionResult DeleteType(Guid id)
    {
        _service.DeleteType(id);
        return Ok();
    }

    [HttpGet("data/page")]
    [ApiResult]
    public IActionResult DataPage([FromQuery] Guid typeId, [FromQuery] PageQuery query) =>
        Ok(_service.GetDataPage(typeId, query));

    [HttpGet("data/byCode")]
    [ApiResult]
    public IActionResult DataByCode([FromQuery] string code) => Ok(_service.GetDataByCode(code));

    [HttpGet("data/{id:guid}")]
    [ApiResult]
    public IActionResult GetData(Guid id)
    {
        var dto = _service.GetDataById(id);
        return dto is null ? NotFound(ApiResult.Fail("字典数据不存在")) : Ok(dto);
    }

    [HttpPost("data")]
    [ApiResult]
    public IActionResult CreateData([FromBody] SysDictDataInput input) => Ok(new { id = _service.CreateData(input) });

    [HttpPut("data")]
    [ApiResult]
    public IActionResult UpdateData([FromBody] SysDictDataInput input)
    {
        _service.UpdateData(input);
        return Ok();
    }

    [HttpDelete("data/{id:guid}")]
    [ApiResult]
    public IActionResult DeleteData(Guid id)
    {
        _service.DeleteData(id);
        return Ok();
    }
}
