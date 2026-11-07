using mooSQL.data;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook.Service;

[SooTable(Name = EbookSchema.TableTopic, Alias = "mtb", Type = DBTableType.Select)]
public class BizSaEbTopicRow : Sa_EbTopic
{
}
