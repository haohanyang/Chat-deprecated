namespace Chat.Areas.Api.Services;

public interface IConnectionService
{
    public ICollection<string> AddConnection(string username, string connectionId);
    public ICollection<string> RemoveConnection(string username, string connectionId);

    public ICollection<string> GetConnections(string username);
}