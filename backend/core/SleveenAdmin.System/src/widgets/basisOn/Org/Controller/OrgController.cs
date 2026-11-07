using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Org.Dtos;
using SleveenAdmin.System.Org.Service;

namespace SleveenAdmin.System.Org.Controller;

[Authorize]
[ApiController]
[Route("api/sysOrg")]
public class OrgController : ControllerBase
{
    private readonly OrgService _service;

    public OrgController(OrgService service) => _service = service;

    [HttpGet("tree")]
    [ApiResult]
    public IActionResult Tree() => Ok(_service.GetTree());

    [HttpGet("{id:guid}")]
    [ApiResult]
    public IActionResult Get(Guid id)
    {
        var dto = _service.GetById(id);
        return dto is null ? NotFound(ApiResult.Fail("组织不存在")) : Ok(dto);
    }

    [HttpPost]
    [ApiResult]
    public IActionResult Create([FromBody] SysOrgInput input) => Ok(new { id = _service.Create(input) });

    [HttpPut]
    [ApiResult]
    public IActionResult Update([FromBody] SysOrgInput input)
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
