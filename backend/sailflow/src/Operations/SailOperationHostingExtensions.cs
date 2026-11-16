using Microsoft.Extensions.DependencyInjection;
using SleveenAdmin.Sailflow.Operations.Handlers;

namespace SleveenAdmin.Sailflow.Operations;

public static class SailOperationHostingExtensions
{
    /// <summary>注册办理分发器 + 已对齐引擎命令的 Handler（含 Transfer）。Copy 仍走产品表 API，不在此列。</summary>
    public static IServiceCollection AddSailOperationHandlers(this IServiceCollection services)
    {
        services.AddScoped<ISailOperationHandler, ApproveOperationHandler>();
        services.AddScoped<ISailOperationHandler, RejectOperationHandler>();
        services.AddScoped<ISailOperationHandler, SendBackOperationHandler>();
        services.AddScoped<ISailOperationHandler, SignForwardOperationHandler>();
        services.AddScoped<ISailOperationHandler, RemoveAssigneeOperationHandler>();
        services.AddScoped<ISailOperationHandler, ChangeAssigneeOperationHandler>();
        services.AddScoped<ISailOperationHandler, UrgeOperationHandler>();
        services.AddScoped<ISailOperationHandler, TransferOperationHandler>();
        services.AddScoped<ISailOperationHandler, DelegateOperationHandler>();
        services.AddScoped<ISailOperationHandler, ConsultOperationHandler>();
        services.AddScoped<ISailOperationHandler, AcknowledgeOperationHandler>();
        services.AddScoped<ISailOperationHandler, RecallConsultOperationHandler>();
        services.AddScoped<ISailOperationHandler, WithdrawOperationHandler>();
        services.AddScoped<ISailOperationHandler, ResendOperationHandler>();
        services.AddScoped<ISailOperationHandler, CloseOperationHandler>();
        services.AddScoped<ISailOperationHandler, TerminateOperationHandler>();
        services.AddScoped<ISailOperationHandler, DiscardOperationHandler>();
        services.AddScoped<ISailOperationHandler, JumpOperationHandler>();
        services.AddScoped<ISailOperationHandler, ReviseOperationHandler>();
        services.AddScoped<ISailOperationHandler, ReverseOperationHandler>();
        services.AddScoped<SailOperationDispatcher>();
        return services;
    }
}
