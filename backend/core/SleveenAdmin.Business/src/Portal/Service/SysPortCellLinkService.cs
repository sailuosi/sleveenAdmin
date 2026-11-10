using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

[Scoped]
public sealed class SysPortCellLinkService {
    private readonly MooRepository<SysPortCellLink> _links;
    private readonly SysPortalCommonService _portalCommon;

    public SysPortCellLinkService(MooRepository<SysPortCellLink> links, SysPortalCommonService portalCommon)
    {
        _links = links;
        _portalCommon = portalCommon;
    }

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TablePortCellLink)
            .where("(Spcl_Deleted is null or Spcl_Deleted=0)");
        if (query.SpPortalId.HasValue)
            kit.where("Spcl_PortalId", query.SpPortalId.Value);
        if (query.SpPortCellId.HasValue)
            kit.where("Spcl_PortCellId", query.SpPortCellId.Value);
        var page = kit.setPage(pageSize, pageNum).orderBy("CL_Idx asc").queryPaged();
        PortalRowAlias.Alias(page.Items, PortalRowAlias.CellLink);
        return page;
    }

    public DataRow? GetById(Guid id)
    {
        var row = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TablePortCellLink).where("Spcl_Id", id).queryRow();
        return PortalRowAlias.Alias(row, PortalRowAlias.CellLink);
    }

    public void SaveBatch(IReadOnlyList<SysPortCellLink> rows)
    {
        var now = DateTime.Now;
        var portalIds = new HashSet<Guid>();
        foreach (var row in rows)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            row.CreateTime ??= now;
            row.Deleted ??= false;
            if (_links.Repo.GetById(row.Id) is null) _links.Repo.Insert(row);
            else _links.Repo.Update(row);

            var pid = row.PortalId;
            if (pid is null || pid == Guid.Empty)
            {
                var raw = DBCash.useSQL(MooSqlSetup.DefaultPosition)
                    .select("Spcl_PortalId").from(PortalSchema.TablePortCellLink).where("Spcl_Id", row.Id).queryRowString("");
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
            .select("Spcl_PortalId").from(PortalSchema.TablePortCellLink).where("Spcl_Id", id).queryRowString("");
        _links.Repo.DeleteById(id);
        if (Guid.TryParse(raw, out var portalId) && portalId != Guid.Empty)
            _portalCommon.InvalidatePortalConfig(portalId);
    }
}
