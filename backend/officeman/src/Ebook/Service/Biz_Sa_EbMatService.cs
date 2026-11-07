using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Officeman.Ebook.Service;

public sealed class BizSaEbMatService : BizServiceBase
{
    public override string BizName => "Biz_Sa_EbMat";

    protected override string TableName => EbookSchema.TableMat;

    protected override string OidColumn => "Sa_EbMatOID";

    protected override string DefaultOrderBy => "mtb.SYS_Created desc";

    protected override Dictionary<string, object> LoadInitDicts()
    {
        var dicts = LoadDictItems(
            EbookSchema.DictPlatform,
            EbookSchema.DictMaterialStatus);
        return dicts.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Em_Title", "mtb.Em_SourceAuthor", "mtb.Em_Summary"], kw);
    }

    public object GetStats()
    {
        var kit = Db.useSQL();
        kit.from($"{TableName} as mtb").whereIsOrNull("mtb.SYS_Deleted", 0);
        var total = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Em_Status", "raw");
        var pending = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Em_Status", "analyzed");
        var analyzed = kit.count();

        var weekStart = DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek + 6) % 7));
        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.SYS_Created", weekStart, ">=");
        var weekNew = kit.count();

        var platformDt = kit.clear().select("mtb.Em_Platform as platform, count(1) as cnt")
            .from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .groupBy("mtb.Em_Platform").query();
        var byPlatform = new List<object>();
        foreach (DataRow r in platformDt.Rows)
        {
            byPlatform.Add(new { platform = r["platform"]?.ToString(), count = ToInt(r["cnt"]) });
        }

        return new
        {
            collected = total,
            pending,
            analyzed,
            weekNew,
            platforms = byPlatform
        };
    }

    private static int ToInt(object? v)
    {
        if (v is null or DBNull) return 0;
        return int.TryParse(v.ToString(), out var n) ? n : Convert.ToInt32(v);
    }
}
