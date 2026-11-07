using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.Bootstrap.Seed;
using SleveenAdmin.System.Org.DBLayer;
using SleveenAdmin.System.Org.Dtos;

namespace SleveenAdmin.System.Org.Service;

public sealed class OrgService : IScoped
{
    private readonly MooRepository<SysOrg> _orgs;

    public OrgService(MooRepository<SysOrg> orgs) => _orgs = orgs;

    public List<SysOrgDto> GetTree()
    {
        var all = _orgs.Repo.GetList().OrderBy(o => o.OrderNo).ToList();
        return BuildOrgTree(all, null);
    }

    public SysOrgDto? GetById(Guid id)
    {
        var org = _orgs.Repo.GetById(id);
        return org is null ? null : ToDto(org);
    }

    public Guid Create(SysOrgInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("组织名称不能为空");

        var id = Guid.CreateVersion7();
        _orgs.Repo.Insert(new SysOrg
        {
            Id = id,
            Pid = input.Pid,
            Name = input.Name.Trim(),
            Code = input.Code,
            OrderNo = input.OrderNo,
            CreateTime = DateTime.Now
        });
        return id;
    }

    public void Update(SysOrgInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var org = _orgs.Repo.GetById(input.Id.Value)
                  ?? throw new InvalidOperationException("组织不存在");

        if (input.Pid == org.Id)
            throw new InvalidOperationException("上级组织不能是自己");

        org.Pid = input.Pid;
        org.Name = input.Name.Trim();
        org.Code = input.Code;
        org.OrderNo = input.OrderNo;
        _orgs.Repo.Update(org);
    }

    public void Delete(Guid id)
    {
        if (id == SeedIds.RootOrgId)
            throw new InvalidOperationException("不能删除根组织");

        var children = _orgs.Repo.GetList(o => o.Pid == id);
        if (children.Count > 0)
            throw new InvalidOperationException("请先删除下级组织");

        if (!_orgs.Repo.DeleteById(id))
            throw new InvalidOperationException("组织不存在");
    }

    private static List<SysOrgDto> BuildOrgTree(List<SysOrg> all, Guid? pid)
    {
        return all.Where(o => o.Pid == pid)
            .Select(o =>
            {
                var dto = ToDto(o);
                dto.Children = BuildOrgTree(all, o.Id);
                return dto;
            }).ToList();
    }

    private static SysOrgDto ToDto(SysOrg org) => new()
    {
        Id = org.Id,
        Pid = org.Pid,
        Name = org.Name,
        Code = org.Code,
        ClassCode = org.ClassCode,
        OrderNo = org.OrderNo,
        CreateTime = org.CreateTime
    };
}
