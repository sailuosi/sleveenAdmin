namespace SleveenAdmin.Sailflow.FormBiz.Handlers;

/// <summary>
/// §H 低代码默认 Handler：不落专用业务表；AppInstanceId 用 GUID，条件来自 Payload。
/// </summary>
public sealed class DynamicFormBizHandler : SailFormBizHandlerBase
{
    public const string Code = "Dynamic";

    public override string FormCode => Code;

    public override void OnInit(SailFormBizContext ctx)
    {
        ctx.Data = new { };
    }

    public override void OnSubmit(SailFormBizContext ctx)
    {
        ctx.AppName = FormCode;
        if (string.IsNullOrWhiteSpace(ctx.AppInstanceId))
            ctx.AppInstanceId = Guid.NewGuid().ToString("N");
        ctx.Data = ctx.PayloadJson;
    }

    public override void OnStarted(SailFormBizContext ctx)
    {
        // 无产品表；ProcessInstanceId 已在 ctx
    }

    public override void OnFinish(SailFormBizContext ctx)
    {
        // 动态表单默认无状态回写；业务可换专用 Handler
    }
}
