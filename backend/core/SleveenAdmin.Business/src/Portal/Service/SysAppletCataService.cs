using System.Data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysAppletCataService : IScoped
{
    private readonly MooRepository<SysAppletCata> _catas;

    public SysAppletCataService(MooRepository<SysAppletCata> catas) => _catas = catas;

    public PagedResult<DataRow> GetPage(PortalPageQuery query)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TableAppletCata)
            .where("(Sac_Deleted is null or Sac_Deleted=0)");
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var dt = kit.setPage(pageSize, pageNum).orderBy("AC_Idx asc").query();
        return PagedResult<DataRow>.Create(dt.Rows.Cast<DataRow>().ToList(), pageNum, pageSize, kit.count());
    }

    public DataRow? GetById(Guid id) =>
        DBCash.useSQL(MooSqlSetup.DefaultPosition).select("*").from(PortalSchema.TableAppletCata).where("Sac_Id", id).queryRow();

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
