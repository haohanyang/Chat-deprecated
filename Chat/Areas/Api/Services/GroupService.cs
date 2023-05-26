using Microsoft.EntityFrameworkCore;
using Chat.Areas.Api.Data;
using Chat.Areas.Api.Models;
using Chat.Common.Dtos;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;

public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _dbContext;




    public GroupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GroupDto> CreateGroup(string username, string groupName)
    {
        if (groupName.Length is > 20 or < 4)
            throw new ArgumentException("Group name must be between 4 and 20 characters");

        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        if (user == null)
        {
            throw new ArgumentException("User " + username + " doesn't exists");
        }

        var group = new Group
        {
            Name = groupName,
            Creator = user,
            Avatar = "https://api.dicebear.com/6.x/initials/svg?seed=" + groupName
        };
        await _dbContext.Groups.AddAsync(group);
        var membership = new Membership { User = user, Group = group };
        await _dbContext.Memberships.AddAsync(membership);
        await _dbContext.SaveChangesAsync();
        return group.ToDto();
    }

    public async Task<MembershipDto> AddMember(string username, int groupId)
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

        var membership = new Membership { User = user, Group = group, CreatedAt = DateTime.Now };

        await _dbContext.AddAsync(membership);
        await _dbContext.SaveChangesAsync();
        return new MembershipDto { Group = group.ToDto(), Member = user.ToDto(), Id = membership.Id };
    }

    public async Task RemoveMember(string username, int groupId)
    {

        var user = await _dbContext.Users.Include(e => e.Memberships).FirstOrDefaultAsync(e => e.UserName == username);
        var group = await _dbContext.Groups.Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == groupId);

        if (user == null)
            throw new ArgumentException($"User {username} doesn't exist");

        if (group == null)
            throw new ArgumentException($"Group {groupId} doesn't exist");

        if (!UserInGroup(user, group))
            throw new ArgumentException($"User {username} is not in group {groupId}");

        var membership = await _dbContext.Memberships.FirstOrDefaultAsync(e => e.UserId == user.Id && e.GroupId == group.Id);
        _dbContext.Remove(membership!);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<IEnumerable<UserDto>?> GetGroupMembers(int groupId)
    {
        var group = await _dbContext.Groups.Include(e => e.Memberships).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == groupId);

        return group?.Memberships.Select(e => e.User.ToDto());
    }

    public async Task<IEnumerable<GroupDto>?> GetJoinedGroups(string username)
    {

        var user = await _dbContext.Users.Include(e => e.Memberships).ThenInclude(e => e.Group)
            .FirstOrDefaultAsync(e => e.UserName == username);
        return user?.Memberships.Select(e => e.Group.ToDto());
    }

    private bool UserInGroup(User user, Group group)
    {
        var groupId = group.Id;
        return (
            from membership in user.Memberships
            where membership.GroupId == groupId || membership.UserId == user.Id
            select membership.Id).Any();
    }


    public async Task<GroupDto?> GetGroup(int groupId)
    {
        var group = await _dbContext.Groups.Include(e => e.Memberships).ThenInclude(e => e.User).FirstOrDefaultAsync(e => e.Id == groupId);
        if (group == null)
            return null;
        return group.ToDto();
    }

    public async Task<IEnumerable<GroupDto>> GetAllGroups()
    {
        // Caution: The members of the group are not included
        var groups = await _dbContext.Groups.Include(e => e.Creator).ToListAsync();
        return groups.Select(group => group.ToDto());
    }
}