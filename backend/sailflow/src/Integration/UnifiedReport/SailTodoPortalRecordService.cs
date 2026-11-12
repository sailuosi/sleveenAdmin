using mooSQL.data;
using sailflow.engine.Client;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Sailflow.Integration.UnifiedReport;

/// <summary>集成平台推送流水。</summary>
[Scoped]
public sealed class SailTodoPortalRecordService
{
    private const int ResultMaxLen = 2000;
    private readonly SailClient _sail;

    public SailTodoPortalRecordService(SailClient sail) =>
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));

    public void EnsureTable()
    {
        _sail.UseRunSQL().useDBInit().autoMode().createTable<SailTodoPortalRecordEntity>();
    }

    public void Insert(
        string? taskId,
        string? scheduleId,
        string? toMaincode,
        string? toNickname,
        string opType,
        bool success,
        string? resultMsg)
    {
        var row = new SailTodoPortalRecordEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            ScheduleId = scheduleId ?? "",
            ToMaincode = toMaincode ?? "",
            ToNickname = toNickname ?? "",
            OpType = opType ?? "",
            IsSuccess = success ? "Y" : "N",
            Result = string.IsNullOrWhiteSpace(resultMsg) ? null : Truncate(resultMsg, ResultMaxLen),
            CreatedAt = DateTime.Now,
        };
        _sail.UseRunSQL().useRepo<SailTodoPortalRecordEntity>().Insert(row);
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max];
}
