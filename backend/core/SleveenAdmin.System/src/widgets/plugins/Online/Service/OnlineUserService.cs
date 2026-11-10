using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.Online.Dtos;
using SleveenAdmin.System.Online.Service;

namespace SleveenAdmin.System.Online.Service;

[Scoped]
public sealed class OnlineUserService {
    private readonly OnlineUserTracker _tracker;

    public OnlineUserService(OnlineUserTracker tracker) => _tracker = tracker;

    public IReadOnlyList<OnlineUserDto> GetList() => _tracker.GetAll();

    public int GetCount() => _tracker.Count;
}
