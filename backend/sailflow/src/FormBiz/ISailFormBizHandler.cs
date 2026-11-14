using sailflow.engine.Common;
using sailflow.engine.Event;

namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>
/// §H：一业务一类 Handler。只做业务单读写/状态；流转仍走引擎命令。
/// </summary>
public interface ISailFormBizHandler
{
    /// <summary>路由键（与 Runner.BizChannel / 发起 FormCode 对齐）。</summary>
    string FormCode { get; }

    void OnInit(SailFormBizContext ctx);
    void OnQuery(SailFormBizContext ctx);
    /// <summary>首次提交：落业务表并写出 BizPK / Conditions；Start 由 Host 模板调用。</summary>
    void OnSubmit(SailFormBizContext ctx);
    /// <summary>StartProcess 成功后回写实例 Id 等。</summary>
    void OnStarted(SailFormBizContext ctx);
    void OnConsent(SailFormBizContext ctx);
    void OnBackToModify(SailFormBizContext ctx);
    void OnCancel(SailFormBizContext ctx);
    void OnFinish(SailFormBizContext ctx);
}

/// <summary>业务表单生命周期上下文（Host 组装；Handler 可改写字段）。</summary>
public sealed class SailFormBizContext
{
    public string FormCode { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string ProcessId { get; set; } = "";
    public string Version { get; set; } = "1";
    public string BizChannel { get; set; } = "";
    public string BizPK { get; set; } = "";
    public string? BizType { get; set; }
    public string? BizCode { get; set; }
    public string? BizName { get; set; }
    public string ProcessInstanceId { get; set; } = "";
    public Dictionary<string, string> Conditions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>发起/查询 JSON（Submit 入参）。</summary>
    public string? PayloadJson { get; set; }
    /// <summary>Handler 可挂业务实体等。</summary>
    public object? Data { get; set; }
    public string? ButtonId { get; set; }
    public EventFireTypeEnum? FinishEvent { get; set; }
    public string? Error { get; set; }

    public FlowCommandContext ToRunner()
    {
        var runner = new FlowCommandContext
        {
            UserId = UserId,
            UserName = UserName,
            ProcessId = ProcessId,
            Version = string.IsNullOrWhiteSpace(Version) ? "1" : Version,
            Conditions = Conditions.Count > 0 ? new Dictionary<string, string>(Conditions) : null
        };
        runner.ApplyBizData(new BizDataDto
        {
            BizPK = BizPK,
            BizType = BizType,
            BizCode = BizCode,
            BizChannel = string.IsNullOrWhiteSpace(BizChannel) ? FormCode : BizChannel,
            BizName = BizName
        });
        return runner;
    }
}

/// <summary>可选空实现，业务 Handler 按需 override。</summary>
public abstract class SailFormBizHandlerBase : ISailFormBizHandler
{
    public abstract string FormCode { get; }

    public virtual void OnInit(SailFormBizContext ctx) { }
    public virtual void OnQuery(SailFormBizContext ctx) { }
    public virtual void OnSubmit(SailFormBizContext ctx) { }
    public virtual void OnStarted(SailFormBizContext ctx) { }
    public virtual void OnConsent(SailFormBizContext ctx) { }
    public virtual void OnBackToModify(SailFormBizContext ctx) { }
    public virtual void OnCancel(SailFormBizContext ctx) { }
    public virtual void OnFinish(SailFormBizContext ctx) { }
}