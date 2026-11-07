using mooSQL.data;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook.Service;

[SooTable(Name = EbookSchema.TableNote, Alias = "mtb", Type = DBTableType.Select)]
public class BizSaEbNoteRow : Sa_EbNote
{
}
