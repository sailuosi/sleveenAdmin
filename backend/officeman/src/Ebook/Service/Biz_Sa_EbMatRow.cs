using mooSQL.data;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook.Service;

[SooTable(Name = EbookSchema.TableMat, Alias = "mtb", Type = DBTableType.Select)]
public class BizSaEbMatRow : Sa_EbMat
{
}
