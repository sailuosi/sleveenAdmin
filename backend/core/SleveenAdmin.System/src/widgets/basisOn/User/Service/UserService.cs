using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Security;
using SleveenAdmin.Foundation.Tenant;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.Bootstrap.Seed;
using SleveenAdmin.System.User.DBLayer;
using SleveenAdmin.System.User.Dtos;

namespace SleveenAdmin.System.User.Service;

public sealed class UserService : IScoped
{
    private readonly MooRepository<SysUser> _users;
    private readonly ITenantContext _tenant;

    public UserService(
        MooRepository<SysUser> users,
        ITenantContext tenant)
    {
        _users = users;
        _tenant = tenant;
    }

    public PagedResult<SysUserDto> GetPage(PageQuery query)
    {
        var list = FilterByTenant(_users.Repo.GetList());
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(u =>
                u.Account.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                (u.RealName != null && u.RealName.Contains(kw, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        list = list.OrderByDescending(u => u.CreateTime).ToList();
        var dtos = list.Select(ToDto).ToList();
        return PageHelper.Apply(dtos, query);
    }

    public SysUserDto? GetById(Guid id)
    {
        var user = _users.Repo.GetById(id);
        if (user is null || !CanAccessUser(user))
            return null;

        return ToDto(user);
    }

    public Guid Create(SysUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Account))
            throw new ArgumentException("账号不能为空");
        if (string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("密码不能为空");
        if (_users.Repo.GetFirst(u => u.Account == input.Account) is not null)
            throw new InvalidOperationException("账号已存在");

        var tenantId = ResolveTenantId(input.TenantId);
        var id = Guid.CreateVersion7();
        var now = DateTime.Now;
        var user = new SysUser
        {
            Id = id,
            Account = input.Account.Trim(),
            Password = PasswordHasher.Hash(input.Password),
            RealName = input.RealName,
            Status = input.Status,
            OrgId = input.OrgId,
            TenantId = tenantId,
            CreateTime = now
        };
        _users.Repo.Insert(user);
        return id;
    }

    public void Update(SysUserInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var user = _users.Repo.GetById(input.Id.Value)
                   ?? throw new InvalidOperationException("用户不存在");
        if (!CanAccessUser(user))
            throw new InvalidOperationException("无权操作该用户");

        if (!string.IsNullOrWhiteSpace(input.Account) && input.Account != user.Account)
        {
            if (_users.Repo.GetFirst(u => u.Account == input.Account && u.Id != user.Id) is not null)
                throw new InvalidOperationException("账号已存在");
            user.Account = input.Account.Trim();
        }

        if (!string.IsNullOrWhiteSpace(input.Password))
            user.Password = PasswordHasher.Hash(input.Password);

        user.RealName = input.RealName;
        user.Status = input.Status;
        user.OrgId = input.OrgId;
        if (_tenant.IsSuperAdmin)
            user.TenantId = input.TenantId;
        user.UpdateTime = DateTime.Now;
        _users.Repo.Update(user);
    }

    public void Delete(Guid id)
    {
        if (id == SeedIds.SuperAdminUserId)
            throw new InvalidOperationException("不能删除超级管理员");

        var user = _users.Repo.GetById(id) ?? throw new InvalidOperationException("用户不存在");
        if (!CanAccessUser(user))
            throw new InvalidOperationException("无权操作该用户");

        if (!_users.Repo.DeleteById(id))
            throw new InvalidOperationException("用户不存在");
    }

    private Guid? ResolveTenantId(Guid? inputTenantId)
    {
        if (_tenant.IsSuperAdmin)
            return inputTenantId;

        return _tenant.TenantId;
    }

    private bool CanAccessUser(SysUser user)
    {
        if (_tenant.IsSuperAdmin)
            return true;

        return user.TenantId == _tenant.TenantId;
    }

    private List<SysUser> FilterByTenant(List<SysUser> list)
    {
        if (_tenant.IsSuperAdmin)
            return list;

        return list.Where(u => u.TenantId == _tenant.TenantId).ToList();
    }

    private static SysUserDto ToDto(SysUser user) => new()
    {
        Id = user.Id,
        Account = user.Account,
        RealName = user.RealName,
        Status = user.Status,
        OrgId = user.OrgId,
        TenantId = user.TenantId,
        CreateTime = user.CreateTime,
        UpdateTime = user.UpdateTime
    };
}
