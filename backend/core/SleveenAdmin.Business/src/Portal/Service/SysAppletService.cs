using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

[Scoped]
public sealed class SysAppletService {
    private readonly MooRepository<SysApplet> _applets;

    public SysAppletService(MooRepository<SysApplet> applets) => _applets = applets;

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TableApplet)
            .where("(Sa_Deleted is null or Sa_Deleted=0)");
        if (query.SaAppletCataId.HasValue)
            kit.where("Sa_AppletCataId", query.SaAppletCataId.Value);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            kit.whereLike("A_Title", query.Keyword);
        var page = kit.setPage(pageSize, pageNum).orderBy("Sa_CreateTime desc").queryPaged();
        PortalRowAlias.Alias(page.Items, PortalRowAlias.Applet);
        return page;
    }

    public DataRow? GetById(Guid id)
    {
        var row = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TableApplet).where("Sa_Id", id).queryRow();
        return PortalRowAlias.Alias(row, PortalRowAlias.Applet);
    }

    public void SaveBatch(IReadOnlyList<SysApplet> rows)
    {
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            row.CreateTime ??= now;
            row.Deleted ??= false;
            if (_applets.Repo.GetById(row.Id) is null)
                _applets.Repo.Insert(row);
            else
                _applets.Repo.Update(row);
        }
    }

    public void Delete(Guid id) => _applets.Repo.DeleteById(id);
}
