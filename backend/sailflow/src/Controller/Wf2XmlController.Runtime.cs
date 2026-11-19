using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mooSQL.data;
using sailflow.biz.query;
using sailflow.engine.Entity;
using sailflow.engine.Manager;
using sailflow.engine.Common;
using sailflow.engine.Result;
using sailflow.hub.Form;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Assignee;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Operations;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>任务 2 T2-4：运行态流转 + 设计器查询（契约对齐 sfdapi Wf2Xml 执行区）。</summary>
public partial class Wf2XmlController
{
    /// <summary>POST api/Wf2Xml/StartProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> StartProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            var exec = await _workflowService.StartProcessAsync(runner, HttpContext.RequestAborted);
            PersistRunnerConditions(runner, exec);
            // 预选办理人：独立 api/WfPreselect，由发起页组件在 Start 成功后写入
            return FromExecuted(exec);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/RunProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> RunProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            EnsureRunnerActivityInstanceId(runner);
            var exec = await _workflowService.RunProcessAsync(runner, HttpContext.RequestAborted);
            PersistRunnerConditions(runner, exec);
            return FromExecuted(exec);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/SignForwardProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> SignForwardProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.SignForwardProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/TransferProcess · 转办（本节点交人，不流转）</summary>
    [HttpPost]
    public async Task<ResponseResult> TransferProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.TransferProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/DelegateProcess · 委派（办完自动回转原人）</summary>
    [HttpPost]
    public async Task<ResponseResult> DelegateProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.DelegateProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/RemoveAssigneeProcess · 减签</summary>
    [HttpPost]
    public async Task<ResponseResult> RemoveAssigneeProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.RemoveAssigneeProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/ChangeAssigneeProcess · 改派（新全集）</summary>
    [HttpPost]
    public async Task<ResponseResult> ChangeAssigneeProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.ChangeAssigneeProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/UrgeProcess · 催办</summary>
    [HttpPost]
    public async Task<ResponseResult> UrgeProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.UrgeProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/ButtonsOperation · 统一办理（码表 Id → Handler）</summary>
    [HttpPost]
    public async Task<ResponseResult> ButtonsOperation([FromBody] SailButtonsOperationRequest req)
    {
        try
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ButtonId))
                return ResponseResult.Error("ButtonId is required.");
            if (req.Runner == null)
                return ResponseResult.Error("Runner is required.");

            SailAuthIdentity.BindRunner(User, req.Runner);
            EnsureRunnerActivityInstanceId(req.Runner);
            var result = await _operationDispatcher.ExecuteAsync(new SailOperationContext
            {
                ButtonId = req.ButtonId.Trim(),
                Runner = req.Runner
            });
            PersistRunnerConditions(req.Runner, result);
            return FromExecuted(result);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/ProbeOperationHandlers · 码表 Id × 是否已挂 Handler</summary>
    [HttpGet]
    public ResponseResult ProbeOperationHandlers()
    {
        try
        {
            var ids = SailButtonsCatalog.Defaults.Select(d => d.Id).ToList();
            var result = ResponseResult.Success("ok");
            result.ExtraData = _operationDispatcher.Probe(ids);
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/ProbeInstalledFeatures · SailClient Build 已装 UseXxx 能力名</summary>
    [HttpGet]
    public ResponseResult ProbeInstalledFeatures()
    {
        try
        {
            var result = ResponseResult.Success("ok");
            result.ExtraData = _sail.InstalledFeatures
                .Select(f => new
                {
                    f.Name,
                    Phase = f.Phase.ToString(),
                    f.Order,
                    DependsOn = f.DependsOn,
                })
                .ToList();
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/JumpProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> JumpProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.JumpProcessAsync(runner, cancellationToken: HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/ReviseProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> ReviseProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.ReviseProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/SendBackProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> SendBackProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.SendBackProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/WithdrawProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> WithdrawProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            var gated = SailExtraOperationGate.TryReject("Withdraw", runner, _workflowService, _processExtra);
            if (gated != null)
                return FromExecuted(gated);
            return FromExecuted(await _workflowService.WithdrawProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/ResendProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> ResendProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.ResendProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/RejectProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> RejectProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.RejectProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/CloseProcess</summary>
    [HttpPost]
    public async Task<ResponseResult> CloseProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            var gated = SailExtraOperationGate.TryReject("Close", runner, _workflowService, _processExtra);
            if (gated != null)
                return FromExecuted(gated);
            return FromExecuted(await _workflowService.CloseProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/TerminateProcess · §B 终止运行实例</summary>
    [HttpPost]
    public ResponseResult TerminateProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(_workflowService.TerminateProcess(runner));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/DiscardProcess · §B 作废实例</summary>
    [HttpPost]
    public ResponseResult DiscardProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(_workflowService.DiscardProcess(runner));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/ReverseProcess · 反转（与退回语义不同）</summary>
    [HttpPost]
    public async Task<ResponseResult> ReverseProcess([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            return FromExecuted(await _workflowService.ReverseProcessAsync(runner, HttpContext.RequestAborted));
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/DeleteInstance</summary>
    [HttpPost]
    public ResponseResult DeleteInstance([FromBody] ProcessQuery query)
    {
        try
        {
            var ok = _workflowService.DeleteProcessInstances(query.ProcessId, query.Version);
            return ok
                ? ResponseResult.Success("DeleteInstance ok")
                : ResponseResult.Error("DeleteInstance failed");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryReadyActivityInstance</summary>
    [HttpPost]
    public ResponseResult<List<ActivityInstanceEntity>> QueryReadyActivityInstance([FromBody] TaskQuery query)
    {
        try
        {
            var list = _queryService.GetRunningActivityInstance(query).ToList();
            return ResponseResult<List<ActivityInstanceEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<ActivityInstanceEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryCompletedTransitionInstance</summary>
    [HttpPost]
    public ResponseResult<List<TransitionImage>> QueryCompletedTransitionInstance(
        [FromBody] TransitionInstanceQuery query)
    {
        try
        {
            var list = _queryService.GetTransitionInstanceList(query).ToList();
            return ResponseResult<List<TransitionImage>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<TransitionImage>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryCompletedTasks</summary>
    [HttpPost]
    public ResponseResult<List<TaskViewEntity>> QueryCompletedTasks([FromBody] TaskQuery query)
    {
        try
        {
            query ??= new TaskQuery();
            SailAuthIdentity.BindTaskQueryMine(User, query);
            var (items, total) = _queryService.QueryCompletedTasksPaged(query);
            var result = ResponseResult<List<TaskViewEntity>>.Success(items.ToList());
            result.TotalRowsCount = total;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<TaskViewEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryReadyTasks · 分页待办（UserId 过滤）</summary>
    [HttpPost]
    public ResponseResult<List<TaskViewEntity>> QueryReadyTasks([FromBody] TaskQuery query)
    {
        try
        {
            query ??= new TaskQuery();
            SailAuthIdentity.BindTaskQueryMine(User, query);
            var (items, total) = _queryService.QueryReadyTasksPaged(query);
            var result = ResponseResult<List<TaskViewEntity>>.Success(items.ToList());
            result.TotalRowsCount = total;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<TaskViewEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/GetTaskView/{id}</summary>
    [HttpGet]
    public ResponseResult<TaskViewEntity> GetTaskView(string id)
    {
        try
        {
            var entity = _workflowService.GetTaskView(id);
            return entity == null
                ? ResponseResult<TaskViewEntity>.Error("task not found")
                : ResponseResult<TaskViewEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ResponseResult<TaskViewEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetNextActivityRoleUserTree</summary>
    [HttpPost]
    public ResponseResult<List<NextActivityNodeDto>> GetNextActivityRoleUserTree([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            var list = _workflowService.GetNextActivityRoleUserTree(runner, runner?.Conditions)?.ToList()
                       ?? new List<NodeView>();
            // R3：对齐 Account；Users≤100；空名单 → anyselect（不再全表回填）
            var dtoList = list
                .Select(n => SleveenUserIdentity.EnrichNextActivityNode(NextActivityNodeDto.FromNodeView(n)))
                .ToList();
            ApplyCandidateSelectorsFromProcess(runner?.ProcessId, runner?.Version, dtoList);
            return ResponseResult<List<NextActivityNodeDto>>.Success(dtoList);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<NextActivityNodeDto>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryNextActivityCandidates · 选人分页（与下一节点树拆分）</summary>
    [HttpPost]
    public ResponseResult<QueryNextActivityCandidatesResult> QueryNextActivityCandidates(
        [FromBody] QueryNextActivityCandidatesRequest req)
    {
        try
        {
            req ??= new QueryNextActivityCandidatesRequest();
            var page = req.Page < 1 ? 1 : req.Page;
            var pageSize = req.PageSize < 1 ? 100 : Math.Min(req.PageSize, SleveenUserIdentity.MaxInlineCandidateUsers);
            var mode = (req.CandidateMode ?? "").Trim().ToLowerInvariant();
            var selector = (req.CandidateSelectorType ?? "").Trim();
            var roleIds = req.RoleIds ?? Array.Empty<string>();

            List<sailflow.hub.Resource.User> users;
            int total;
            var useRole = string.Equals(selector, "RoleUserSearch", StringComparison.OrdinalIgnoreCase)
                          || mode == "search"
                          || (mode == "bound" && roleIds.Length > 0 && string.Equals(selector, "RoleUserSearch", StringComparison.OrdinalIgnoreCase));
            if (useRole && roleIds.Length > 0)
                (users, total) = SleveenUserIdentity.QueryRoleUsers(roleIds, req.Keyword, page, pageSize);
            else
                (users, total) = SleveenUserIdentity.QueryHostUsers(req.Keyword, page, pageSize);

            return ResponseResult<QueryNextActivityCandidatesResult>.Success(new QueryNextActivityCandidatesResult
            {
                Users = users,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return ResponseResult<QueryNextActivityCandidatesResult>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetPreviousActivityTree · 退回候选节点</summary>
    [HttpPost]
    public ResponseResult<List<NodeView>> GetPreviousActivityTree([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            var list = _workflowService.GetPreviousActivityTree(runner)?.ToList() ?? new List<NodeView>();
            return ResponseResult<List<NodeView>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<NodeView>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetTaskActivityList · 跳转候选人工任务节点</summary>
    [HttpPost]
    public ResponseResult<object> GetTaskActivityList([FromBody] ProcessQuery query)
    {
        try
        {
            if (query == null || string.IsNullOrWhiteSpace(query.ProcessId))
                return ResponseResult<object>.Error("ProcessId required");
            var version = string.IsNullOrWhiteSpace(query.Version) ? "1" : query.Version;
            var list = _workflowService.GetTaskActivityList(query.ProcessId, version)
                ?.Select(a => new
                {
                    ActivityId = a.ActivityId,
                    ActivityName = a.ActivityName,
                    ActivityCode = a.ActivityCode,
                    ActivityType = a.ActivityType
                })
                .ToList() ?? [];
            return ResponseResult<object>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetApprovalWorkbench · 办理详情聚合</summary>
    [HttpPost]
    public ResponseResult<ApprovalWorkbenchDto> GetApprovalWorkbench([FromBody] ApprovalWorkbenchQuery query)
    {
        try
        {
            query ??= new ApprovalWorkbenchQuery();
            var dto = new ApprovalWorkbenchDto();

            TaskViewEntity? task = null;
            if (!string.IsNullOrWhiteSpace(query.TaskId))
                task = _workflowService.GetTaskView(query.TaskId);
            dto.Task = task;

            var processInstanceId = !string.IsNullOrWhiteSpace(query.ProcessInstanceId)
                ? query.ProcessInstanceId.Trim()
                : task?.ProcessInstanceId;
            if (!string.IsNullOrWhiteSpace(processInstanceId))
            {
                dto.ProcessInstance = _workflowService.GetProcessInstance(processInstanceId);
                dto.ActivityInstances = _workflowService.GetActivityInstanceList(processInstanceId)?.ToList()
                                        ?? new List<ActivityInstanceEntity>();
            }

            if (task != null)
            {
                try
                {
                    dto.IsLastTask = _queryService.IsLastTask(task.TaskId);
                }
                catch
                {
                    dto.IsLastTask = true;
                }

                var runner = new FlowCommandContext
                {
                    TaskId = task.TaskId,
                    BizPK = task.BizPK,
                    ProcessId = task.ProcessId,
                    Version = task.Version,
                    UserId = task.SignId,
                    UserName = task.SignName,
                    BizChannel = task.BizChannel,
                    Conditions = query.Conditions ?? new Dictionary<string, string>()
                };
                try
                {
                    var rawNext = _workflowService.GetNextActivityRoleUserTree(runner, runner.Conditions)?.ToList()
                                  ?? new List<NodeView>();
                    dto.NextNodes = rawNext
                        .Select(n => SleveenUserIdentity.EnrichNextActivityNode(NextActivityNodeDto.FromNodeView(n)))
                        .ToList();
                    ApplyCandidateSelectorsFromProcess(task.ProcessId, task.Version, dto.NextNodes);
                }
                catch
                {
                    dto.NextNodes = new List<NextActivityNodeDto>();
                }

                try
                {
                    dto.PreviousNodes = _workflowService.GetPreviousActivityTree(runner)?.ToList()
                                        ?? new List<NodeView>();
                }
                catch
                {
                    dto.PreviousNodes = new List<NodeView>();
                }

                dto.ReadyActivities = _queryService.GetRunningActivityInstance(new TaskQuery
                {
                    BizPK = task.BizPK,
                    ProcessId = task.ProcessId,
                    Version = task.Version
                })?.ToList() ?? new List<ActivityInstanceEntity>();

                // §G：退回源 — 被退回待办可「返送退回人」
                try
                {
                    ActivityInstanceEntity? ai = null;
                    if (!string.IsNullOrWhiteSpace(task.ActivityInstanceId))
                    {
                        ai = dto.ActivityInstances.FirstOrDefault(a => a.ActivityInstanceId == task.ActivityInstanceId)
                             ?? _workflowService.GetActivityInstance(task.ActivityInstanceId);
                    }
                    if (!string.IsNullOrWhiteSpace(ai?.BackSrcActInstanceId))
                    {
                        dto.HasBackSrc = true;
                        dto.BackSrcActInstanceId = ai.BackSrcActInstanceId;
                        // 退回人：BackSrc 活动办结人（EndedUser*），禁止用 CreatedUser*
                        var backSrc = dto.ActivityInstances.FirstOrDefault(a =>
                                          a.ActivityInstanceId == ai.BackSrcActInstanceId)
                                      ?? _workflowService.GetActivityInstance(ai.BackSrcActInstanceId);
                        if (backSrc != null)
                        {
                            dto.BackSrcUserId = string.IsNullOrWhiteSpace(backSrc.EndedUserId)
                                ? null
                                : backSrc.EndedUserId;
                            dto.BackSrcUserName = string.IsNullOrWhiteSpace(backSrc.EndedUserName)
                                ? null
                                : backSrc.EndedUserName;
                        }
                    }
                }
                catch
                {
                    /* 退回源探测失败不阻断办理 */
                }

                // 减签/改派/催办候选人：同 ActivityInstance 未办结 Waiting
                try
                {
                    var open = BuildOpenAssignees(task);
                    dto.CurrentAssignees = open;
                    dto.RemovableAssignees = open
                        .Where(a => !string.Equals(a.UserId, task.SignId, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                catch
                {
                    dto.CurrentAssignees = new List<RemovableAssigneeDto>();
                    dto.RemovableAssignees = new List<RemovableAssigneeDto>();
                }

                // 节点办理按钮 + NodePolicy（§J 两维）
                try
                {
                    var file = _workflowService.GetProcessFile(task.ProcessId, task.Version);
                    dto.ButtonsSetting = SailButtonsCatalog.ParseForActivity(
                        file?.XmlContent,
                        task.ActivityId);
                    var policy = SailNodePolicyCatalog.ParseForActivity(
                        file?.XmlContent,
                        task.ActivityId);
                    dto.NodePolicy = policy;
                    dto.SendBackTarget = policy.SendBackTarget;
                    dto.ResubmitMode = policy.ResubmitMode;
                    dto.PreviousNodes = SailNodePolicyCatalog.FilterPreviousBySendBackTarget(
                        dto.PreviousNodes,
                        policy.SendBackTarget);
                }
                catch
                {
                    dto.ButtonsSetting = SailButtonsCatalog.Merge(null);
                    var fallback = SailNodePolicyCatalog.Merge(null);
                    dto.NodePolicy = fallback;
                    dto.SendBackTarget = fallback.SendBackTarget;
                    dto.ResubmitMode = fallback.ResubmitMode;
                }

                ApplyConsultWorkbenchState(dto, task);
            }

            // ExtraJson 运行时：闸控 / 打印 / 摘要；可覆盖退回两维
            ApplyExtraWorkbenchState(dto, task);

            // 表单：显式 FormId → 实例 BizCode（发起写入的 formId，Guid/数字均可）
            // → 流程 meta.FormId 回落。fm_form.ff_id 已是 Guid，禁止再用 int.TryParse 拦截。
            var formId = ResolveWorkbenchFormId(query.FormId, dto, task);

            IList<FieldActivityEdit>? formEdits = null;
            if (!string.IsNullOrWhiteSpace(formId))
            {
                try
                {
                    var formSvc = FormServiceFactory.Create(_sail);
                    var form = formSvc.GetForm(formId);
                    if (form != null)
                    {
                        dto.FormConf = new
                        {
                            formId = form.FormId,
                            formName = form.FormName,
                            conf = form.Conf,
                            fieldsJson = form.FieldsJson
                        };
                        if (task != null)
                        {
                            try
                            {
                                formEdits = formSvc.GetFieldActivityEditByForm(
                                    formId,
                                    new FieldQuery
                                    {
                                        FormId = formId,
                                        ProcessId = task.ProcessId,
                                        ProcessVersion = task.Version,
                                        ActivityId = task.ActivityId
                                    });
                            }
                            catch
                            {
                                /* 权限表缺失不阻断 */
                            }
                        }
                    }
                }
                catch
                {
                    /* 表单缺失不阻断办理 */
                }
            }

            // §C：NodePolicy.fieldsPermission 优先，否则 Form 活动权限表
            var modeItems = SailFieldModes.Resolve(
                dto.NodePolicy?.FieldsPermission,
                formEdits);
            dto.FieldPermissions = modeItems;
            dto.FieldModes = SailFieldModes.ToMap(modeItems);

            // 回填发起/办理写入的流程变量 → 动态表单值
            if (!string.IsNullOrWhiteSpace(processInstanceId))
            {
                dto.FormValues = SailProcessVariableHelper.LoadFormValues(
                    _workflowService, processInstanceId, _logger);
                dto.ActivityOpinions = SailProcessVariableHelper.LoadActivityOpinions(dto.FormValues);
                SailProcessVariableHelper.AttachGlobalOpinionFallback(
                    dto.ActivityOpinions, dto.FormValues, dto.ActivityInstances);
                if (dto.FormValues.TryGetValue(SailProcessExtraTemplate.SummaryVar, out var summary)
                    && !string.IsNullOrWhiteSpace(summary))
                    dto.Summary = summary;
            }

            return ResponseResult<ApprovalWorkbenchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ResponseResult<ApprovalWorkbenchDto>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryProcessInstances · 我的/管理流程实例</summary>
    [HttpPost]
    public ResponseResult<List<ProcessInstanceEntity>> QueryProcessInstances([FromBody] ProcessInstanceQuery query)
    {
        try
        {
            query ??= new ProcessInstanceQuery();
            SailAuthIdentity.BindProcessInstanceMine(User, query);
            var (items, total) = _queryService.QueryProcessInstancesPaged(query);
            var result = ResponseResult<List<ProcessInstanceEntity>>.Success(items.ToList());
            result.TotalRowsCount = total;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<ProcessInstanceEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/GetProcessInstance/{id}</summary>
    [HttpGet]
    public ResponseResult<ProcessInstanceEntity> GetProcessInstance(string id)
    {
        try
        {
            var entity = _workflowService.GetProcessInstance(id);
            return entity == null
                ? ResponseResult<ProcessInstanceEntity>.Error("process instance not found")
                : ResponseResult<ProcessInstanceEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessInstanceEntity>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/GetTaskToDoListTop</summary>
    [HttpGet]
    public ResponseResult<List<TaskViewEntity>> GetTaskToDoListTop()
    {
        try
        {
            var list = _queryService.GetTaskToDoListTop();
            return ResponseResult<List<TaskViewEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<TaskViewEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/GetTaskDoneListTop</summary>
    [HttpGet]
    public ResponseResult<List<TaskViewEntity>> GetTaskDoneListTop()
    {
        try
        {
            var list = _queryService.GetTaskDoneListTop();
            return ResponseResult<List<TaskViewEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<TaskViewEntity>>.Error(ex.Message);
        }
    }

    /// <summary>
    /// 办理表单 Id：query → BizCode（Guid/旧数字）→ sf_process_meta.FormId。
    /// </summary>
    private string? ResolveWorkbenchFormId(
        string? queryFormId,
        ApprovalWorkbenchDto dto,
        TaskViewEntity? task)
    {
        if (!string.IsNullOrWhiteSpace(queryFormId))
            return queryFormId.Trim();

        var code = dto.ProcessInstance?.BizCode?.Trim();
        if (!string.IsNullOrWhiteSpace(code))
            return code;

        var processId = task?.ProcessId
                        ?? dto.ProcessInstance?.ProcessId;
        var version = task?.Version
                      ?? dto.ProcessInstance?.Version
                      ?? "1";
        if (string.IsNullOrWhiteSpace(processId))
            return null;

        try
        {
            var metaFormId = _processMeta.GetMeta(processId, version)?.FormId?.Trim();
            return string.IsNullOrWhiteSpace(metaFormId) ? null : metaFormId;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>从流程 BPMN 读取 sf:CandidateSelector，覆盖节点选人器元数据。</summary>
    private void ApplyCandidateSelectorsFromProcess(
        string? processId,
        string? version,
        List<NextActivityNodeDto> nodes)
    {
        if (nodes == null || nodes.Count == 0 || string.IsNullOrWhiteSpace(processId))
            return;
        try
        {
            var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
            var file = _workflowService.GetProcessFile(processId, ver);
            var xml = file?.XmlContent;
            if (string.IsNullOrWhiteSpace(xml)) return;
            foreach (var n in nodes)
            {
                if (string.IsNullOrWhiteSpace(n.ActivityId)) continue;
                var (type, parms) = SailCandidateSelectorCatalog.ReadFromXml(xml, n.ActivityId);
                if (type != null || parms != null)
                    SleveenUserIdentity.ApplyCandidateSelectorOverride(n, type, parms);
            }
        }
        catch
        {
            /* 无文件或解析失败时保留推断默认 */
        }
    }

    /// <summary>Start/Run/办理成功后：把 Conditions 落成流程变量（表单回填依赖）。</summary>
    private void PersistRunnerConditions(FlowCommandContext? runner, WfExecutedResult? exec)
    {
        if (exec == null || exec.Status != WfExecutedStatus.Success)
            return;
        EnsureRunnerActivityInstanceId(runner);
        var piId = SailProcessVariableHelper.ResolveProcessInstanceId(
            _workflowService,
            runner,
            exec.ProcessInstanceIdStarted);
        SailProcessVariableHelper.PersistConditions(_workflowService, runner, piId, _logger);
    }

    /// <summary>写意见 act 键前补齐 ActivityInstanceId（优先 Runner，否则办结前 TaskView）。</summary>
    void EnsureRunnerActivityInstanceId(FlowCommandContext? runner)
    {
        if (runner == null || !string.IsNullOrWhiteSpace(runner.ActivityInstanceId))
            return;
        if (string.IsNullOrWhiteSpace(runner.TaskId))
            return;
        try
        {
            var actId = _workflowService.GetTaskView(runner.TaskId)?.ActivityInstanceId;
            if (!string.IsNullOrWhiteSpace(actId))
                runner.ActivityInstanceId = actId.Trim();
        }
        catch
        {
            /* ignore */
        }
    }

    List<RemovableAssigneeDto> BuildOpenAssignees(TaskViewEntity task)
    {
        var list = new List<RemovableAssigneeDto>();
        if (string.IsNullOrWhiteSpace(task.ActivityInstanceId) || string.IsNullOrWhiteSpace(task.ProcessInstanceId))
            return list;

        var clip = _sail.UseRunSQL().useClip();
        clip.from<TaskEntity>(out var t)
            .where(() => t.ProcessInstanceId, task.ProcessInstanceId)
            .where(() => t.ActivityInstanceId, task.ActivityInstanceId)
            .where(() => t.TaskState, (short)TaskStateEnum.Waiting);
        var waiting = clip.select(t).queryList() ?? new List<TaskEntity>();
        foreach (var row in waiting)
        {
            if (string.IsNullOrWhiteSpace(row.TaskId) || string.IsNullOrWhiteSpace(row.SignId))
                continue;
            list.Add(new RemovableAssigneeDto
            {
                TaskId = row.TaskId,
                UserId = row.SignId,
                UserName = row.SignName ?? row.SignId
            });
        }
        return list;
    }

    void ApplyExtraWorkbenchState(ApprovalWorkbenchDto dto, TaskViewEntity? task)
    {
        try
        {
            var processId = task?.ProcessId
                            ?? dto.ProcessInstance?.ProcessId;
            var version = task?.Version
                          ?? dto.ProcessInstance?.Version
                          ?? "1";
            if (string.IsNullOrWhiteSpace(processId))
                return;

            var extra = _processExtra.Get(processId, version);
            dto.Extra = new SailProcessExtraRuntimeDto
            {
                AllowCancelRunningProcess = extra.AllowCancelRunningProcess,
                AllowWithdrawTask = extra.AllowWithdrawTask,
                AutoApprovalType = extra.AutoApprovalType,
                PrintTemplateEnable = extra.PrintTemplateSetting.Enable,
                PrintTemplate = extra.PrintTemplateSetting.Enable
                    ? extra.PrintTemplateSetting.Template
                    : null,
                SendBackTarget = string.IsNullOrWhiteSpace(extra.SendBackTarget)
                    ? null
                    : SailNodePolicyCatalog.NormalizeTarget(extra.SendBackTarget),
                ResubmitMode = string.IsNullOrWhiteSpace(extra.ResubmitMode)
                    ? null
                    : SailNodePolicyCatalog.NormalizeResubmit(extra.ResubmitMode)
            };

            if (!extra.AllowWithdrawTask && dto.ButtonsSetting != null)
            {
                foreach (var b in dto.ButtonsSetting)
                {
                    if (string.Equals(b.Id, "Withdraw", StringComparison.OrdinalIgnoreCase))
                        b.Enable = false;
                }
            }

            // 流程级退回默认：Extra 非空覆盖节点；再按最终维①收窄 PreviousNodes
            var resolved = SailSendBackPolicy.Resolve(
                dto.SendBackTarget,
                dto.ResubmitMode,
                extra.SendBackTarget,
                extra.ResubmitMode);
            dto.SendBackTarget = resolved.SendBackTarget;
            dto.ResubmitMode = resolved.ResubmitMode;
            dto.PreviousNodes = SailNodePolicyCatalog.FilterPreviousBySendBackTarget(
                dto.PreviousNodes,
                dto.SendBackTarget);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ApplyExtraWorkbenchState failed");
        }
    }

    /// <summary>
    /// 沟通：下发 AssigneeRole / ConsultPending，并按角色裁剪 ButtonsSetting。
    /// 参与者仅留 Acknowledge；常规办理人待反馈时隐藏推进类按钮，保留沟通/收回/抄送。
    /// </summary>
    void ApplyConsultWorkbenchState(ApprovalWorkbenchDto dto, TaskViewEntity task)
    {
        dto.AssigneeRole = task.AssigneeRole;
        var isParticipant = task.AssigneeRole == (int)AssigneeRoleEnum.Participant;

        if (isParticipant)
        {
            dto.ConsultPending = false;
            dto.OpenParticipantCount = 0;
            dto.ButtonsSetting = (dto.ButtonsSetting ?? SailButtonsCatalog.Merge(null))
                .Where(b => string.Equals(b.Id, "Acknowledge", StringComparison.OrdinalIgnoreCase))
                .Select(b =>
                {
                    b.Enable = true;
                    return b;
                })
                .ToList();
            if (dto.ButtonsSetting.Count == 0)
            {
                dto.ButtonsSetting =
                [
                    new SailButtonSettingDto
                    {
                        Id = "Acknowledge",
                        DisplayName = "阅办",
                        Enable = true,
                        Engine = "AcknowledgeProcess"
                    }
                ];
            }
            return;
        }

        try
        {
            using var kit = _sail.UseRunSQL();
            var open = _sail.ManagerFactory.Tasks.GetOpenParticipantsBySrc(kit, task.TaskId);
            dto.OpenParticipantCount = open?.Count ?? 0;
            dto.ConsultPending = dto.OpenParticipantCount > 0;
        }
        catch
        {
            dto.ConsultPending = false;
            dto.OpenParticipantCount = 0;
        }

        if (!dto.ConsultPending)
            return;

        // 锁定推进类；保留沟通 / 收回 / 抄送
        var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Consult", "RecallConsult", "Copyto"
        };
        foreach (var b in dto.ButtonsSetting ?? [])
        {
            if (keep.Contains(b.Id))
            {
                b.Enable = true;
                continue;
            }
            // 推进/改责任类：显示但禁用由前端处理；此处直接关掉 enable 更稳
            if (IsConsultLockedButton(b.Id))
                b.Enable = false;
        }
    }

    static bool IsConsultLockedButton(string? id) =>
        id is "Approve" or "Reject" or "SendBack" or "SignForward" or "RemoveAssignee"
            or "ChangeAssignee" or "Urge" or "Transfer" or "Delegate"
            or "Revise" or "Resend" or "Jump" or "Close";

    private static ResponseResult FromExecuted(WfExecutedResult wfResult)
    {
        if (wfResult.Status != WfExecutedStatus.Success)
        {
            var fail = ResponseResult.Error(wfResult.Message ?? "workflow execution failed");
            fail.ExtraData = new
            {
                reasonCode = wfResult.ReasonCode,
                exceptionType = wfResult.ExceptionType,
                validationCode = wfResult.ValidationCode,
                aiResponse = wfResult.AiResponse
            };
            return fail;
        }

        var result = ResponseResult.Success(wfResult.Message);
        if (!string.IsNullOrWhiteSpace(wfResult.ProcessInstanceIdStarted))
            result.NewId = wfResult.ProcessInstanceIdStarted;
        result.ExtraData = new
        {
            processInstanceId = wfResult.ProcessInstanceIdStarted,
            aiResponse = wfResult.AiResponse,
            validationCode = wfResult.ValidationCode,
            reasonCode = wfResult.ReasonCode
        };
        return result;
    }
}
