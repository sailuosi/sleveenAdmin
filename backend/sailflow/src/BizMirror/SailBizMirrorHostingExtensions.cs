using sailflow.engine.Client;
using sailflow.engine.Event;
using sailflow.engine.Hosting;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.BizMirror;

/// <summary>
/// stub: console-only; real port later。
/// BizMirror 伪能力包：仅控制台占位，对齐 pxxt8 挂载点 <c>UseBizMirrorHook</c>。
/// </summary>
public static class SailBizMirrorHostingExtensions
{
    static readonly EventFireTypeEnum[] MirrorEvents =
    [
        EventFireTypeEnum.OnProcessContinued,
        EventFireTypeEnum.OnProcessCompleted,
        EventFireTypeEnum.OnProcessRejected,
        EventFireTypeEnum.OnProcessClosed,
        EventFireTypeEnum.OnProcessWithdrawn,
        EventFireTypeEnum.OnProcessSendBacked,
        EventFireTypeEnum.OnProcessResent,
        EventFireTypeEnum.OnProcessRevised,
        EventFireTypeEnum.OnProcessReversed,
        EventFireTypeEnum.OnProcessJumped,
        EventFireTypeEnum.OnProcessSignForwarded
    ];

    static void ConfigureStub(HookRegistrationOptions o)
    {
        o.Match = HookMatch.All;
        // Sync：伪实现无需 Outbox；真实移植时应改为 AsyncOutbox + 投影服务
        o.Mode = HookMode.Sync;
        o.Events = MirrorEvents;
    }

    /// <summary>构建期挂伪 BizMirror Hook（仅控制台）。</summary>
    public static SailClientBuilder UseBizMirrorHook(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddHook(new SailBizMirrorStubHook(), ConfigureStub);
    }

    /// <summary>Collection 管道注册。</summary>
    public static ISailCollection UseBizMirrorHook(this ISailCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHook(new SailBizMirrorStubHook(), ConfigureStub);
    }

    /// <summary>已构建 Client 上补挂。</summary>
    public static SailClient useBizMirrorHook(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return client.addHook(new SailBizMirrorStubHook(), ConfigureStub);
    }
}
