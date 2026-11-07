using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.Monitor.Dtos;
using SleveenAdmin.System.Monitor.Service;

namespace SleveenAdmin.System.Monitor.Controller;

[Authorize]
[ApiController]
[Route("api/sysRedis")]
public class RedisController : ControllerBase
{
    private readonly RedisService _redisService;

    public RedisController(RedisService redisService) => _redisService = redisService;

    [HttpGet("info")]
    [ApiResult]
    public IActionResult Info() => Ok(_redisService.GetInfo());

    [HttpGet("keys/page")]
    [ApiResult]
    public IActionResult KeysPage([FromQuery] RedisKeyQuery query) => Ok(_redisService.GetKeysPage(query));

    [HttpGet("keys/{*key}")]
    [ApiResult]
    public IActionResult GetKey(string key)
    {
        var dto = _redisService.GetKeyValue(key);
        return dto is null ? NotFound(ApiResult.Fail("Key 不存在")) : Ok(dto);
    }

    [Authorize(Policy = AuthRoles.SuperAdmin)]
    [HttpDelete("keys/{*key}")]
    [ApiResult]
    public IActionResult DeleteKey(string key)
    {
        _redisService.DeleteKey(key);
        return Ok();
    }
}
