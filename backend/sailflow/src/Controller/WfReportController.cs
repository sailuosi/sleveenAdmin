using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Saiflow.Dto;
using SleveenAdmin.Saiflow.Service;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>流程报表（只读聚合）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfReportController : ControllerBase
{
    private readonly SailReportService _report;

    public WfReportController(SailReportService report) => _report = report;

    /// <summary>GET api/WfReport/Overview?top=10</summary>
    [HttpGet]
    public ResponseResult<WorkflowReportOverviewDto> Overview([FromQuery] int top = 10)
    {
        try
        {
            return ResponseResult<WorkflowReportOverviewDto>.Success(_report.GetOverview(top));
        }
        catch (Exception ex)
        {
            return ResponseResult<WorkflowReportOverviewDto>.Error(ex.Message);
        }
    }
}
