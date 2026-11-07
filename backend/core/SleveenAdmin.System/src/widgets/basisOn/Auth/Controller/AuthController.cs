using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Auth.AppRelease;
using SleveenAdmin.System.Auth.Dtos;
using SleveenAdmin.System.Auth.Service;

namespace SleveenAdmin.System.Auth.Controller;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly CaptchaService _captchaService;

    public AuthController(AuthService authService, CaptchaService captchaService)
    {
        _authService = authService;
        _captchaService = captchaService;
    }

    [HttpGet("captcha")]
    [ApiResult]
    public IActionResult Captcha() => Ok(_captchaService.Create());

    /// <summary>
    /// 获取当前前端发版信息（匿名，供 WS/冷启动版本检查）
    /// </summary>
    [AllowAnonymous]
    [HttpGet("appRelease")]
    [ApiResult]
    public IActionResult GetAppRelease() => Ok(AppReleaseCatalog.Current);

    [HttpPost("login")]
    [ApiResult]
    public async Task<IActionResult> Login([FromBody] LoginInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.UserName) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest(ApiResult.Fail("用户名或密码不能为空"));

        try
        {
            var tokens = await _authService.LoginAsync(input, cancellationToken);
            if (tokens is null)
                return Unauthorized(ApiResult.Fail("用户名或密码错误", ApiResultCodes.Unauthorized));

            Response.Headers["X-Token"] = tokens.Token;
            Response.Headers["X-Refresh-Token"] = tokens.RefreshToken;
            return Ok(tokens);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResult.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("user-info")]
    [ApiResult]
    public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null)
            return Unauthorized(ApiResult.Fail("未登录", ApiResultCodes.Unauthorized));

        var info = await _authService.GetUserInfoAsync(userId.Value, cancellationToken);
        if (info is null)
            return Unauthorized(ApiResult.Fail("用户不存在", ApiResultCodes.Unauthorized));

        return Ok(info);
    }

    [HttpPost("refreshToken")]
    [ApiResult]
    public IActionResult RefreshToken([FromBody] RefreshTokenInput input)
    {
        try
        {
            var tokens = _authService.RefreshTokenAsync(input.RefreshToken);
            Response.Headers["X-Token"] = tokens.Token;
            Response.Headers["X-Refresh-Token"] = tokens.RefreshToken;
            return Ok(tokens);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(ApiResult.Fail(ex.Message, ApiResultCodes.Unauthorized));
        }
    }
}
