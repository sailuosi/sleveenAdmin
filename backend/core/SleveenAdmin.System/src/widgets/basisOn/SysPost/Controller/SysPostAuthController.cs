using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.SysPost.Service;

namespace SleveenAdmin.System.SysPost.Controller;

/// <summary>人员岗位权限分配 / 授权人名录。</summary>
[Authorize]
[ApiController]
[ApiResult]
[Route("bpo/sys/post-auth")]
public class SysPostAuthController : ControllerBase
{
    private readonly SysPostAuthService _auth;
    private readonly IAuthUserContext _user;

    public SysPostAuthController(SysPostAuthService auth, IAuthUserContext user)
    {
        _auth = auth;
        _user = user;
    }

    [HttpPost("init")]
    public IActionResult Init() => Ok(new
    {
        codeTable = new Dictionary<string, object>
        {
            ["HH_DOrgSrc"] = new[]
            {
                new { value = PostOrgSrc.AdminOrg, label = "行政组织" },
                new { value = PostOrgSrc.Place, label = "位置" }
            }
        }
    });

    [HttpGet("load/{userId:guid}")]
    public IActionResult Load(Guid userId) => Ok(_auth.LoadUserAuth(userId));

    [HttpPost("posts/save/{userId:guid}")]
    public IActionResult SavePosts(Guid userId, [FromBody] List<BizSysPostRow> posts) =>
        Ok(_auth.SavePosts(userId, posts ?? [], _user.RealName ?? _user.Account));

    [HttpPost("posts/rmbyids")]
    public IActionResult RemovePosts([FromBody] List<Guid> ids) =>
        Ok(new { affected = _auth.RemovePosts(ids ?? []) });

    [HttpPost("scopes/save/{userId:guid}")]
    public IActionResult SaveScopes(Guid userId, [FromBody] List<BizSysPostScopeRow> scopes)
    {
        Guid? actor = _user.IsSuperAdmin ? null : _user.UserId;
        return Ok(_auth.SaveScopes(userId, scopes ?? [], actor));
    }

    [HttpPost("scopes/rmbyids")]
    public IActionResult RemoveScopes([FromBody] JsonElement body)
    {
        var ids = new List<Guid>();
        Guid? userId = null;
        if (body.ValueKind == JsonValueKind.Object)
        {
            if (body.TryGetProperty("userId", out var u) && Guid.TryParse(u.ToString(), out var uid))
                userId = uid;
            if (body.TryGetProperty("ids", out var arr))
            {
                foreach (var el in arr.EnumerateArray())
                    if (Guid.TryParse(el.ToString(), out var g)) ids.Add(g);
            }
        }
        else if (body.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in body.EnumerateArray())
                if (Guid.TryParse(el.ToString(), out var g)) ids.Add(g);
        }
        return Ok(new { affected = _auth.RemoveScopes(ids, userId) });
    }

    [HttpGet("assignable-types/{postId:guid}")]
    public IActionResult AssignableTypes(Guid postId)
    {
        return Ok(_auth.LoadAssignableChildTypeIds(_user.UserId, postId));
    }

    [HttpPost("clear-cache/{userId:guid}")]
    public IActionResult ClearCache(Guid userId)
    {
        SysPostAuthService.ClearUserDutyCache(userId);
        return Ok(new { ok = true });
    }
}

[Authorize]
[ApiController]
[ApiResult]
[Route("bpo/sys/post-directory")]
public class SysPostDirectoryController : ControllerBase
{
    private readonly SysPostAuthService _auth;
    public SysPostDirectoryController(SysPostAuthService auth) => _auth = auth;

    [HttpPost("query")]
    public IActionResult Query([FromBody] JsonElement para)
    {
        var orgSrc = para.TryGetProperty("orgSrc", out var s) ? s.GetString() ?? PostOrgSrc.AdminOrg : PostOrgSrc.AdminOrg;
        if (!para.TryGetProperty("orgId", out var o) || !Guid.TryParse(o.ToString(), out var orgId))
            return BadRequest("orgId required");
        return Ok(_auth.LoadDirectory(orgSrc, orgId));
    }
}

[Authorize]
[ApiController]
[ApiResult]
[Route("bpo/sys/duty-check")]
public class SysDutyCheckController : ControllerBase
{
    [HttpPost("init")]
    public IActionResult Init() => Ok(new { BizName = "Biz_Sys_DutyCheck" });

    [HttpPost("qlist")]
    public IActionResult QList([FromBody] BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = SleveenAdmin.Foundation.Data.DBCash.useSQL(SleveenAdmin.Foundation.Data.MooSqlSetup.DefaultPosition)
            .select("Adi_Id as Id, Adi_Name as Name, Adi_Code as Code, Adi_Idx as Idx, Adi_Note as Note")
            .from("AuthDutyItem")
            .where("(Adi_IsDeleted=0 or Adi_IsDeleted is null)");
        if (!string.IsNullOrWhiteSpace(input.Keyword))
            kit.whereLikes(["Adi_Name", "Adi_Code"], input.Keyword.Trim());
        var total = kit.count();
        var dt = kit.setPage(size, page).orderBy("Adi_Idx asc, Adi_Name asc").query();
        var rows = new List<object>();
        foreach (global::System.Data.DataRow row in dt.Rows)
        {
            rows.Add(new
            {
                Id = row["Id"],
                Name = row["Name"]?.ToString(),
                Code = row["Code"]?.ToString(),
                Idx = row["Idx"],
                Note = row["Note"]?.ToString(),
                HH_DutyItemOID = row["Id"],
                Di_Name = row["Name"]?.ToString(),
                Di_Code = row["Code"]?.ToString()
            });
        }
        return Ok(PagedResult<object>.Create(rows, page, size, total));
    }
}
