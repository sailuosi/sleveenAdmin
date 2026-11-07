using System.Data;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Business.Portal.Dtos;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Portal.Service;

/// <summary>门户角色、链接、页面选择/字段/关联等辅助表 CRUD。</summary>
public sealed class SysPortalAuxService : IScoped
{
    private readonly MooRepository<SysPortalRole> _roles;
    private readonly MooRepository<SysPortalLink> _links;
    private readonly MooRepository<SysPageSelect> _selects;
    private readonly MooRepository<SysPageField> _fields;
    private readonly MooRepository<SysPageConnect> _connects;

    public SysPortalAuxService(
        MooRepository<SysPortalRole> roles,
        MooRepository<SysPortalLink> links,
        MooRepository<SysPageSelect> selects,
        MooRepository<SysPageField> fields,
        MooRepository<SysPageConnect> connects)
    {
        _roles = roles;
        _links = links;
        _selects = selects;
        _fields = fields;
        _connects = connects;
    }

    public PagedDataTable GetRolePage(PortalPageQuery q) => QueryPage(PortalSchema.TablePortalRole, "Spr_Deleted", q);
    public PagedDataTable GetLinkPage(PortalPageQuery q) => QueryPage(PortalSchema.TablePortalLink, "Spl_Deleted", q);
    public PagedDataTable GetSelectPage(PortalPageQuery q) => QueryPage(PortalSchema.TablePageSelect, "Sps_Deleted", q);
    public PagedDataTable GetFieldPage(PortalPageQuery q) => QueryPage(PortalSchema.TablePageField, "Spf_Deleted", q);
    public PagedDataTable GetConnectPage(PortalPageQuery q) => QueryPage(PortalSchema.TablePageConnect, "Spcn_Deleted", q);

    public void SaveRoles(IReadOnlyList<SysPortalRole> rows) => SaveBatch(_roles, rows);
    public void SaveLinks(IReadOnlyList<SysPortalLink> rows) => SaveBatch(_links, rows);
    public void SaveSelects(IReadOnlyList<SysPageSelect> rows) => SaveBatch(_selects, rows);
    public void SaveFields(IReadOnlyList<SysPageField> rows) => SaveBatch(_fields, rows);
    public void SaveConnects(IReadOnlyList<SysPageConnect> rows) => SaveBatch(_connects, rows);

    public void DeleteRole(Guid id) => _roles.Repo.DeleteById(id);
    public void DeleteLink(Guid id) => _links.Repo.DeleteById(id);
    public void DeleteSelect(Guid id) => _selects.Repo.DeleteById(id);
    public void DeleteField(Guid id) => _fields.Repo.DeleteById(id);
    public void DeleteConnect(Guid id) => _connects.Repo.DeleteById(id);

    public DataRow? GetRole(Guid id) => Get(PortalSchema.TablePortalRole, "Spr_Id", id);
    public DataRow? GetLink(Guid id) => Get(PortalSchema.TablePortalLink, "Spl_Id", id);
    public DataRow? GetSelect(Guid id) => Get(PortalSchema.TablePageSelect, "Sps_Id", id);
    public DataRow? GetField(Guid id) => Get(PortalSchema.TablePageField, "Spf_Id", id);
    public DataRow? GetConnect(Guid id) => Get(PortalSchema.TablePageConnect, "Spcn_Id", id);

    private static PagedDataTable QueryPage(string table, string deletedCol, PortalPageQuery query)
    {
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var pageNum = query.Page <= 0 ? 1 : query.Page;
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        kit.select("*").from(table).where($"({deletedCol} is null or {deletedCol}=0)");
        return kit.setPage(pageSize, pageNum).queryPaged();
    }

    private static DataRow? Get(string table, string idCol, Guid id) =>
        DBCash.useSQL(MooSqlSetup.DefaultPosition).select("*").from(table).where(idCol, id).queryRow();

    private static void SaveBatch<T>(MooRepository<T> repo, IReadOnlyList<T> rows) where T : class, new()
    {
        foreach (var row in rows)
        {
            var idProp = typeof(T).GetProperty("Id")!;
            var id = (Guid)idProp.GetValue(row)!;
            if (id == Guid.Empty) idProp.SetValue(row, Guid.CreateVersion7());
            var updateProp = typeof(T).GetProperty("UpdateTime");
            updateProp?.SetValue(row, DateTime.Now);
            var createProp = typeof(T).GetProperty("CreateTime");
            if (createProp?.GetValue(row) is null) createProp.SetValue(row, DateTime.Now);
            var deletedProp = typeof(T).GetProperty("Deleted");
            if (deletedProp?.GetValue(row) is null) deletedProp?.SetValue(row, false);
            var currentId = (Guid)idProp.GetValue(row)!;
            if (repo.Repo.GetById(currentId) is null) repo.Repo.Insert(row);
            else repo.Repo.Update(row);
        }
    }
}
