using mooSQL.data;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook.Service;

[SooTable(Name = EbookSchema.TableTopic, Alias = "mtb", Type = DBTableType.Select)]
public class BizSaEbTopicRow : Sa_EbTopic
{
    /// <summary>列表联查：绑定素材数。</summary>
    public int MatCount { get; set; }
}
