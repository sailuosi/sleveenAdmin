using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Officeman.Ebook.Service;

public sealed class BizSaEbCataService : BizServiceBase
{
    public override string BizName => "Biz_Sa_EbCata";

    protected override string TableName => EbookSchema.TableCata;

    protected override string OidColumn => "Sa_EbCataOID";

    protected override string DefaultOrderBy => "mtb.Ec_Idx asc, mtb.SYS_Created desc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Ec_Name", "mtb.Ec_Code"], kw);
    }

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            if (key.Equals("Ec_Scope", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("scope", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(value, "all", StringComparison.OrdinalIgnoreCase))
                {
                    kit.whereFormat(
                        "(mtb.Ec_Scope = {0} or mtb.Ec_Scope = {1} or mtb.Ec_Scope is null)",
                        value,
                        "all");
                }

                continue;
            }

            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }
}
