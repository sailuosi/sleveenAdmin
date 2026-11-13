using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Client;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>探活 / InitDb；路由 api/sail/Probe。故意匿名，其余 Sailflow API 需登录。</summary>
[AllowAnonymous]
[Route("api/sail/[controller]")]
[ApiController]
public class ProbeController : ControllerBase
{
    private readonly SailClient _client;

    public ProbeController(SailClient client)
    {
        _client = client;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        try
        {
            var db = _client.GetDB();
            var table = db.ExeQuery("select 1 as n");
            var one = table.Rows.Count > 0 ? Convert.ToInt32(table.Rows[0][0]) : -1;
            return Ok(new
            {
                ok = true,
                scalar = one,
                position = SailflowDefaults.DbPosition,
                dialect = db.dialect?.GetType().Name,
                dbName = db.config?.name
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    /// <summary>POST/GET api/sail/Probe/init-db?seed=true</summary>
    [HttpPost("init-db")]
    [HttpGet("init-db")]
    public IActionResult InitDb([FromQuery] bool seed = true)
    {
        try
        {
            // 建表仅在宿主业务侧（不在 sailflow 包内）
            var result = SailHostSchemaInitializer.InitDb(_client, seed);
            return Ok(new
            {
                ok = true,
                seededRows = result.SeededRows,
                progress = result.Progress,
                position = SailflowDefaults.DbPosition
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }
}
