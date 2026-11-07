using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.User.Dtos;
using SleveenAdmin.System.User.Service;

namespace SleveenAdmin.System.User.Controller;

[Authorize]
[ApiController]
[Route("api/sysUser")]
public class UserController : ControllerBase
{
    private readonly UserService _service;

    public UserController(UserService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PageQuery query) => Ok(_service.GetPage(query));

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _service.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("用户不存在")) : Ok(dto);
    }

    [HttpPost]
    [Permission("manage:user:add")]
    [ApiResult]
    public IActionResult Create([FromBody] SysUserInput input) => Ok(new { id = _service.Create(input) });

    [HttpPut]
    [Permission("manage:user:edit")]
    [ApiResult]
    public IActionResult Update([FromBody] SysUserInput input)
    {
        _service.Update(input);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Permission("manage:user:delete")]
    [ApiResult]
    public IActionResult Delete(Guid id)
    {
        _service.Delete(id);
        return Ok();
    }
}
