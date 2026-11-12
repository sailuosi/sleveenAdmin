using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.biz.query;
using sailflow.engine.Client;
using sailflow.engine.Entity;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>
/// Sail 办理台只读列表（待办/已办/流程实例/抄送）。无增删改。
/// </summary>
[Authorize]
[ApiController]
[ApiResult]
[Route("biz/sailworkbench")]
public class BizSailWorkbenchListController : ControllerBase
{
    private readonly WorkflowQueryService _query;
    private readonly SailCopyService _copy;

    public BizSailWorkbenchListController(WorkflowQueryService query, SailClient sail)
    {
        _query = query;
        _copy = new SailCopyService(sail);
    }

    /// <summary>待办 Ready：mine=true 为我的待办；mine=false 为任务管理（全量）。</summary>
    [HttpPost("todo/qlist")]
    public IActionResult TodoQList([FromBody] SailWorkbenchQueryInput? input)
    {
        input ??= new SailWorkbenchQueryInput();
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var query = new TaskQuery
        {
            AppName = string.IsNullOrWhiteSpace(input.Keyword) ? null : input.Keyword.Trim(),
            PageIndex = page,
            PageSize = size
        };
        if (input.Mine)
            query.UserId = SailAuthIdentity.RequireAccount(User);
        SailAuthIdentity.BindTaskQueryMine(User, query);

        var (items, total) = _query.QueryReadyTasksPaged(query);
        return Ok(PagedResult<TaskViewEntity>.Create(items.ToList(), page, size, total));
    }

    /// <summary>我的已办。</summary>
    [HttpPost("done/qlist")]
    public IActionResult DoneQList([FromBody] SailWorkbenchQueryInput? input)
    {
        input ??= new SailWorkbenchQueryInput();
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var query = new TaskQuery
        {
            AppName = string.IsNullOrWhiteSpace(input.Keyword) ? null : input.Keyword.Trim(),
            EndedUserId = SailAuthIdentity.RequireAccount(User),
            PageIndex = page,
            PageSize = size
        };
        SailAuthIdentity.BindTaskQueryMine(User, query);

        var (items, total) = _query.QueryCompletedTasksPaged(query);
        return Ok(PagedResult<TaskViewEntity>.Create(items.ToList(), page, size, total));
    }

    /// <summary>流程实例：mine=true 为我的流程；mine=false 为实例管理。</summary>
    [HttpPost("process/qlist")]
    public IActionResult ProcessQList([FromBody] SailWorkbenchQueryInput? input)
    {
        input ??= new SailWorkbenchQueryInput();
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var query = new ProcessInstanceQuery
        {
            AppName = string.IsNullOrWhiteSpace(input.Keyword) ? null : input.Keyword.Trim(),
            ProcessState = input.ProcessState,
            PageIndex = page,
            PageSize = size
        };
        if (input.Mine)
            query.CreatedUserId = SailAuthIdentity.RequireAccount(User);
        SailAuthIdentity.BindProcessInstanceMine(User, query);

        var (items, total) = _query.QueryProcessInstancesPaged(query);
        return Ok(PagedResult<ProcessInstanceEntity>.Create(items.ToList(), page, size, total));
    }

    /// <summary>抄送我的。</summary>
    [HttpPost("copy/qlist")]
    public IActionResult CopyQList([FromBody] SailWorkbenchQueryInput? input)
    {
        input ??= new SailWorkbenchQueryInput();
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var userId = SailAuthIdentity.RequireAccount(User);
        var (items, total) = _copy.QueryMyPage(userId, page, size);
        return Ok(PagedResult<SailProcessCopyEntity>.Create(items.ToList(), page, size, total));
    }
}

/// <summary>办理台列表查询（Biz 分页 + Sail 过滤）。不继承 sealed BizQueryInput。</summary>
public sealed class SailWorkbenchQueryInput
{
    public int Page { get; set; } = 1;
    public int PageNum { get; set; }
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }

    /// <summary>true=当前登录人视角；false=管理端全量（待办/流程实例）。</summary>
    public bool Mine { get; set; }

    /// <summary>流程实例状态过滤（可选）。</summary>
    public short? ProcessState { get; set; }

    public int ResolvePage()
    {
        if (PageNum > 0) return PageNum;
        return Page <= 0 ? 1 : Page;
    }

    public int ResolvePageSize() => PageSize <= 0 ? 20 : PageSize;
}
