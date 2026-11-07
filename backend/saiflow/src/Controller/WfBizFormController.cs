using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using saiflow.engine.Result;
using saiflow.WebUtility;
using SleveenAdmin.Saiflow.FormBiz;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>§H：按 FormCode 走业务 Handler（Submit → Start）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfBizFormController(SailFormBizHost bizHost) : ControllerBase
{
    public class BizFormSubmitBody
    {
        public string? FormCode { get; set; }
        public string? ProcessId { get; set; }
        public string? Version { get; set; }
        public object? Payload { get; set; }
        public string? PayloadJson { get; set; }
    }

    public class BizFormQueryBody
    {
        public string? FormCode { get; set; }
        public string? AppInstanceId { get; set; }
    }

    /// <summary>POST api/WfBizForm/Submit · OnSubmit + StartProcess</summary>
    [HttpPost]
    public async Task<ResponseResult<object>> Submit([FromBody] BizFormSubmitBody? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.FormCode))
                return ResponseResult<object>.Error("FormCode required");
            if (string.IsNullOrWhiteSpace(body.ProcessId))
                return ResponseResult<object>.Error("ProcessId required");

            SailAuthIdentity.BindCreator(User, out var account, out var displayName);
            var payloadJson = body.PayloadJson;
            if (string.IsNullOrWhiteSpace(payloadJson) && body.Payload != null)
                payloadJson = JsonSerializer.Serialize(body.Payload);

            var (ctx, exec) = await bizHost.SubmitAndStartAsync(new SubmitBizFormRequest
            {
                FormCode = body.FormCode!,
                ProcessId = body.ProcessId,
                Version = body.Version,
                PayloadJson = payloadJson,
                UserId = account,
                UserName = displayName
            });

            if (!string.IsNullOrWhiteSpace(ctx.Error))
                return ResponseResult<object>.Error(ctx.Error);

            if (exec == null)
                return ResponseResult<object>.Error("StartProcess skipped");

            if (exec.Status != WfExecutedStatus.Success)
                return ResponseResult<object>.Error(exec.Message ?? "StartProcess failed");

            var ok = ResponseResult<object>.Success(ctx.Data ?? new
            {
                ctx.AppInstanceId,
                ctx.ProcessInstanceId,
                ctx.FormCode
            }, "submitted");
            if (exec.ProcessInstanceIdStarted > 0)
                ok.NewId = exec.ProcessInstanceIdStarted;
            return ok;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No form biz handler"))
        {
            return ResponseResult<object>.Error($"FormBizHandlerMissing: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfBizForm/Init · OnInit 默认值</summary>
    [HttpPost]
    public ResponseResult<object> Init([FromBody] BizFormQueryBody? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.FormCode))
                return ResponseResult<object>.Error("FormCode required");
            var handler = bizHost.Registry.GetRequired(body.FormCode);
            var ctx = new SailFormBizContext { FormCode = body.FormCode };
            handler.OnInit(ctx);
            return ResponseResult<object>.Success(ctx.Data ?? new { });
        }
        catch (InvalidOperationException ex)
        {
            return ResponseResult<object>.Error($"FormBizHandlerMissing: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfBizForm/Query · OnQuery</summary>
    [HttpPost]
    public ResponseResult<object> Query([FromBody] BizFormQueryBody? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.FormCode))
                return ResponseResult<object>.Error("FormCode required");
            var handler = bizHost.Registry.GetRequired(body.FormCode);
            var ctx = new SailFormBizContext
            {
                FormCode = body.FormCode,
                AppInstanceId = body.AppInstanceId ?? ""
            };
            handler.OnQuery(ctx);
            if (!string.IsNullOrWhiteSpace(ctx.Error))
                return ResponseResult<object>.Error(ctx.Error);
            return ResponseResult<object>.Success(ctx.Data ?? new { });
        }
        catch (InvalidOperationException ex)
        {
            return ResponseResult<object>.Error($"FormBizHandlerMissing: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfBizForm/Probe · 已注册 FormCode</summary>
    [HttpGet]
    public ResponseResult Probe()
    {
        var ok = ResponseResult.Success("ok");
        ok.ExtraData = bizHost.Registry.RegisteredCodes;
        return ok;
    }
}
