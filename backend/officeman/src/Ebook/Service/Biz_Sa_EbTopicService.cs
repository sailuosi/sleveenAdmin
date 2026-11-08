using System.Data;
using System.Text.Json;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Officeman.Ebook.Service;

public sealed class BizSaEbTopicService : BizServiceBase<BizSaEbTopicRow>
{
    private static readonly string[] GraphicStages = ["inspiration", "draft", "final", "graphic", "publish"];
    private static readonly string[] VideoStages = ["inspiration", "outline", "script", "filming", "editing", "publish"];

    public override string BizName => "Biz_Sa_EbTopic";

    protected override string TableName => EbookSchema.TableTopic;

    protected override string OidColumn => "Sa_EbTopicOID";

    protected override string DefaultOrderBy => "mtb.SYS_Created desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems(
            EbookSchema.DictPlatform,
            EbookSchema.DictStageGraphic,
            EbookSchema.DictStageVideo,
            EbookSchema.DictPublishFormat,
            EbookSchema.DictTopicStatus);

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Et_Title", "mtb.Et_Desc", "mtb.Et_Audience"], kw);
    }

    public override PagedResult<BizSaEbTopicRow> LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select($@"mtb.*, (
            select count(1) from {EbookSchema.TableTpMat} tm
            where tm.Tm_TopicOID = mtb.{OidColumn}
              and (tm.SYS_Deleted is null or tm.SYS_Deleted = 0)
        ) as MatCount").from($"{TableName} as mtb");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? DefaultOrderBy : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        var records = MaterializeList(dt);
        return PagedResult<BizSaEbTopicRow>.Create(records, page, size, total);
    }

    public object GetStats()
    {
        var kit = Db.useSQL();
        kit.from($"{TableName} as mtb").whereIsOrNull("mtb.SYS_Deleted", 0);
        var total = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .whereFormat("(mtb.Et_Status is null or mtb.Et_Status <> {0})", "published");
        var inProgress = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Et_Stage in ('draft','outline','script','final','graphic','filming','editing')");
        var writing = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Et_Stage", "inspiration");
        var inspiration = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Et_Status", "published");
        var published = kit.count();

        var weekStart = DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek + 6) % 7));
        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.SYS_Created", weekStart, ">=");
        var weekNew = kit.count();

        kit.clear().from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .where("mtb.Et_Status", "published")
            .where("mtb.SYS_LAST_UPD", weekStart, ">=");
        var publishedWeek = kit.count();

        var stageDt = kit.clear().select("mtb.Et_Stage as stage, count(1) as cnt")
            .from($"{TableName} as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .groupBy("mtb.Et_Stage").query();
        var byStage = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (DataRow r in stageDt.Rows)
        {
            var key = r["stage"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(key)) byStage[key] = ToInt(r["cnt"]);
        }

        return new
        {
            total,
            inProgress,
            writing,
            inspiration,
            published,
            weekNew,
            publishedWeek,
            byStage
        };
    }

    public object AdvanceStage(Guid topicId)
    {
        var row = LoadRowById(topicId)
                  ?? throw new InvalidOperationException("选题不存在");
        var pipeline = row.Et_Pipeline ?? "graphic";
        var stage = row.Et_Stage ?? "inspiration";
        var stages = string.Equals(pipeline, "video", StringComparison.OrdinalIgnoreCase)
            ? VideoStages
            : GraphicStages;
        var idx = Array.FindIndex(stages, s => string.Equals(s, stage, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) idx = 0;
        if (idx >= stages.Length - 1)
            return new { stage = stages[^1], done = true, message = "已到最终阶段" };

        var next = stages[idx + 1];
        var status = next == "publish" ? "published" : (next is "final" or "graphic" or "editing" ? "final" : "draft");
        Db.useSQL().setTable(TableName)
            .set("Et_Stage", next)
            .set("Et_Status", status)
            .set("SYS_LAST_UPD", DateTime.Now)
            .whereGuid(OidColumn, topicId)
            .doUpdate();
        return new { stage = next, status, done = next == "publish" };
    }

    public List<Dictionary<string, object?>> ListMaterials(Guid topicId)
    {
        var kit = Db.useSQL();
        kit.select("m.*, tm.Sa_EbTpMatOID, tm.Tm_Idx")
            .from(EbookSchema.TableTpMat + " as tm")
            .join(EbookSchema.TableMat + " as m on m.Sa_EbMatOID = tm.Tm_MatOID")
            .whereGuid("tm.Tm_TopicOID", topicId)
            .whereIsOrNull("tm.SYS_Deleted", 0)
            .whereIsOrNull("m.SYS_Deleted", 0)
            .orderBy("tm.Tm_Idx asc, tm.SYS_Created desc");
        var dt = kit.query();
        var list = new List<Dictionary<string, object?>>();
        foreach (DataRow r in dt.Rows)
        {
            var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn col in r.Table.Columns)
            {
                var v = r[col];
                map[col.ColumnName] = v == DBNull.Value ? null : v;
            }

            list.Add(map);
        }

        return list;
    }

    public object BindMaterial(JsonElement para)
    {
        var topicId = ReadGuid(para, "topicId", "Tm_TopicOID", "Sa_EbTopicOID");
        var matId = ReadGuid(para, "matId", "Tm_MatOID", "Sa_EbMatOID");
        if (topicId == Guid.Empty || matId == Guid.Empty)
            throw new ArgumentException("topicId / matId 不能为空");

        var exists = Db.useSQL().from(EbookSchema.TableTpMat)
            .whereGuid("Tm_TopicOID", topicId)
            .whereGuid("Tm_MatOID", matId)
            .whereIsOrNull("SYS_Deleted", 0)
            .count() > 0;
        if (exists)
            return new { bound = true, duplicated = true };

        var oid = Guid.CreateVersion7();
        var now = DateTime.Now;
        Db.useSQL().setTable(EbookSchema.TableTpMat)
            .set("Sa_EbTpMatOID", oid)
            .set("Tm_TopicOID", topicId)
            .set("Tm_MatOID", matId)
            .set("Tm_Idx", 0)
            .set("SYS_Created", now)
            .set("SYS_LAST_UPD", now)
            .set("SYS_Deleted", 0)
            .doInsert();
        return new { bound = true, Sa_EbTpMatOID = oid };
    }

    public object UnbindMaterial(JsonElement para)
    {
        var linkId = ReadGuid(para, "linkId", "Sa_EbTpMatOID");
        var topicId = ReadGuid(para, "topicId", "Tm_TopicOID");
        var matId = ReadGuid(para, "matId", "Tm_MatOID");

        var kit = Db.useSQL().setTable(EbookSchema.TableTpMat)
            .set("SYS_Deleted", 1)
            .set("SYS_LAST_UPD", DateTime.Now);
        if (linkId != Guid.Empty)
            kit.whereGuid("Sa_EbTpMatOID", linkId);
        else
        {
            kit.whereGuid("Tm_TopicOID", topicId).whereGuid("Tm_MatOID", matId);
        }

        kit.doUpdate();
        return new { unbound = true };
    }

    private static Guid ReadGuid(JsonElement para, params string[] keys)
    {
        if (para.ValueKind != JsonValueKind.Object) return Guid.Empty;
        foreach (var key in keys)
        {
            if (!para.TryGetProperty(key, out var el)) continue;
            if (Guid.TryParse(el.ToString(), out var g)) return g;
        }

        return Guid.Empty;
    }

    private static int ToInt(object? v)
    {
        if (v is null or DBNull) return 0;
        return int.TryParse(v.ToString(), out var n) ? n : Convert.ToInt32(v);
    }
}
