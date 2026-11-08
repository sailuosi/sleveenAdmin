using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Officeman.Ebook.Service;

public sealed class BizSaEbNoteService : BizServiceBase<BizSaEbNoteRow>
{
    public override string BizName => "Biz_Sa_EbNote";

    protected override string TableName => EbookSchema.TableNote;

    protected override string OidColumn => "Sa_EbNoteOID";

    protected override string DefaultOrderBy => "mtb.SYS_Created desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems(EbookSchema.DictTopicStatus);

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.En_Title", "mtb.En_SourceUrl", "mtb.En_Content"], kw);
    }
}

public sealed class BizSaEbTpMatService : BizServiceBase<BizSaEbTpMatRow>
{
    public override string BizName => "Biz_Sa_EbTpMat";

    protected override string TableName => EbookSchema.TableTpMat;

    protected override string OidColumn => "Sa_EbTpMatOID";

    protected override string DefaultOrderBy => "mtb.Tm_Idx asc, mtb.SYS_Created desc";
}
