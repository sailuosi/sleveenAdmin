using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Tenant.Dtos;
using SleveenAdmin.System.Tenant.Service;

namespace SleveenAdmin.System.Tenant.Controller;

[Authorize(Policy = AuthRoles.SuperAdmin)]
[ApiController]
[Route("api/sysTenant")]
public class TenantController : ControllerBase
{
    private readonly TenantService _service;

    public TenantController(TenantService service) => _service = service;

    [HttpGet("page")]
    [ApiResult]
    public IActionResult Page([FromQuery] PageQuery query) => Ok(_service.GetPage(query));

    [HttpGet("list")]
    [ApiResult]
    public IActionResult List() => Ok(_service.GetList());

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _service.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("租户不存在")) : Ok(dto);
    }

    [HttpPost]
    [ApiResult]
    public IActionResult Create([FromBody] SysTenantInput input) => Ok(new { id = _service.Create(input) });

    [HttpPut]
    [ApiResult]
    public IActionResult Update([FromBody] SysTenantInput input)
    {
        _service.Update(input);
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
