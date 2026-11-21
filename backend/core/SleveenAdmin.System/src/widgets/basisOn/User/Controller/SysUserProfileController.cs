using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.User.Dtos;
using SleveenAdmin.System.User.Service;

namespace SleveenAdmin.System.User.Controller;

/// <summary>个人中心资料（对齐前端 /api/sysUser/baseInfo）</summary>
[Authorize]
[ApiController]
[Route("api/sysUser")]
public class SysUserProfileController : ControllerBase
{
    private readonly SysUserProfileService _profile;

    public SysUserProfileController(SysUserProfileService profile) => _profile = profile;

    [HttpGet("baseInfo")]
    [ApiResult]
    public IActionResult GetBaseInfo()
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null)
            return Unauthorized(ApiResult.Fail("未登录", ApiResultCodes.Unauthorized));

        var info = _profile.GetBaseInfo(userId.Value);
        if (info is null)
            return NotFound(ApiResult.Fail("用户不存在"));

        return Ok(info);
    }

    [HttpPost("baseInfo")]
    [ApiResult]
    public IActionResult UpdateBaseInfo([FromBody] SysUserBaseInfoDto input)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null)
            return Unauthorized(ApiResult.Fail("未登录", ApiResultCodes.Unauthorized));

        try
        {
            var updated = _profile.UpdateBaseInfo(userId.Value, input ?? new SysUserBaseInfoDto());
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResult.Fail(ex.Message));
        }
    }
}
