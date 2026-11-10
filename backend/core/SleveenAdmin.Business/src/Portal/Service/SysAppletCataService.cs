using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

[Scoped]
public sealed class SysAppletCataService {
    private readonly MooRepository<SysAppletCata> _catas;

    public SysAppletCataService(MooRepository<SysAppletCata> catas) => _catas = catas;

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TableAppletCata)
            .where("(Sac_Deleted is null or Sac_Deleted=0)");
        var page = kit.setPage(pageSize, pageNum).orderBy("AC_Idx asc").queryPaged();
        PortalRowAlias.Alias(page.Items, PortalRowAlias.AppletCata);
        return page;
    }

    public DataRow? GetById(Guid id)
    {
        var row = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TableAppletCata).where("Sac_Id", id).queryRow();
        return PortalRowAlias.Alias(row, PortalRowAlias.AppletCata);
    }

    public void SaveBatch(IReadOnlyList<SysAppletCata> rows)
    {
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            row.CreateTime ??= now;
            row.Deleted ??= false;
            if (_catas.Repo.GetById(row.Id) is null)
                _catas.Repo.Insert(row);
            else
                _catas.Repo.Update(row);
        }
    }

    public void Delete(Guid id) => _catas.Repo.DeleteById(id);
}
