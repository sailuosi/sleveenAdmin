using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class SysPortPageService : IScoped
{
    private readonly MooRepository<SysPortPage> _pages;
    private readonly MooRepository<SysPortal> _portals;

    public SysPortPageService(MooRepository<SysPortPage> pages, MooRepository<SysPortal> portals)
    {
        _pages = pages;
        _portals = portals;
    }

    public PagedDataTable GetPage(PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(PortalSchema.TablePortPage)
            .where("(Spp_Deleted is null or Spp_Deleted=0)");
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            kit.whereLike("PP_Title", query.Keyword);
        var page = kit.setPage(pageSize, pageNum).orderBy("Spp_CreateTime desc").queryPaged();
        PortalRowAlias.Alias(page.Items, PortalRowAlias.PortPage);
        return page;
    }

    public DataRow? GetById(Guid id)
    {
        var row = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("*").from(PortalSchema.TablePortPage).where("Spp_Id", id).queryRow();
        return PortalRowAlias.Alias(row, PortalRowAlias.PortPage);
    }

    public void SaveBatch(IReadOnlyList<SysPortPage> rows)
    {
        var now = DateTime.Now;
        foreach (var row in rows)
        {
            if (row.Id == Guid.Empty) row.Id = Guid.CreateVersion7();
            row.UpdateTime = now;
            row.CreateTime ??= now;
            row.Deleted ??= false;
            var exists = _pages.Repo.GetById(row.Id);
            if (exists is null)
            {
                _pages.Repo.Insert(row);
                EnsureTwinPortal(row);
            }
            else
                _pages.Repo.Update(row);
        }
    }

    public void Delete(Guid id)
    {
        _pages.Repo.DeleteById(id);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.setTable(PortalSchema.TablePortal).where("Sp_PortPageId", id).doDelete();
    }

    private void EnsureTwinPortal(SysPortPage page)
    {
        var portal = new SysPortal
        {
            Id = page.Id,
            PortPageId = page.Id,
            OrgId = page.OrgId,
            Name = page.Title,
            Key = page.Code,
            IsOn = page.IsOn ?? true,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Deleted = false,
            Pulic = "1"
        };
        if (_portals.Repo.GetById(portal.Id) is null)
            _portals.Repo.Insert(portal);
    }
}
