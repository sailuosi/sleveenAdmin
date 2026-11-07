using mooSQL.data;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook.Service;

[SooTable(Name = EbookSchema.TableTpMat, Alias = "mtb", Type = DBTableType.Select)]
public class BizSaEbTpMatRow : Sa_EbTpMat
{
}
