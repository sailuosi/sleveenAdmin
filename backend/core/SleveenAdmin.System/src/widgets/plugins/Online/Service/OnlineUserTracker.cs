using System.Collections.Concurrent;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.Online.Dtos;

namespace SleveenAdmin.System.Online.Service;

[Singleton]
public sealed class OnlineUserTracker {
    private readonly ConcurrentDictionary<string, OnlineUserDto> _sessions = new();

    public void Add(OnlineUserDto session) => _sessions[session.ConnectionId] = session;

    public void Remove(string connectionId) => _sessions.TryRemove(connectionId, out _);

    public IReadOnlyList<OnlineUserDto> GetAll() =>
        _sessions.Values.OrderByDescending(s => s.ConnectedAt).ToList();

    public int Count => _sessions.Count;
}
