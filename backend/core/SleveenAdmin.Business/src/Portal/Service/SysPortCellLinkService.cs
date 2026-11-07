using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysPortCellLinkService : IScoped
{
    private readonly MooRepository<SysPortCellLink> _links;

    public SysPortCellLinkService(MooRepository<SysPortCellLink> links) => _links = links;

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TablePortCellLink)
            .where("(Spcl_Deleted is null or Spcl_Deleted=0)");
        if (query.SpPortalId.HasValue)
            kit.where("Spcl_PortalId", query.SpPortalId.Value);
        return kit.setPage(pageSize, pageNum).orderBy("CL_Idx asc").queryPaged();
    }

    public DataRow? GetById(Guid id) =>
        DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TablePortCellLink).where("Spcl_Id", id).queryRow();

    public void SaveBatch(IReadOnlyList<SysPortCellLink> rows)
    {
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            row.CreateTime ??= now;
            row.Deleted ??= false;
            if (_links.Repo.GetById(row.Id) is null) _links.Repo.Insert(row);
            else _links.Repo.Update(row);
        }
    }

    public void Delete(Guid id) => _links.Repo.DeleteById(id);
}
