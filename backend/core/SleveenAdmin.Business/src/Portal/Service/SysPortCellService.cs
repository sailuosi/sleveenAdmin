using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

[Scoped]
public sealed class SysPortCellService {
    private readonly MooRepository<SysPortCell> _cells;
    private readonly SysPortalCommonService _portalCommon;

    public SysPortCellService(MooRepository<SysPortCell> cells, SysPortalCommonService portalCommon)
    {
        _cells = cells;
        _portalCommon = portalCommon;
    }

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TablePortCell)
            .where("(Spc_Deleted is null or Spc_Deleted=0)");
        if (query.SpPortalId.HasValue)
            kit.where("Spc_PortalId", query.SpPortalId.Value);
        var page = kit.setPage(pageSize, pageNum).orderBy("PC_Idx asc").queryPaged();
        PortalRowAlias.Alias(page.Items, PortalRowAlias.Cell);
        return page;
    }

    public DataRow? GetById(Guid id)
    {
        var row = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TablePortCell).where("Spc_Id", id).queryRow();
        return PortalRowAlias.Alias(row, PortalRowAlias.Cell);
    }

    public void SaveBatch(IReadOnlyList<SysPortCell> rows)
    {
        var now = DateTime.Now;
        var portalIds = new HashSet<Guid>();
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

            var pid = row.PortalId;
            if (pid is null || pid == Guid.Empty)
            {
                var raw = DBCash.useSQL(MooSqlSetup.DefaultPosition)
                    .select("Spc_PortalId").from(PortalSchema.TablePortCell).where("Spc_Id", row.Id).queryRowString("");
                if (Guid.TryParse(raw, out var g)) pid = g;
            }
            if (pid.HasValue && pid.Value != Guid.Empty)
                portalIds.Add(pid.Value);
        }
        foreach (var pid in portalIds)
            _portalCommon.InvalidatePortalConfig(pid);
    }

    public void Delete(Guid id)
    {
        var raw = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("Spc_PortalId").from(PortalSchema.TablePortCell).where("Spc_Id", id).queryRowString("");
        _cells.Repo.DeleteById(id);
        if (Guid.TryParse(raw, out var portalId) && portalId != Guid.Empty)
            _portalCommon.InvalidatePortalConfig(portalId);
    }
}
