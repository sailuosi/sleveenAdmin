using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Tenant;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.Notice.Constants;
using SleveenAdmin.System.Notice.DBLayer;
using SleveenAdmin.System.Notice.Dtos;
using SleveenAdmin.System.User.DBLayer;

namespace SleveenAdmin.System.Notice.Service;

public sealed class NoticeService : IScoped
{
    private readonly MooRepository<SysNotice> _notices;
    private readonly MooRepository<SysNoticeUser> _noticeUsers;
    private readonly MooRepository<SysUser> _users;
    private readonly ITenantContext _tenant;

    public NoticeService(
        MooRepository<SysNotice> notices,
        MooRepository<SysNoticeUser> noticeUsers,
        MooRepository<SysUser> users,
        ITenantContext tenant)
    {
        _notices = notices;
        _noticeUsers = noticeUsers;
        _users = users;
        _tenant = tenant;
    }

    public PagedResult<SysNoticeDto> GetPage(PageQuery query)
    {
        var list = FilterByTenant(_notices.Repo.GetList());
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(n => n.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        list = list.OrderByDescending(n => n.CreateTime).ToList();
        var targetMap = BuildTargetMap(list.Select(n => n.Id).ToList());
        return PageHelper.Apply(list.Select(n => ToDto(n, targetMap)).ToList(), query);
    }

    public PagedResult<MyNoticeDto> GetMyPage(PageQuery query)
    {
        var userId = _tenant.UserId ?? throw new InvalidOperationException("未登录");
        var noticeUsers = _noticeUsers.Repo.GetList(nu => nu.UserId == userId);
        var noticeIds = noticeUsers.Select(nu => nu.NoticeId).ToHashSet();

        var published = FilterByTenant(_notices.Repo.GetList(n =>
            n.Status == NoticeStatus.Published &&
            (n.Type == NoticeTypes.Announcement || noticeIds.Contains(n.Id))));

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            published = published.Where(n => n.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var readMap = noticeUsers.ToDictionary(nu => nu.NoticeId, nu => nu);
        var dtos = published.OrderByDescending(n => n.PublishTime ?? n.CreateTime)
            .Select(n =>
            {
                readMap.TryGetValue(n.Id, out var nu);
                var isRead = n.Type == NoticeTypes.Announcement
                    ? nu?.IsRead == true
                    : nu?.IsRead == true;
                return new MyNoticeDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    Type = n.Type,
                    IsRead = isRead,
                    PublishTime = n.PublishTime,
                    CreateTime = n.CreateTime
                };
            }).ToList();

        return PageHelper.Apply(dtos, query);
    }

    public int GetUnreadCount()
    {
        var userId = _tenant.UserId ?? throw new InvalidOperationException("未登录");
        return _noticeUsers.Repo.Count(nu => nu.UserId == userId && !nu.IsRead);
    }

    public Guid Create(SysNoticeInput input)
    {
        var userId = _tenant.UserId ?? throw new InvalidOperationException("未登录");
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("标题不能为空");

        var id = Guid.CreateVersion7();
        var now = DateTime.Now;
        var notice = new SysNotice
        {
            Id = id,
            Title = input.Title.Trim(),
            Content = input.Content ?? string.Empty,
            Type = input.Type,
            Status = NoticeStatus.Draft,
            TenantId = _tenant.IsSuperAdmin ? null : _tenant.TenantId,
            CreateUserId = userId,
            CreateTime = now
        };
        _notices.Repo.Insert(notice);

        if (input.Type == NoticeTypes.Message && input.TargetUserIds is { Count: > 0 })
            CreateNoticeUsers(id, input.TargetUserIds);

        return id;
    }

    public void Publish(Guid id)
    {
        var notice = _notices.Repo.GetById(id) ?? throw new InvalidOperationException("通知不存在");
        EnsureTenantAccess(notice.TenantId);

        notice.Status = NoticeStatus.Published;
        notice.PublishTime = DateTime.Now;
        _notices.Repo.Update(notice);

        if (notice.Type == NoticeTypes.Announcement)
            EnsureAnnouncementReceivers(notice);
    }

    public void MarkRead(Guid noticeId)
    {
        var userId = _tenant.UserId ?? throw new InvalidOperationException("未登录");
        var notice = _notices.Repo.GetById(noticeId) ?? throw new InvalidOperationException("通知不存在");

        var existing = _noticeUsers.Repo.GetFirst(nu => nu.NoticeId == noticeId && nu.UserId == userId);
        if (existing is not null)
        {
            if (!existing.IsRead)
            {
                existing.IsRead = true;
                existing.ReadTime = DateTime.Now;
                _noticeUsers.Repo.Update(existing);
            }

            return;
        }

        _noticeUsers.Repo.Insert(new SysNoticeUser
        {
            Id = Guid.CreateVersion7(),
            NoticeId = noticeId,
            UserId = userId,
            IsRead = true,
            ReadTime = DateTime.Now
        });
    }

    public void Delete(Guid id)
    {
        var notice = _notices.Repo.GetById(id) ?? throw new InvalidOperationException("通知不存在");
        EnsureTenantAccess(notice.TenantId);

        foreach (var nu in _noticeUsers.Repo.GetList(x => x.NoticeId == id))
            _noticeUsers.Repo.Delete(nu);

        _notices.Repo.Delete(notice);
    }

    private void EnsureAnnouncementReceivers(SysNotice notice)
    {
        var users = FilterUsersByTenant(_users.Repo.GetList());
        foreach (var user in users)
        {
            if (_noticeUsers.Repo.GetFirst(nu => nu.NoticeId == notice.Id && nu.UserId == user.Id) is not null)
                continue;

            _noticeUsers.Repo.Insert(new SysNoticeUser
            {
                Id = Guid.CreateVersion7(),
                NoticeId = notice.Id,
                UserId = user.Id,
                IsRead = false
            });
        }
    }

    private void CreateNoticeUsers(Guid noticeId, IReadOnlyList<Guid> userIds)
    {
        foreach (var userId in userIds.Distinct())
        {
            _noticeUsers.Repo.Insert(new SysNoticeUser
            {
                Id = Guid.CreateVersion7(),
                NoticeId = noticeId,
                UserId = userId,
                IsRead = false
            });
        }
    }

    private List<SysNotice> FilterByTenant(List<SysNotice> list)
    {
        if (_tenant.IsSuperAdmin)
            return list;

        return list.Where(n => n.TenantId == _tenant.TenantId).ToList();
    }

    private List<SysUser> FilterUsersByTenant(List<SysUser> list)
    {
        if (_tenant.IsSuperAdmin)
            return list;

        return list.Where(u => u.TenantId == _tenant.TenantId).ToList();
    }

    private void EnsureTenantAccess(Guid? tenantId)
    {
        if (_tenant.IsSuperAdmin)
            return;

        if (tenantId != _tenant.TenantId)
            throw new InvalidOperationException("无权操作该租户数据");
    }

    private Dictionary<Guid, List<Guid>> BuildTargetMap(IReadOnlyList<Guid> noticeIds)
    {
        if (noticeIds.Count == 0)
            return [];

        return _noticeUsers.Repo.GetList(nu => noticeIds.Contains(nu.NoticeId))
            .GroupBy(nu => nu.NoticeId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).ToList());
    }

    private static SysNoticeDto ToDto(SysNotice notice, Dictionary<Guid, List<Guid>> targetMap)
    {
        targetMap.TryGetValue(notice.Id, out var targets);
        return new SysNoticeDto
        {
            Id = notice.Id,
            Title = notice.Title,
            Content = notice.Content,
            Type = notice.Type,
            Status = notice.Status,
            TenantId = notice.TenantId,
            CreateUserId = notice.CreateUserId,
            CreateTime = notice.CreateTime,
            PublishTime = notice.PublishTime,
            TargetUserIds = targets ?? []
        };
    }
}
