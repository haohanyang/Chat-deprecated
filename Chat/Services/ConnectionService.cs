using System.Collections.Concurrent;
using Chat.Services.Interface;

namespace Chat.Services;


public class ConnectionService : IConnectionService
{

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = new();
    public ICollection<string> AddConnection(string username, string connectionId)
    {
        var connections = _connections.AddOrUpdate(username,
            _ => new ConcurrentDictionary<string, byte>(new[] { new KeyValuePair<string, byte>(connectionId, 0) }),
            (_, oldValue) =>
            {
                oldValue.TryAdd(connectionId, 0);
                return oldValue;
            }).Keys;
        return connections;
    }
    public ICollection<string> RemoveConnection(string username, string connectionId)
    {
        var connections = _connections.AddOrUpdate(username,
            _ => new ConcurrentDictionary<string, byte>(),
            (_, oldValue) =>
            {
                oldValue.TryRemove(connectionId, out var _);
                return oldValue;
            }).Keys;
        return connections;
    }

    public ICollection<string> GetConnections(string username)
    {
        _connections.TryGetValue(username, out var connections);
        if (connections == null)
        {
            return new HashSet<string>();
        }
        return connections.Keys;
    }
}