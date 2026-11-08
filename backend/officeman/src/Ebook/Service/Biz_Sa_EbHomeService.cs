using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Officeman.Ebook.Service;

/// <summary>创作工作台首页门户聚合（选题 / 素材 / 笔记）。</summary>
public sealed class BizSaEbHomeService : BizServiceBase<BizSaEbNoteRow>
{
    public override string BizName => "Biz_Sa_EbHome";

    protected override string TableName => EbookSchema.TableNote;

    protected override string OidColumn => "Sa_EbNoteOID";

    public object GetDashboard(int days = 7)
    {
        if (days < 1) days = 7;
        if (days > 30) days = 30;

        var weekStart = WeekStart(DateTime.Today);
        var trendStart = DateTime.Today.AddDays(-(days - 1));

        var topicTotal = CountAll(EbookSchema.TableTopic);
        var topicInProgress = CountTopicInProgress();
        var topicPublished = CountWhere(EbookSchema.TableTopic, "Et_Status", "published");
        var topicWeekNew = CountCreatedSince(EbookSchema.TableTopic, weekStart);
        var publishedWeek = CountPublishedUpdatedSince(weekStart);

        var matTotal = CountAll(EbookSchema.TableMat);
        var matPending = CountWhere(EbookSchema.TableMat, "Em_Status", "raw");
        var matAnalyzed = CountWhere(EbookSchema.TableMat, "Em_Status", "analyzed");
        var matWeekNew = CountCreatedSince(EbookSchema.TableMat, weekStart);

        var noteTotal = CountAll(EbookSchema.TableNote);
        var noteWeekNew = CountCreatedSince(EbookSchema.TableNote, weekStart);
        var noteWithReport = CountNoteWithReport();

        var labels = BuildDayLabels(trendStart, days);
        var topicDaily = DailyCounts(EbookSchema.TableTopic, trendStart, days);
        var matDaily = DailyCounts(EbookSchema.TableMat, trendStart, days);
        var noteDaily = DailyCounts(EbookSchema.TableNote, trendStart, days);
        var publishedDaily = DailyPublishedCounts(trendStart, days);
        var totalDaily = new int[days];
        for (var i = 0; i < days; i++)
            totalDaily[i] = topicDaily[i] + matDaily[i] + noteDaily[i];

        var engage = SumNoteEngage();

        return new
        {
            welcome = new
            {
                topicTotal,
                inProgress = topicInProgress,
                published = topicPublished,
                noteTotal,
                weekTopicNew = topicWeekNew,
                weekMatNew = matWeekNew,
                weekNoteNew = noteWeekNew
            },
            stats = new
            {
                topicTotal,
                topicWeekNew,
                matTotal,
                matWeekNew,
                noteTotal,
                noteWeekNew,
                published = topicPublished,
                publishedWeek,
                sparklines = new
                {
                    topic = topicDaily,
                    mat = matDaily,
                    note = noteDaily,
                    published = publishedDaily
                }
            },
            trend = new
            {
                labels,
                topic = topicDaily,
                mat = matDaily,
                note = noteDaily,
                total = totalDaily
            },
            engage,
            health = new
            {
                matDigestRate = Rate(matAnalyzed, matTotal),
                topicFinishRate = Rate(topicPublished, topicTotal),
                noteReportRate = Rate(noteWithReport, noteTotal),
                matAnalyzed,
                matPending,
                matTotal,
                topicPublished,
                topicTotal,
                noteWithReport,
                noteTotal
            }
        };
    }

    private int CountAll(string table)
    {
        var kit = Db.useSQL();
        kit.from($"{table} as mtb").whereIsOrNull("mtb.SYS_Deleted", 0);
        return kit.count();
    }

    private int CountWhere(string table, string column, string value)
    {
        var kit = Db.useSQL();
        kit.from($"{table} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where($"mtb.{column}", value);
        return kit.count();
    }

    private int CountTopicInProgress()
    {
        var kit = Db.useSQL();
        kit.from($"{EbookSchema.TableTopic} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .whereFormat("(mtb.Et_Status is null or mtb.Et_Status <> {0})", "published");
        return kit.count();
    }

    private int CountCreatedSince(string table, DateTime since)
    {
        var kit = Db.useSQL();
        kit.from($"{table} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.SYS_Created", since, ">=");
        return kit.count();
    }

    private int CountPublishedUpdatedSince(DateTime since)
    {
        var kit = Db.useSQL();
        kit.from($"{EbookSchema.TableTopic} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Et_Status", "published")
            .where("mtb.SYS_LAST_UPD", since, ">=");
        return kit.count();
    }

    private int CountNoteWithReport()
    {
        var kit = Db.useSQL();
        kit.from($"{EbookSchema.TableNote} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .whereFormat("(mtb.En_ReportJson is not null and mtb.En_ReportJson <> {0})", "");
        return kit.count();
    }

    private int[] DailyCounts(string table, DateTime start, int days)
    {
        var kit = Db.useSQL();
        var dt = kit.select("mtb.SYS_Created as created")
            .from($"{table} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.SYS_Created", start, ">=")
            .query();
        return BucketByDay(dt, start, days, "created");
    }

    private int[] DailyPublishedCounts(DateTime start, int days)
    {
        var kit = Db.useSQL();
        var dt = kit.select("mtb.SYS_LAST_UPD as created")
            .from($"{EbookSchema.TableTopic} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Et_Status", "published")
            .where("mtb.SYS_LAST_UPD", start, ">=")
            .query();
        return BucketByDay(dt, start, days, "created");
    }

    private static int[] BucketByDay(DataTable dt, DateTime start, int days, string col)
    {
        var buckets = new int[days];
        var startDate = start.Date;
        foreach (DataRow r in dt.Rows)
        {
            if (r[col] is not DateTime d) continue;
            var idx = (d.Date - startDate).Days;
            if (idx >= 0 && idx < days) buckets[idx]++;
        }
        return buckets;
    }

    private static List<string> BuildDayLabels(DateTime start, int days)
    {
        var labels = new List<string>(days);
        for (var i = 0; i < days; i++)
            labels.Add(start.AddDays(i).ToString("MM-dd"));
        return labels;
    }

    private object SumNoteEngage()
    {
        var kit = Db.useSQL();
        var dt = kit.select("mtb.En_LikeCount as likes, mtb.En_BookmarkCount as bookmarks, mtb.En_ViewCount as views")
            .from($"{EbookSchema.TableNote} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .query();

        long likes = 0, bookmarks = 0, views = 0;
        foreach (DataRow r in dt.Rows)
        {
            likes += ParseCount(r["likes"]);
            bookmarks += ParseCount(r["bookmarks"]);
            views += ParseCount(r["views"]);
        }

        return new { likes, bookmarks, views };
    }

    private static long ParseCount(object? v)
    {
        if (v is null or DBNull) return 0;
        var s = v.ToString()?.Trim();
        if (string.IsNullOrEmpty(s)) return 0;
        var digits = new string(s.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray())
            .Replace(",", "");
        if (double.TryParse(digits, out var n))
        {
            if (s.Contains('万')) n *= 10000;
            return (long)Math.Round(n);
        }
        return 0;
    }

    private static double Rate(int part, int total)
    {
        if (total <= 0) return 0;
        return Math.Round(part * 100.0 / total, 1);
    }

    private static DateTime WeekStart(DateTime day)
        => day.AddDays(-(((int)day.DayOfWeek + 6) % 7));
}
