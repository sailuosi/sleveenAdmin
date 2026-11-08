using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Tenant.Service;

namespace SleveenAdmin.System.Tenant.Controller;

[Authorize(Policy = AuthRoles.SuperAdmin)]
[Route("biz/systenantlist")]
public class BizSysTenantListController : BizControllerBase<BizSysTenantRow>
{
    private readonly BizSysTenantService _service;

    public BizSysTenantListController(BizSysTenantService service) => _service = service;

    protected override BizServiceBase<BizSysTenantRow> Service => _service;

    public override string BizName => "Biz_Sys_TenantList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysTenantRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    /// <summary>下拉等场景全量列表。</summary>
    [HttpGet("list")]
    public IActionResult List() => Ok(_service.LoadAll());
}
