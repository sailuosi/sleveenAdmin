using System.Linq;
using Microsoft.AspNetCore.Mvc;
using saiflow.biz.query;
using saiflow.engine.Entity;
using saiflow.engine.Manager;
using saiflow.engine.Common;
using saiflow.engine.Result;
using saiflow.hub.Form;
using saiflow.WebUtility;
using SleveenAdmin.Saiflow.Assignee;
using SleveenAdmin.Saiflow.Dto;
using SleveenAdmin.Saiflow.Operations;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Controller;

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
            var ok = _workflowService.TerminateProcess(runner);
            return ok
                ? ResponseResult.Success("TerminateProcess ok")
                : ResponseResult.Error("TerminateProcess failed");
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
            var ok = _workflowService.DiscardProcess(runner);
            return ok
                ? ResponseResult.Success("DiscardProcess ok")
                : ResponseResult.Error("DiscardProcess failed");
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
    public ResponseResult<List<NodeView>> GetNextActivityRoleUserTree([FromBody] FlowCommandContext runner)
    {
        try
        {
            SailAuthIdentity.BindRunner(User, runner);
            var list = _workflowService.GetNextActivityRoleUserTree(runner, runner?.Conditions)?.ToList()
                       ?? new List<NodeView>();
            // R3：候选用户对齐 Account；ApproverPick/角色空展开时补宿主候选人
            foreach (var node in list)
                SleveenUserIdentity.EnrichNodeCandidateUsers(node);
            return ResponseResult<List<NodeView>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<NodeView>>.Error(ex.Message);
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
                var runner = new FlowCommandContext
                {
                    TaskId = task.TaskId,
                    AppInstanceId = task.AppInstanceId,
                    ProcessId = task.ProcessId,
                    Version = task.Version,
                    UserId = task.AssignedUserId,
                    UserName = task.AssignedUserName,
                    AppName = task.AppName,
                    Conditions = query.Conditions ?? new Dictionary<string, string>()
                };
                try
                {
                    dto.NextNodes = _workflowService.GetNextActivityRoleUserTree(runner, runner.Conditions)?.ToList()
                                    ?? new List<NodeView>();
                    foreach (var node in dto.NextNodes)
                        SleveenUserIdentity.EnrichNodeCandidateUsers(node);
                }
                catch
                {
                    dto.NextNodes = new List<NodeView>();
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
                    AppInstanceId = task.AppInstanceId,
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
                    if (!string.IsNullOrWhiteSpace(ai?.BackSrcActivityInstanceId))
                    {
                        dto.HasBackSrc = true;
                        dto.BackSrcActivityInstanceId = ai.BackSrcActivityInstanceId;
                    }
                }
                catch
                {
                    /* 退回源探测失败不阻断办理 */
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
                    dto.PreviousNodes = FilterPreviousBySendBackTarget(
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
            }

            // 表单：显式 FormId → 实例 AppInstanceCode（发起写入的 formId，Guid/数字均可）
            // → 流程 meta.FormId 回落。FbForm.Id 已是 Guid，禁止再用 int.TryParse 拦截。
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
                dto.FormValues = SailProcessVariableHelper.LoadFormValues(
                    _workflowService, processInstanceId, _logger);

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
    /// 办理表单 Id：query → AppInstanceCode（Guid/旧数字）→ sf_process_meta.FormId。
    /// </summary>
    private string? ResolveWorkbenchFormId(
        string? queryFormId,
        ApprovalWorkbenchDto dto,
        TaskViewEntity? task)
    {
        if (!string.IsNullOrWhiteSpace(queryFormId))
            return queryFormId.Trim();

        var code = dto.ProcessInstance?.AppInstanceCode?.Trim();
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

    /// <summary>§J 维①：按 sendBackTarget 收窄可退回节点列表。</summary>
    static List<NodeView> FilterPreviousBySendBackTarget(List<NodeView> nodes, string? target)
    {
        if (nodes == null || nodes.Count == 0)
            return nodes ?? new List<NodeView>();

        switch (SailNodePolicyCatalog.NormalizeTarget(target))
        {
            case "Prev":
                return nodes.Take(1).ToList();
            case "Starter":
            {
                var starts = nodes.Where(n => n.ActivityType == ActivityTypeEnum.StartNode).ToList();
                if (starts.Count > 0)
                    return starts;
                // 无开始事件时取列表末项（通常更靠近发起侧）
                return new List<NodeView> { nodes[^1] };
            }
            default:
                return nodes;
        }
    }

    /// <summary>Start/Run/办理成功后：把 Conditions 落成流程变量（表单回填依赖）。</summary>
    private void PersistRunnerConditions(FlowCommandContext? runner, WfExecutedResult? exec)
    {
        if (exec == null || exec.Status != WfExecutedStatus.Success)
            return;
        var piId = SailProcessVariableHelper.ResolveProcessInstanceId(
            _workflowService,
            runner,
            exec.ProcessInstanceIdStarted);
        SailProcessVariableHelper.PersistConditions(_workflowService, runner, piId, _logger);
    }

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
