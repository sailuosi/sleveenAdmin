namespace SleveenAdmin.Sailflow.Utility;

/// <summary>§J：节点 NodePolicy 与流程 Extra 退回两维合并（Extra 非空覆盖节点）。</summary>
public static class SailSendBackPolicy
{
    public readonly record struct Resolved(string SendBackTarget, string ResubmitMode);

    /// <summary>
    /// 节点优先；当 Extra 提供非空值时覆盖对应维。
    /// </summary>
    public static Resolved Resolve(
        string? nodeTarget,
        string? nodeResubmit,
        string? extraTarget,
        string? extraResubmit)
    {
        var target = string.IsNullOrWhiteSpace(extraTarget)
            ? SailNodePolicyCatalog.NormalizeTarget(nodeTarget)
            : SailNodePolicyCatalog.NormalizeTarget(extraTarget);
        var resubmit = string.IsNullOrWhiteSpace(extraResubmit)
            ? SailNodePolicyCatalog.NormalizeResubmit(nodeResubmit)
            : SailNodePolicyCatalog.NormalizeResubmit(extraResubmit);
        return new Resolved(target, resubmit);
    }
}
