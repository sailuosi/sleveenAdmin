using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.System.Online.Dtos;
using SleveenAdmin.System.Online.Service;
using SleveenAdmin.System.User.DBLayer;

namespace SleveenAdmin.System.Online.Hubs;

[Authorize]
public sealed class OnlineUserHub : Hub
{
    private readonly OnlineUserTracker _tracker;
    private readonly MooRepository<SysUser> _users;

    public OnlineUserHub(OnlineUserTracker tracker, MooRepository<SysUser> users)
    {
        _tracker = tracker;
        _users = users;
    }

    public override Task OnConnectedAsync()
    {
        var userId = CurrentUser.GetUserId(Context.User);
        if (userId is null)
            return base.OnConnectedAsync();

        var user = _users.Repo.GetById(userId.Value);
        if (user is null)
            return base.OnConnectedAsync();

        var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        _tracker.Add(new OnlineUserDto
        {
            ConnectionId = Context.ConnectionId,
            UserId = user.Id,
            Account = user.Account,
            RealName = user.RealName,
            Ip = ip,
            ConnectedAt = DateTime.Now
        });

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _tracker.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
