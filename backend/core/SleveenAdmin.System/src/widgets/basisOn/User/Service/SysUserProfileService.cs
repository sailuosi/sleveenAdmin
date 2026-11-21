using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.User.DBLayer;
using SleveenAdmin.System.User.Dtos;

namespace SleveenAdmin.System.User.Service;

[Scoped]
public sealed class SysUserProfileService
{
    private readonly MooRepository<SysUser> _users;

    public SysUserProfileService(MooRepository<SysUser> users) => _users = users;

    public SysUserBaseInfoDto? GetBaseInfo(Guid userId)
    {
        var user = _users.Repo.GetById(userId);
        return user is null ? null : ToDto(user);
    }

    public SysUserBaseInfoDto UpdateBaseInfo(Guid userId, SysUserBaseInfoDto input)
    {
        var user = _users.Repo.GetById(userId)
                   ?? throw new InvalidOperationException("用户不存在");

        user.RealName = input.RealName?.Trim();
        user.NickName = input.NickName?.Trim();
        user.Phone = input.Phone?.Trim();
        user.Email = input.Email?.Trim();
        user.Sex = input.Sex;
        user.Birthday = input.Birthday;
        user.Address = input.Address?.Trim();
        user.Remark = input.Remark?.Trim();
        // Avatar / Signature 由上传接口写入，此处允许显式清空或保留
        if (input.Avatar is not null)
            user.Avatar = string.IsNullOrWhiteSpace(input.Avatar) ? null : input.Avatar.Trim();
        if (input.Signature is not null)
            user.Signature = string.IsNullOrWhiteSpace(input.Signature) ? null : input.Signature.Trim();
        user.UpdateTime = DateTime.Now;

        if (!_users.Repo.Update(user))
            throw new InvalidOperationException("更新用户信息失败");

        return ToDto(user);
    }

    public void SetSignature(Guid userId, string url)
    {
        var user = _users.Repo.GetById(userId)
                   ?? throw new InvalidOperationException("用户不存在");
        user.Signature = url;
        user.UpdateTime = DateTime.Now;
        if (!_users.Repo.Update(user))
            throw new InvalidOperationException("更新电子签名失败");
    }

    public void SetAvatar(Guid userId, string url)
    {
        var user = _users.Repo.GetById(userId)
                   ?? throw new InvalidOperationException("用户不存在");
        user.Avatar = url;
        user.UpdateTime = DateTime.Now;
        if (!_users.Repo.Update(user))
            throw new InvalidOperationException("更新头像失败");
    }

    private static SysUserBaseInfoDto ToDto(SysUser user) => new()
    {
        Id = user.Id,
        Account = user.Account,
        RealName = user.RealName,
        NickName = user.NickName,
        Avatar = user.Avatar,
        Signature = user.Signature,
        Phone = user.Phone,
        Email = user.Email,
        Sex = user.Sex,
        Birthday = user.Birthday,
        Address = user.Address,
        Remark = user.Remark
    };
}
