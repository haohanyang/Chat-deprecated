namespace Chat.Server.Services;

public interface IConnectionService
{
    public void AddConnection(string username, string connectionId);
    public void RemoveConnection(string username, string connectionId);
}

public class ConnectionService : IConnectionService
{
    private readonly Dictionary<string, HashSet<string>> _connections = new();

    public void AddConnection(string username, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(username, out var connections))
            {
                connections = new HashSet<string>();
                _connections.Add(username, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public void RemoveConnection(string username, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(username, out var connections)) return;

            lock (connections)
            {
                connections.Remove(connectionId);

                if (connections.Count == 0) _connections.Remove(username);
            }
        }
    }
}