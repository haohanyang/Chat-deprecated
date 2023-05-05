using Chat.Common.DTOs;
namespace Chat.Areas.Api.Services;

public interface IUserGroupService
{
    public Task<int> CreateGroup(string groupName);
    public Task LeaveGroup(string username, string groupName);
    public Task<int> JoinGroup(string username, string groupName);
    public Task<IEnumerable<string>> GetJoinedGroups(string username);
    public Task<IEnumerable<string>> GetGroupMembers(string groupName);
    public Task<IEnumerable<UserDTO>> GetAllUsers();
    public Task<bool> UserExists(string username);
    public Task<bool> GroupExists(string groupName);
}