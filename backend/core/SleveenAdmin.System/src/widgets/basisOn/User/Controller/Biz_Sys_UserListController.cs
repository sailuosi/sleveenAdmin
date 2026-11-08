using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.User.Service;

namespace SleveenAdmin.System.User.Controller;

[Route("biz/sysuserlist")]
public class BizSysUserListController : BizControllerBase<BizSysUserRow>
{
    private readonly BizSysUserService _service;

    public BizSysUserListController(BizSysUserService service) => _service = service;

    protected override BizServiceBase<BizSysUserRow> Service => _service;

    public override string BizName => "Biz_Sys_UserList";

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("save")]
    [Permission("manage:user:edit")]
    public IActionResult Save([FromBody] List<BizSysUserRow> rows) => SaveCore(rows);

    [HttpPost("rmbyids")]
    [Permission("manage:user:delete")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);
}
