using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.SysPost.Service;

namespace SleveenAdmin.System.SysPost.Controller;

[Route("bpo/sys/post")]
public class BizSysPostListController : BizControllerBase<BizSysPostRow>
{
    private readonly BizSysPostService _service;
    public BizSysPostListController(BizSysPostService service) => _service = service;
    protected override BizServiceBase<BizSysPostRow> Service => _service;
    public override string BizName => "Biz_Sys_PostList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysPostRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}

[Route("bpo/sys/post-type")]
public class BizSysPostTypeSetController : BizControllerBase<BizSysPostTypeRow>
{
    private readonly BizSysPostTypeService _types;
    private readonly BizSysPostTypeRoleService _roles;
    private readonly BizSysPostTypeChildService _childs;

    public BizSysPostTypeSetController(
        BizSysPostTypeService types,
        BizSysPostTypeRoleService roles,
        BizSysPostTypeChildService childs)
    {
        _types = types;
        _roles = roles;
        _childs = childs;
    }

    protected override BizServiceBase<BizSysPostTypeRow> Service => _types;
    public override string BizName => "Biz_Sys_PostTypeSet";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysPostTypeRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("role/qlist")]
    public IActionResult RoleQList([FromBody] BizQueryInput input) => Ok(_roles.LoadPageList(input));

    [HttpGet("role/qone/{id:guid}")]
    public IActionResult RoleQOne(Guid id) => Ok(_roles.LoadRowById(id));

    [HttpPost("role/save")]
    public IActionResult RoleSave([FromBody] List<BizSysPostTypeRoleRow> rows) => Ok(_roles.SaveList(rows ?? []));

    [HttpPost("role/rmbyids")]
    public IActionResult RoleRemove([FromBody] JsonElement para) =>
        Ok(_roles.RemoveByOIDs(ExtractIds(para)));

    [HttpPost("child/qlist")]
    public IActionResult ChildQList([FromBody] BizQueryInput input) => Ok(_childs.LoadPageList(input));

    [HttpGet("child/qone/{id:guid}")]
    public IActionResult ChildQOne(Guid id) => Ok(_childs.LoadRowById(id));

    [HttpPost("child/save")]
    public IActionResult ChildSave([FromBody] List<BizSysPostTypeChildRow> rows) => Ok(_childs.SaveList(rows ?? []));

    [HttpPost("child/rmbyids")]
    public IActionResult ChildRemove([FromBody] JsonElement para) =>
        Ok(_childs.RemoveByOIDs(ExtractIds(para)));
}

[Route("bpo/sys/post-scope")]
public class BizSysPostScopeController : BizControllerBase<BizSysPostScopeRow>
{
    private readonly BizSysPostScopeService _service;
    public BizSysPostScopeController(BizSysPostScopeService service) => _service = service;
    protected override BizServiceBase<BizSysPostScopeRow> Service => _service;
    public override string BizName => "Biz_Sys_PostScope";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    public IActionResult Save([FromBody] List<BizSysPostScopeRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
