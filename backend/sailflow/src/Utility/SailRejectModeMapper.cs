using sailflow.utils;

namespace SleveenAdmin.Sailflow.Utility;

// FlowCommandContext / ControlParameterSheet / RejectModeCodes：sailflow.utils

/// <summary>
/// NodePolicy.rejectHandlerType → 引擎 <see cref="RejectModeCodes"/>。
/// 兼容：1=Terminate（纠偏旧「结束」文案）、2=Specified；新增 3=Previous（默认）、4=Starter。
/// </summary>
public static class SailRejectModeMapper
{
    public const int TypeTerminate = 1;
    public const int TypeSpecified = 2;
    public const int TypePrevious = 3;
    public const int TypeStarter = 4;

    /// <summary>产品默认：上一节点。</summary>
    public const int DefaultType = TypePrevious;

    public static string ToRejectMode(int rejectHandlerType) =>
        rejectHandlerType switch
        {
            TypeTerminate => RejectModeCodes.Terminate,
            TypeSpecified => RejectModeCodes.Specified,
            TypeStarter => RejectModeCodes.Starter,
            TypePrevious => RejectModeCodes.Previous,
            0 => RejectModeCodes.Previous,
            _ => RejectModeCodes.Previous
        };

    public static void ApplyToRunner(FlowCommandContext runner, SailNodePolicyDto policy)
    {
        ArgumentNullException.ThrowIfNull(runner);
        policy ??= SailNodePolicyCatalog.Defaults;
        runner.ControlParameterSheet ??= new ControlParameterSheet();
        runner.ControlParameterSheet.RejectMode = ToRejectMode(policy.RejectHandlerType);
        if (string.Equals(
                runner.ControlParameterSheet.RejectMode,
                RejectModeCodes.Specified,
                StringComparison.OrdinalIgnoreCase))
        {
            runner.ControlParameterSheet.RejectReturnActivityId =
                policy.ReturnNodeId?.Trim() ?? "";
        }
        else
        {
            runner.ControlParameterSheet.RejectReturnActivityId = "";
        }
    }
}
