using Microsoft.Extensions.DependencyInjection;
using sailflow.biz.Hosting;
using sailflow.engine.Client;
using sailflow.engine.Hosting;
using SleveenAdmin.Sailflow.FormBiz.Handlers;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.FormBiz;

public static class SailFormBizHostingExtensions
{
    /// <summary>DI：Registry + Leave/Dynamic Handler + Host 模板。</summary>
    public static IServiceCollection AddSailFormBiz(this IServiceCollection services)
    {
        services.AddScoped<ISailFormBizHandler, LeaveFormBizHandler>();
        services.AddScoped<ISailFormBizHandler, DynamicFormBizHandler>();
        services.AddScoped<SailFormBizRegistry>();
        services.AddScoped<SailFormBizHost>();
        return services;
    }

    /// <summary>
    /// 构建期挂审批业务门面（Finish/Aborted + Completing Gate）。
    /// 内部委托 <see cref="ApprovalBusinessSailExtensions.UseApprovalBusiness"/>。
    /// </summary>
    public static SailClientBuilder UseFormBizLifecycleHook(this SailClientBuilder builder)
        => builder.UseApprovalBusinessForHostFormBiz();

    /// <summary>显式挂载：请假 + Dynamic → <c>UseApprovalBusiness</c>。</summary>
    public static SailClientBuilder UseApprovalBusinessForHostFormBiz(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddApprovalBusiness(c => new LeaveFormBizHandler(new SailLeaveService(c)));
        builder.AddApprovalBusiness(_ => new DynamicFormBizHandler());
        return builder.UseApprovalBusiness();
    }
}
