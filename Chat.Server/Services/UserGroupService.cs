using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Services;

public interface IUserGroupService
{
    public Task CreateGroup(string groupName);
    public Task LeaveGroup(string username, string groupName);
    public Task JoinGroup(string username, string groupName);
    public Task<IEnumerable<string>> GetJoinedGroups(string username);
    public Task<IEnumerable<string>> GetGroupMembers(string groupName);
}

public class UserGroupService : IUserGroupService
{
    private readonly ApplicationDbContext _dbContext;

    private readonly ILogger<UserGroupService> _logger;


    public UserGroupService(ApplicationDbContext dbContext, ILogger<UserGroupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task CreateGroup(string groupName)
    {
        if (await _dbContext.Groups.FirstOrDefaultAsync(e => e.GroupName == groupName) != null)
        {
            throw new ArgumentException("Group " + groupName + " already exists");
        }
        await _dbContext.Groups.AddAsync(new Group { GroupName = groupName });
        await _dbContext.SaveChangesAsync();
    }

    public async Task JoinGroup(string username, string groupName)
    {
        var user = await _dbContext.Users.Include(e => e.Memberships).FirstOrDefaultAsync(e => e.UserName == username);
        var group = await _dbContext.Groups.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.GroupName == groupName);

        if (user == null)
            throw new ArgumentException("User " + username + " doesn't exist");

        if (group == null)
            throw new ArgumentException("Group " + groupName + " doesn't exist");

        if (UserInGroup(user, group))
            throw new ArgumentException("User " + username + " is already in group " + groupName);

        await _dbContext.AddAsync(new Membership { User = user, Group = group });
        await _dbContext.SaveChangesAsync();
    }

    public async Task LeaveGroup(string username, string groupName)
    {
        
        var user = await _dbContext.Users.Include(e => e.Memberships).FirstOrDefaultAsync(e => e.UserName == username);
        var group = await _dbContext.Groups.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.GroupName == groupName);
        
        if (user == null)
            throw new ArgumentException("User " + username + " doesn't exist");

        if (group == null)
            throw new ArgumentException("Group " + groupName + " doesn't exist");

        if (!UserInGroup(user, group))
            throw new ArgumentException("User " + username + " is not in group " + groupName);

        var membership =  await _dbContext.Memberships.FirstOrDefaultAsync(e => e.UserId == user.Id && e.GroupId == group.Id);
        _dbContext.Remove(membership!);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<IEnumerable<string>> GetGroupMembers(string groupName)
    {
        var group = await _dbContext.Groups.Include(e => e.Memberships).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(e => e.GroupName == groupName);
        if (group == null)
            throw new ArgumentException("Group " + groupName + " doesn't exist");

        return group.Memberships.Select(e => e.User.UserName!);
    }

    public async Task<IEnumerable<string>> GetJoinedGroups(string username)
    {
        var user = await _dbContext.Users.Include(e => e.Memberships).ThenInclude(m => m.Group)
            .FirstOrDefaultAsync(e => e.UserName == username);
        if (user == null)
            throw new ArgumentException("User " + user.UserName + " doesn't exist");

        return user.Memberships.Select(e => e.Group.GroupName);
    }

    private bool UserInGroup(User user, Group group)
    {
        var groupId = group.Id;
        return (
            from membership in user.Memberships
            where membership.GroupId == groupId
            select membership.Id).Any();
    }
}