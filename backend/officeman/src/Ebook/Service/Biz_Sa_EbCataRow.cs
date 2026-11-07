using mooSQL.data;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook.Service;

/// <summary>查询/序列化行（Alias=mtb）。</summary>
[SooTable(Name = EbookSchema.TableCata, Alias = "mtb", Type = DBTableType.Select)]
public class BizSaEbCataRow : Sa_EbCata
{
}
