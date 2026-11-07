using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mooSQL.data;
using saiflow.biz.Bootstrap;
using saiflow.data;
using saiflow.engine.Client;
using SleveenAdmin.Saiflow.Entity;
using SleveenAdmin.Saiflow.Hosting;
using SleveenAdmin.Saiflow.Integration.UnifiedReport;
using SleveenAdmin.Saiflow.Service;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>探活 / InitDb；路由 api/sail/Probe。故意匿名，其余 Saiflow API 需登录。</summary>
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
                position = SaiflowDefaults.DbPosition,
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
            // 含 FbForm / FbFormFieldActivityEdit（见 SailSchemaInitializer）
            var result = SailSchemaInitializer.InitDb(_client, seed);
            // 请假样例 / 流程分类 / 流程编辑元数据 / 集成平台推送流水
            new SailLeaveService(_client).EnsureTable();
            new SailTodoPortalRecordService(_client).EnsureTable();
            _client.UseRunSQL().useDBInit().autoMode().createTable<SailProcessCategoryEntity>();
            _client.UseRunSQL().useDBInit().autoMode().createTable<SailProcessMetaEntity>();
            return Ok(new
            {
                ok = true,
                seededRows = result.SeededRows,
                progress = result.Progress,
                position = SaiflowDefaults.DbPosition
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }
}
