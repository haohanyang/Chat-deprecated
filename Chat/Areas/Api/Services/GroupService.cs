using Microsoft.EntityFrameworkCore;
using Chat.Areas.Api.Data;
using Chat.Areas.Api.Models;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;

public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _dbContext;

    private readonly ILogger<GroupService> _logger;


    public GroupService(ApplicationDbContext dbContext, ILogger<GroupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> CreateGroup(string username, string groupName)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        if (user == null)
        {
            throw new ArgumentException("User " + username + " doesn't exists");
        }

        var group = new Group
        {
            Name = groupName,
            Owner = user,
            AvatarUrl = "https://api.dicebear.com/6.x/initials/svg?seed=" + groupName
        };
        await _dbContext.Groups.AddAsync(group);
        var membership = new Membership { User = user, Group = group };
        await _dbContext.Memberships.AddAsync(membership);
        await _dbContext.SaveChangesAsync();
        return group.Id;
    }

    public async Task<int> JoinGroup(string username, int groupId)
    {
        var user = await _dbContext.Users.Include(e => e.Memberships).FirstOrDefaultAsync(e => e.UserName == username);
        var group = await _dbContext.Groups.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == groupId);

        if (user == null)
            throw new ArgumentException("User " + username + " doesn't exist");

        if (group == null)
            throw new ArgumentException("Group " + groupId + " doesn't exist");

        if (UserInGroup(user, group))
            throw new ArgumentException("User " + username + " is already in group " + groupId);

        var membership = new Membership { User = user, Group = group };

        await _dbContext.AddAsync(membership);
        await _dbContext.SaveChangesAsync();
        return membership.Id;
    }

    public async Task LeaveGroup(string username, int groupId)
    {

        var user = await _dbContext.Users.Include(e => e.Memberships).FirstOrDefaultAsync(e => e.UserName == username);
        var group = await _dbContext.Groups.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == groupId);

        if (user == null)
            throw new ArgumentException("User " + username + " doesn't exist");

        if (group == null)
            throw new ArgumentException("Group " + groupId + " doesn't exist");

        if (!UserInGroup(user, group))
            throw new ArgumentException("User " + username + " is not in group " + groupId);

        var membership = await _dbContext.Memberships.FirstOrDefaultAsync(e => e.UserId == user.Id && e.GroupId == group.Id);
        _dbContext.Remove(membership!);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<IEnumerable<UserDTO>> GetGroupMembers(int groupId)
    {
        var group = await _dbContext.Groups.Include(e => e.Memberships).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == groupId);
        if (group == null)
            throw new ArgumentException("Group " + groupId + " doesn't exist");

        return group.Memberships.Select(e => e.User.ToDto());
    }

    public async Task<IEnumerable<GroupDTO>> GetJoinedGroups(string username)
    {
        var user = await _dbContext.Users.Include(e => e.Memberships).ThenInclude(m => m.Group)
            .FirstOrDefaultAsync(e => e.UserName == username);
        if (user == null)
            throw new ArgumentException("User " + username + " doesn't exist");

        return user.Memberships.Select(e => e.Group.ToDto());
    }

    private bool UserInGroup(User user, Group group)
    {
        var groupId = group.Id;
        return (
            from membership in user.Memberships
            where membership.GroupId == groupId
            select membership.Id).Any();
    }


    public async Task<bool> GroupExists(int groupId)
    {
        var group = await _dbContext.Groups.FirstOrDefaultAsync(e => e.Id == groupId);
        return group != null;
    }

    public async Task<GroupDTO> GetGroup(int groupId)
    {
        var group = await _dbContext.Groups.Include(e => e.Owner).FirstOrDefaultAsync(e => e.Id == groupId);
        if (group == null)
            throw new ArgumentException("Group " + groupId + " doesn't exist");

        return group.ToDto();
    }

    public Task<IEnumerable<GroupDTO>> GetAllGroups()
    {
        throw new NotImplementedException();
    }
}