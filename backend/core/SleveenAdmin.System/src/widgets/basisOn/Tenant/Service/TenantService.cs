using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.Tenant.DBLayer;
using SleveenAdmin.System.Tenant.Dtos;

namespace SleveenAdmin.System.Tenant.Service;

public sealed class TenantService : IScoped
{
    private readonly MooRepository<SysTenant> _tenants;

    public TenantService(MooRepository<SysTenant> tenants) => _tenants = tenants;

    public PagedResult<SysTenantDto> GetPage(PageQuery query)
    {
        var list = _tenants.Repo.GetList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(t =>
                t.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                t.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        list = list.OrderByDescending(t => t.CreateTime).ToList();
        return PageHelper.Apply(list.Select(ToDto).ToList(), query);
    }

    public List<SysTenantDto> GetList() =>
        _tenants.Repo.GetList().OrderBy(t => t.Name).Select(ToDto).ToList();

    public SysTenantDto? GetById(Guid id)
    {
        var tenant = _tenants.Repo.GetById(id);
        return tenant is null ? null : ToDto(tenant);
    }

    public Guid Create(SysTenantInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("租户名称不能为空");
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new ArgumentException("租户编码不能为空");
        if (_tenants.Repo.GetFirst(t => t.Code == input.Code.Trim()) is not null)
            throw new InvalidOperationException("租户编码已存在");

        var id = Guid.CreateVersion7();
        _tenants.Repo.Insert(new SysTenant
        {
            Id = id,
            Name = input.Name.Trim(),
            Code = input.Code.Trim(),
            Status = input.Status,
            Remark = input.Remark,
            CreateTime = DateTime.Now
        });
        return id;
    }

    public void Update(SysTenantInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var tenant = _tenants.Repo.GetById(input.Id.Value)
                     ?? throw new InvalidOperationException("租户不存在");

        if (!string.IsNullOrWhiteSpace(input.Code) && input.Code != tenant.Code)
        {
            if (_tenants.Repo.GetFirst(t => t.Code == input.Code && t.Id != tenant.Id) is not null)
                throw new InvalidOperationException("租户编码已存在");
            tenant.Code = input.Code.Trim();
        }

        tenant.Name = input.Name.Trim();
        tenant.Status = input.Status;
        tenant.Remark = input.Remark;
        _tenants.Repo.Update(tenant);
    }

    public void Delete(Guid id) =>
        _ = _tenants.Repo.DeleteById(id) ? true : throw new InvalidOperationException("租户不存在");

    private static SysTenantDto ToDto(SysTenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Code = tenant.Code,
        Status = tenant.Status,
        Remark = tenant.Remark,
        CreateTime = tenant.CreateTime
    };
}
