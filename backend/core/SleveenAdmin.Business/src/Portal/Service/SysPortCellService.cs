using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysPortCellService : IScoped
{
    private readonly MooRepository<SysPortCell> _cells;

    public SysPortCellService(MooRepository<SysPortCell> cells) => _cells = cells;

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TablePortCell)
            .where("(Spc_Deleted is null or Spc_Deleted=0)");
        if (query.SpPortalId.HasValue)
            kit.where("Spc_PortalId", query.SpPortalId.Value);
        return kit.setPage(pageSize, pageNum).orderBy("PC_Idx asc").queryPaged();
    }

    public DataRow? GetById(Guid id) =>
        DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TablePortCell).where("Spc_Id", id).queryRow();

    public void SaveBatch(IReadOnlyList<SysPortCell> rows)
    {
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            row.CreateTime ??= now;
            row.Deleted ??= false;
            if (_cells.Repo.GetById(row.Id) is null)
                _cells.Repo.Insert(row);
            else
                _cells.Repo.Update(row);
        }
    }

    public void Delete(Guid id) => _cells.Repo.DeleteById(id);
}
