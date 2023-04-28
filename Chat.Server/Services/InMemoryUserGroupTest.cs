namespace Chat.Server.Services;

// For test only
public class InMemoryUserGroupService : IUserGroupService
{
    // groups -> members
    private Dictionary<string, HashSet<string>> _groups = new Dictionary<string, HashSet<string>>();
    private static readonly object Lock = new();

    public async Task CreateGroup(string groupName)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        lock (Lock)
        {
            if (_groups.ContainsKey(groupName))
                throw new ArgumentException("Group " + groupName + " already exists");
            _groups.Add(groupName,new HashSet<string>());
        }
        
    }

    public void AddGroupMembers(string groupName, List<string> membersToAdd)
    {
        lock (Lock)
        {
            if (_groups.TryGetValue(groupName, out var members))
            {
                lock (members)
                {
                    foreach (var member in membersToAdd)
                    {
                        members.Add(member);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Group " + groupName + " doesn't exist");
            }
        }
    }
    public async Task LeaveGroup(string username, string groupName)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        lock (Lock)
        {
            if (_groups.TryGetValue(groupName, out var members))
            {
                lock (members)
                {
                    if (!members.Remove(username))
                    {
                        throw new ArgumentException("User " + username + " is not in the group " + groupName);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Group " + groupName + " doesn't exist");
            }
        }
    }

    public async Task JoinGroup(string username, string groupName)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        lock (Lock)
        {
            if (_groups.TryGetValue(groupName, out var members))
            {

                lock (members)
                {
                    if (!members.Add(username))
                    {
                        throw new ArgumentException("User " + username + " is already in the group " + groupName);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Group " + groupName + " doesn't exist");
            }
        }
    }

    public async Task<IEnumerable<string>> GetJoinedGroups(string username)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        var groups = new List<string>();
        lock (Lock)
        {
            foreach (var kv in _groups)
            {
                if (kv.Value.Contains(username))
                {
                    groups.Add(kv.Key);
                }
            }
            return groups;
        }
    }

    public async Task<IEnumerable<string>> GetGroupMembers(string groupName)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        lock (Lock)
        {
            if (_groups.TryGetValue(groupName, out var members))
            {
                return members;
            }
            throw new ArgumentException("Group " + groupName + " doesn't exist");
        }
    }
}